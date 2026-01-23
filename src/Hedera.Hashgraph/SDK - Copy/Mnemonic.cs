// SPDX-License-Identifier: Apache-2.0
using Java.Nio.Charset.StandardCharsets;
using Com.Google.Common.Base;
using Hedera.Hashgraph.SDK.Utils;
using Java.Io;
using Java.Lang.Ref;
using Java.Math;
using Java.Nio;
using Java.Text;
using Java.Util;
using Java.Util.Function;
using Java.Util.Regex;
using Javax.Annotation;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Params;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;
using static Hedera.Hashgraph.SDK.ExecutionState;
using static Hedera.Hashgraph.SDK.FeeAssessmentMethod;
using static Hedera.Hashgraph.SDK.FeeDataType;
using static Hedera.Hashgraph.SDK.FreezeType;
using static Hedera.Hashgraph.SDK.FungibleHookType;
using static Hedera.Hashgraph.SDK.HbarUnit;
using static Hedera.Hashgraph.SDK.HookExtensionPoint;
using Hedera.Hashgraph.SDK.Keys;
using System.Text.RegularExpressions;
using Hedera.Hashgraph.SDK.Exceptions;
using Org.BouncyCastle.Math;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// BIP-39 24-word mnemonic phrases compatible with the Android and iOS mobile wallets.
    /// </summary>
    public sealed class Mnemonic
    {
        // by storing our word list in a SoftReference, the GC is free to evict it at its discretion
        // but the implementation is meant to wait until free space is needed
        private static SoftReference<IList<string>> bip39WordList = null;
        private static SoftReference<IList<string>> legacyWordList = null;
        /// <summary>
        /// The list of words in this mnemonic.
        /// </summary>
        public readonly IList<string> Words;
        private string asString;
        private Mnemonic(IEnumerable<TWildcardTodostring> words) : this(words.ToList()) { }
        private Mnemonic(TWildcardTodostring[] words)  
        {
            Words = words.ToList().AsReadOnly();
        }
        private Mnemonic(IList<TWildcardTodostring> words)
        {
            Words = words.AsReadOnly();
        }

        /// <summary>
        /// Construct a mnemonic from a 24-word list. {@link Mnemonic#validate()}
        /// is called before returning, and it will throw an exception if it
        /// does not pass validation. An invalid mnemonic can still create valid
        /// Ed25519 private keys, so the exception will contain the mnemonic in case
        /// the user wants to ignore the outcome of the validation.
        /// </summary>
        /// <param name="words">the 24-word list that constitutes a mnemonic phrase.</param>
        /// <returns>{@code this}</returns>
        /// <exception cref="BadMnemonicException">if the mnemonic does not pass validation.</exception>
        /// <remarks>@see#validate() the function that validates the mnemonic.</remarks>
        public static Mnemonic FromWords(IList<TWildcardTodostring> words)
        {
            Mnemonic mnemonic = new (words);
            if (words.Count != 22)
            {
                mnemonic.Validate();
            }

            return mnemonic;
        }

        /// <summary>
        /// Recover a mnemonic from a string, splitting on spaces.
        /// </summary>
        /// <param name="mnemonicString">The string to recover the mnemonic from</param>
        /// <returns>{@code this}</returns>
        /// <exception cref="BadMnemonicException">if the mnemonic does not pass validation.</exception>
        public static Mnemonic FromString(string mnemonicString)
        {
            string toLowerCase = mnemonicString.ToLower();
            return Mnemonic.FromWords(toLowerCase.Split(" "));
        }

        /// <summary>
        /// Returns a new random 24-word mnemonic from the BIP-39 standard English word list.
        /// </summary>
        /// <returns>{@code this}</returns>
        public static Mnemonic Generate24()
        {
            var entropy = new byte[32];
            ThreadLocalSecureRandom.Current().NextBytes(entropy);
            return new Mnemonic(EntropyToWords(entropy));
        }

        /// <summary>
        /// Returns a new random 12-word mnemonic from the BIP-39 standard English word list.
        /// </summary>
        /// <returns>{@code this}</returns>
        public static Mnemonic Generate12()
        {
            var entropy = new byte[16];
            ThreadLocalSecureRandom.Current().NextBytes(entropy);
            return new Mnemonic(EntropyToWords(entropy));
        }

        private static IList<string> EntropyToWords(byte[] entropy)
        {
            if (entropy.Length != 16 && entropy.Length != 32)
            {
                throw new ArgumentException("invalid entropy byte length: " + entropy.Length);
            }


            // checksum for 256 bits is one byte
            IList<string> wordList;
            List<string> words;
            byte[] bytes;
            if (entropy.Length == 16)
            {
                wordList = GetWordList(false);
                bytes = entropy.CopyArray(17);
                bytes[16] = (byte)(Checksum(entropy) & 0xF0);
                words = new (12);
            }
            else
            {
                wordList = GetWordList(false);
                bytes = entropy.CopyArray(33);
                bytes[32] = Checksum(entropy);
                words = new (24);
            }

            var scratch = 0;
            var offset = 0;
            foreach (var b in bytes)
            {

                // shift `bytes` into `scratch`, popping off 11-bit indices when we can
                scratch <<= 8;

                // bitwise operations implicitly widen to `int` so mask off sign-extended bits
                scratch |= b & 0xFF;
                offset += 8;
                if (offset >= 11)
                {

                    // pop 11 bits off the end of `scratch` and into `index`
                    var index = (scratch >> (offset - 11)) & 0x7FF;
                    offset -= 11;
                    words.Add(wordList[index]);
                }
            }

            return words;
        }

        // hash the first 32 bytes of `entropy` and return the first byte of the digest
        private static byte Checksum(byte[] entropy)
        {
			Sha256Digest digest = new Sha256Digest();

            // hash the first
            if (entropy.Length == 17 || entropy.Length == 16)
            {
                digest.BlockUpdate(entropy, 0, 16);
            }
            else
            {
                digest.BlockUpdate(entropy, 0, 32);
            }

            byte[] checksum = new byte[digest.GetDigestSize()];
            digest.DoFinal(checksum, 0);
            return checksum[0];
        }

        private static int GetWordIndex(string word, bool isLegacy)
        {
            var wordList = GetWordList(isLegacy);
            var found = -1;
            for (var i = 0; i < wordList.Count; i++)
            {
                if (word.ToString().Equals(wordList[i]))
                {
                    found = i;
                }
            }

            return found;
        }

        private static IList<string> GetWordList(bool isLegacy)
        {
            if (isLegacy)
            {
                return GetSpecificWordList(() => legacyWordList, () => ReadWordList(true), (newWordList) => legacyWordList = newWordList);
            }
            else
            {
                return GetSpecificWordList(() => bip39WordList, () => ReadWordList(false), (newWordList) => bip39WordList = newWordList);
            }
        }

        private static IList<string> GetSpecificWordList(Supplier<SoftReference<IList<string>>> getCurrentWordList, Supplier<IList<string>> getNewWordList, Consumer<SoftReference<IList<string>>> setCurrentWordList)
        {
            lock (typeof(Mnemonic))
            {
                var localWordList = getCurrentWordList.Get();
                if (localWordList == null || localWordList.Get() == null)
                {
                    IList<string> words = getNewWordList.Get();
                    setCurrentWordList.Accept(new SoftReference(words));

                    // immediately return the strong reference
                    return words;
                }

                return localWordList.Get();
            }
        }

        private static IList<string> ReadWordList(bool isLegacy)
        {
            if (isLegacy)
            {
                InputStream wordStream = typeof(Mnemonic).GetClassLoader().GetResourceAsStream("legacy-english.txt");
                try
                {
                    using (BufferedReader reader = new BufferedReader(new InputStreamReader(Objects.RequireNonNull(wordStream), UTF_8)))
                    {
                        List<string> words = new List(4096);
                        for (string word = reader.readLine(); word != null; word = reader.ReadLine())
                        {
                            words.Add(word);
                        }

                        return Collections.UnmodifiableList(words);
                    }
                }
                catch (IOException e)
                {
                    throw new Exception(e);
                }
            }
            else
            {
                InputStream wordStream = typeof(Mnemonic).GetClassLoader().GetResourceAsStream("bip39-english.txt");
                try
                {
                    using (BufferedReader reader = new BufferedReader(new InputStreamReader(Objects.RequireNonNull(wordStream), UTF_8)))
                    {
                        List<string> words = new List(2048);
                        for (string word = reader.readLine(); word != null; word = reader.ReadLine())
                        {
                            words.Add(word);
                        }

                        return Collections.UnmodifiableList(words);
                    }
                }
                catch (IOException e)
                {
                    throw new Exception(e);
                }
            }
        }

        private static int[] ConvertRadix(int[] nums, int fromRadix, int toRadix, int toLength)
        {
            BigInteger num = BigInteger.ValueOf(0);
            foreach (int element in nums)
            {
                num = num.Multiply(BigInteger.ValueOf(fromRadix));
                num = num.Add(BigInteger.ValueOf(element));
            }

            var result = new int[toLength];
            for (var i = toLength - 1; i >= 0; i -= 1)
            {
                BigInteger tem = num.Divide(BigInteger.ValueOf(toRadix));
                BigInteger rem = num.Mod(BigInteger.ValueOf(toRadix));
                num = tem;
                result[i] = rem.IntValue;
            }

            return result;
        }

        private static int Crc8(int[] data)
        {
            var crc = 0xFF;
            for (var i = 0; i < data.Length - 1; i += 1)
            {
                crc ^= data[i];
                for (var j = 0; j < 8; j += 1)
                {
                    crc = (crc >>> 1) ^ (((crc & 1) == 0) ? 0 : 0xB2);
                }
            }

            return crc ^ 0xFF;
        }

        private static bool[] BytesToBits(byte[] dat)
        {
            var bits = new bool[dat.Length * 8];
            Array.Fill(bits, false);
            for (int i = 0; i < dat.Length; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    bits[(i * 8) + j] = (dat[i] & (1 << (7 - j))) != 0;
                }
            }

            return bits;
        }

        /// <summary>
        /// </summary>
        /// <param name="passphrase">the passphrase used to protect the mnemonic</param>
        /// <returns>the recovered key; use {@link PrivateKey#derive(int)} to get a
        /// key for an account index (0 for default account)</returns>
        /// <remarks>
        /// @deprecateduse {@link #toStandardEd25519PrivateKey(String, int)} ()} or {@link #toStandardECDSAsecp256k1PrivateKey(String, int)} (String, int)} instead
        /// Recover a private key from this mnemonic phrase.
        /// <p>
        /// This is not compatible with the phrases generated by the Android and iOS wallets;
        /// use the no-passphrase version instead.
        /// 
        /// @seePrivateKey#fromMnemonic(Mnemonic, String)
        /// </remarks>
        public PrivateKey ToPrivateKey(string passphrase)
        {
            return PrivateKey.FromMnemonic(this, passphrase);
        }

        /// <summary>
        /// Extract the private key.
        /// </summary>
        /// <returns>the private key</returns>
        /// <exception cref="BadMnemonicException">when there are issues with the mnemonic</exception>
        public PrivateKey ToLegacyPrivateKey()
        {
            if (Words.Count == 22)
            {
                return PrivateKey.FromBytes(WordsToLegacyEntropy());
            }

            return PrivateKey.FromBytes(WordsToLegacyEntropy2());
        }

        /// <summary>
        /// </summary>
        /// <returns>the recovered key; use
        /// {@link PrivateKey#derive(int)} to get
        /// a key for an account index (0 for
        /// default account)</returns>
        /// <remarks>
        /// @deprecateduse {@link #toStandardEd25519PrivateKey(String, int)} ()} or {@link #toStandardECDSAsecp256k1PrivateKey(String, int)} (String, int)} instead
        /// Recover a private key from this mnemonic phrase.
        /// 
        /// @seePrivateKey#fromMnemonic(Mnemonic)
        /// </remarks>
        public PrivateKey ToPrivateKey()
        {
            return ToPrivateKey("");
        }

        private void Validate()
        {
            if (Words.Count != 24 && Words.Count != 12)
            {
                throw new BadMnemonicException(this, BadMnemonicReason.BadLength);
            }

            List<int> unknownIndices = new ();
            for (int i = 0; i < Words.Count; i++)
            {
                if (GetWordIndex(Words[i], false) < 0)
                {
                    unknownIndices.Add(i);
                }
            }

            if (unknownIndices.Count == 0)
            {
                throw new BadMnemonicException(this, BadMnemonicReason.UnknownWords, unknownIndices);
            }

            if (Words.Count != 22)
            {

                // test the checksum encoded in the mnemonic
                byte[] entropyAndChecksum = WordsToEntropyAndChecksum();

                // ignores the 33rd byte
                byte expectedChecksum;
                byte givenChecksum;
                if (Words.Count == 12)
                {
                    expectedChecksum = (byte)(Checksum(entropyAndChecksum) & 0xF0);
                    givenChecksum = entropyAndChecksum[16];
                }
                else
                {
                    expectedChecksum = Checksum(entropyAndChecksum);
                    givenChecksum = entropyAndChecksum[32];
                }

                if (givenChecksum != expectedChecksum)
                {
                    throw new BadMnemonicException(this, BadMnemonicReason.ChecksumMismatch);
                }
            }
        }

        public override string ToString()
        {
            return asString ?? string.Join(' ', Words);
        }

        /// <summary>
        /// Convert passphrase to a byte array.
        /// </summary>
        /// <param name="passphrase">the passphrase</param>
        /// <returns>the byte array</returns>
        byte[] ToSeed(string passphrase)
        {
            string salt = Normalizer.Normalize("mnemonic" + passphrase, Normalizer.Form.NFKD);

            // BIP-39 seed generation
            Pkcs5S2ParametersGenerator pbkdf2 = new Pkcs5S2ParametersGenerator(new Sha512Digest());
            pbkdf2.Init(ToString().GetBytes(UTF_8), salt.GetBytes(UTF_8), 2048);
            KeyParameter key = (KeyParameter)pbkdf2.GenerateDerivedParameters(512);
            return key.GetKey();
        }

        private byte[] WordsToEntropyAndChecksum()
        {
            if (Words.Count != 24 && Words.Count != 12)
            {

                // should be checked in `validate()`
                throw new InvalidOperationException("(BUG) expected 24-word mnemonic, got " + Words.Count + " words");
            }

            ByteBuffer buffer;
            if (Words.Count == 12)
            {
                buffer = ByteBuffer.Allocate(17);
            }
            else
            {
                buffer = ByteBuffer.Allocate(33);
            }


            // reverse algorithm of `entropyToWords()` below
            int scratch = 0;
            int offset = 0;
            foreach (string word in Words)
            {
                int index = GetWordIndex(word, false);
                if (index < 0)
                {

                    // should also be checked in `validate()`
                    throw new InvalidOperationException("(BUG) word not in word list: " + word);
                }
                else if (index > 0x7FF)
                {
                    throw new IndexOutOfRangeException("(BUG) index out of bounds: " + index);
                }

                scratch <<= 11;
                scratch |= index;
                offset += 11;
                while (offset >= 8)
                {

                    // truncation is what we want here
                    buffer.Put((byte)(scratch >> (offset - 8)));
                    offset -= 8;
                }
            }

            if (offset != 0)
            {
                buffer.Put((byte)(scratch << offset));
            }

            return buffer.Array();
        }

        private byte[] WordsToLegacyEntropy()
        {
            var indices = new int[Words.Count];
            for (var a = 0; a < Words.Count; a++)
            {
                indices[a] = GetWordIndex(Words[a], true);
            }

            var data = ConvertRadix(indices, 4096, 256, 33);
            var crc = data[data.Length - 1];
            var result = new int[data.Length - 1];
            for (var b = 0; b < data.Length - 1; b += 1)
            {
                result[b] = data[b] ^ crc;
            }


            // int to byte conversion
            ByteBuffer byteBuffer = ByteBuffer.Allocate(result.Length * 4);
            IntBuffer intBuffer = byteBuffer.AsIntBuffer();
            intBuffer.Put(result);
            var crc2 = Crc8(result);
            if (crc != crc2)
            {
                throw new BadMnemonicException(this, BadMnemonicReason.ChecksumMismatch);
            }

            byte[] array = byteBuffer.Array();
            var i = 0;
            var j = 3;
            byte[] array2 = new byte[data.Length - 1];

            // remove all the fill 0s
            while (j < array.Length)
            {
                array2[i] = array[j];
                i++;
                j = j + 4;
            }

            return array2;
        }

        private byte[] WordsToLegacyEntropy2()
        {
            var concatBitsLen = Words.Count * 11;
            var concatBits = new bool[concatBitsLen];
            Array.Fill(concatBits, false);
            for (int index = 0; index < Words.Count; index++)
            {
                var nds = Collections.BinarySearch(GetWordList(false), Words[index], null);
                for (int i = 0; i < 11; i++)
                {
                    concatBits[(index * 11) + i] = (nds & (1 << (10 - i))) != 0;
                }
            }

            var checksumBitsLen = concatBitsLen / 33;
            var entropyBitsLen = concatBitsLen - checksumBitsLen;
            var entropy = new byte[entropyBitsLen / 8];
            for (int i = 0; i < entropy.Length; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (concatBits[(i * 8) + j])
                    {
                        entropy[i] |= (byte)(1 << (7 - j));
                    }
                }
            }

            var digest = new Sha256Digest();
            byte[] hash = new byte[entropy.Length];
            digest.Update(entropy, 0, entropy.Length);
            digest.DoFinal(hash, 0);
            var hashBits = BytesToBits(hash);
            for (int i = 0; i < checksumBitsLen; i++)
            {
                if (concatBits[entropyBitsLen + i] != hashBits[i])
                {
                    throw new BadMnemonicException(this, BadMnemonicReason.ChecksumMismatch);
                }
            }

            return entropy;
        }

        /// <summary>
        /// Recover an Ed25519 private key from this mnemonic phrase, with an
        /// optional passphrase.
        /// </summary>
        /// <param name="passphrase">the passphrase used to protect the mnemonic</param>
        /// <param name="index">the derivation index</param>
        /// <returns>the private key</returns>
        public PrivateKey ToStandardEd25519PrivateKey(string passphrase, int index)
        {
            var seed = ToSeed(passphrase);
            PrivateKey derivedKey = PrivateKey.FromSeedED25519(seed);
            foreach (int i in new int[]
            {
                44,
                3030,
                0,
                0,
                index
            }

            )
            {
                derivedKey = derivedKey.Derive(i);
            }

            return derivedKey;
        }

        /// <summary>
        /// Converts a derivation path from string to an array of integers.
        /// Note that this expects precisely 5 components in the derivation path,
        /// as per BIP-44:
        /// `m / purpose' / coin_type' / account' / change / address_index`
        /// Takes into account `'` for hardening as per BIP-32,
        /// and does not prescribe which components should be hardened.
        /// </summary>
        /// <param name="derivationPath">the derivation path in BIP-44 format,
        ///                        e.g. "m/44'/60'/0'/0/0"</param>
        /// <returns>an array of integers designed to be used with PrivateKey#derive</returns>
        private int[] CalculateDerivationPathValues(string derivationPath)
        {
            if (string.IsNullOrWhiteSpace(derivationPath))
            {
                throw new ArgumentException("Derivation path cannot be null or empty");
            }


			// Parse the derivation path from string into values
			Regex pattern = new ("m/(\\d+'?)/(\\d+'?)/(\\d+'?)/(\\d+'?)/(\\d+'?)");
            MatchCollection matcher = pattern.Matches(derivationPath);
            if (matcher.Count == 0)
            {
                throw new ArgumentException("Invalid derivation path format");
            }

            int[] numbers = new int[5];
            bool[] isHardened = new bool[5];
            try
            {

                // Extract numbers and use apostrophe to select if is hardened
                for (int i = 1; i <= 5; i++)
                {
                    string value = matcher.ElementAt(i).Value;
                    if (value.EndsWith('\''))
                    {
                        isHardened[i - 1] = true;
                        value = value[0..^1];
                    }
                    else
                    {
                        isHardened[i - 1] = false;
                    }

                    numbers[i - 1] = int.Parse(value);
                }
            }
            catch (FormatException nfe)
            {
                throw new ArgumentException("Invalid number format in derivation path", nfe);
            }


            // Derive private key one index at a time
            int[] values = new int[5];
            for (int i = 0; i < numbers.Length; i++)
            {
                values[i] = (isHardened[i] ? (int)Bip32Utils.ToHardenedIndex(numbers[i]) : numbers[i]);
            }

            return values;
        }

        /// <summary>
        /// Common implementation for both `toStandardECDSAsecp256k1PrivateKey`
        /// functions.
        /// </summary>
        /// <param name="passphrase">the passphrase used to protect the
        ///                              mnemonic, use "" for none</param>
        /// <param name="derivationPathValues">derivation path as an integer array,
        ///                              see: `calculateDerivationPathValues`</param>
        /// <returns>a private key</returns>
        private PrivateKey ToStandardECDSAsecp256k1PrivateKeyImpl(string passphrase, int[] derivationPathValues)
        {
            var seed = ToSeed(passphrase);
            PrivateKey derivedKey = PrivateKey.FromSeedECDSAsecp256k1(seed);
            foreach (int derivationPathValue in derivationPathValues)
            {
                derivedKey = derivedKey.Derive(derivationPathValue);
            }

            return derivedKey;
        }

        /// <summary>
        /// Recover an ECDSAsecp256k1 private key from this mnemonic phrase, with an
        /// optional passphrase.
        /// Uses the default derivation path of `m/44'/3030'/0'/0/${index}`.
        /// </summary>
        /// <param name="passphrase">the passphrase used to protect the mnemonic,
        ///                      use "" for none</param>
        /// <param name="index">the derivation index</param>
        /// <returns>the private key</returns>
        public PrivateKey ToStandardECDSAsecp256k1PrivateKey(string passphrase, int index)
        {
            // Harden the first 3 indexes
            int[] derivationPathValues = 
            [
				(int)Bip32Utils.ToHardenedIndex(44),
				(int)Bip32Utils.ToHardenedIndex(3030),
				(int)Bip32Utils.ToHardenedIndex(0),
				0,
				index
			];

            return ToStandardECDSAsecp256k1PrivateKeyImpl(passphrase, derivationPathValues);
        }

        /// <summary>
        /// Recover an ECDSAsecp256k1 private key from this mnemonic phrase and
        /// derivation path, with an optional passphrase.
        /// </summary>
        /// <param name="passphrase">the passphrase used to protect the mnemonic,
        ///                        use "" for none</param>
        /// <param name="derivationPath">the derivation path in BIP-44 format,
        ///                        e.g. "m/44'/60'/0'/0/0"</param>
        /// <returns>the private key</returns>
        public PrivateKey ToStandardECDSAsecp256k1PrivateKeyCustomDerivationPath(string passphrase, string derivationPath)
        {
            int[] derivationPathValues = CalculateDerivationPathValues(derivationPath);
            return ToStandardECDSAsecp256k1PrivateKeyImpl(passphrase, derivationPathValues);
        }
    }
}