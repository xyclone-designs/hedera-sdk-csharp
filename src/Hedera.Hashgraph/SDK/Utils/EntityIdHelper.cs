// SPDX-License-Identifier: Apache-2.0
using Com.Google.Gson;
using Java.Net;
using Java.Net.Http;
using Java.Nio;
using Java.Time;
using Java.Util;
using Java.Util.Concurrent;
using Java.Util.Regex;
using Javax.Annotation;
using Org.Bouncycastle.Util.Encoders;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;

namespace Hedera.Hashgraph.SDK.Utils
{
    /// <summary>
    /// Utility class used internally by the sdk.
    /// </summary>
    public class EntityIdHelper
    {
		public delegate R WithIdNums<out R>(long shard, long realm, long num, string? Checksum);

		/// <summary>
		/// The length of a Solidity address in bytes.
		/// </summary>
		public static readonly int SOLIDITY_ADDRESS_LEN = 20;
        /// <summary>
        /// The length of a hexadecimal-encoded Solidity address, in ASCII characters (bytes).
        /// </summary>
        public static readonly int SOLIDITY_ADDRESS_LEN_HEX = SOLIDITY_ADDRESS_LEN * 2;
        private static readonly Pattern ENTITY_ID_REGEX = Pattern.Compile("(0|[1-9]\\d*)\\.(0|[1-9]\\d*)\\.(0|[1-9]\\d*)(?:-([a-z]{5}))?$");
        public static readonly Duration MIRROR_NODE_CONNECTION_TIMEOUT = Duration.FromTimeSpan(TimeSpan.FromSeconds(30);
        /// <summary>
        /// Constructor.
        /// </summary>
        private EntityIdHelper()
        {
        }

        /// <summary>
        /// Generate an R object from a string.
        /// </summary>
        /// <param name="idString">the id string</param>
        /// <param name="constructObjectWithIdNums">the R object generator</param>
        /// <param name="<R>"></param>
        /// <returns>the R type object</returns>
        public static R FromString<R>(string idString, WithIdNums<R> constructObjectWithIdNums)
        {
            var match = ENTITY_ID_REGEX.Matcher(idString);
            if (!match.Find())
            {
                throw new ArgumentException("Invalid ID \"" + idString + "\": format should look like 0.0.123 or 0.0.123-vfmkw");
            }

            return constructObjectWithIdNums.Invoke(long.Parse(match.Group(1)), long.Parse(match.Group(2)), long.Parse(match.Group(3)), match.Group(4));
        }

        /// <summary>
        /// Generate an R object from a solidity address.
        /// </summary>
        /// <param name="address">the string representation</param>
        /// <param name="withAddress">the R object generator</param>
        /// <param name="<R>"></param>
        /// <returns>the R type object</returns>
        public static R FromSolidityAddress<R>(string address, WithIdNums<R> withAddress)
        {
            return FromSolidityAddress(DecodeEvmAddress(address), withAddress);
        }

        private static R FromSolidityAddress<R>(byte[] address, WithIdNums<R> withAddress)
        {
            if (address.Length != SOLIDITY_ADDRESS_LEN)
            {
                throw new ArgumentException("Solidity addresses must be 20 bytes or 40 hex chars");
            }

            var buf = ByteBuffer.Wrap(address);
            
            return withAddress.Invoke(buf.GetInt(), buf.GetLong(), buf.GetLong(), null);
        }

        /// <summary>
        /// Decode the solidity address from a string.
        /// </summary>
        /// <param name="address">the string representation</param>
        /// <returns>the decoded address</returns>
        public static byte[] DecodeEvmAddress(string address)
        {
            address = address.StartsWith("0x") ? address.Substring(2) : address;
            if (address.Length != SOLIDITY_ADDRESS_LEN_HEX)
            {
                throw new ArgumentException("Solidity addresses must be 20 bytes or 40 hex chars");
            }

            try
            {
                return Hex.Decode(address);
            }
            catch (DecoderException e)
            {
                throw new ArgumentException("failed to decode Solidity address as hex", e);
            }
        }

        /// <summary>
        /// Generate a solidity address.
        /// </summary>
        /// <param name="shard">the shard part</param>
        /// <param name="realm">the realm part</param>
        /// <param name="num">the num part</param>
        /// <returns>the solidity address</returns>
        public static string ToSolidityAddress(long shard, long realm, long num)
        {
            if (Long.HighestOneBit(shard) > 32)
            {
                throw new InvalidOperationException("shard out of 32-bit range " + shard);
            }

            return Hex.ToHexString(ByteBuffer.Allocate(20).PutInt((int)shard).PutLong(realm).PutLong(num).Array());
        }

        /// <summary>
        /// Generate a checksum.
        /// </summary>
        /// <param name="ledgerId">the ledger id</param>
        /// <param name="addr">the address</param>
        /// <returns>the checksum</returns>
        public static string Checksum(LedgerId ledgerId, string addr)
        {
            StringBuilder answer = new ();
            IList<int> d = []; // Digits with 10 for ".", so if addr == "0.0.123" then d == [0, 10, 0, 10, 1, 2, 3]
            long s0 = 0; // Sum of even positions (mod 11)
            long s1 = 0; // Sum of odd positions (mod 11)
            long s = 0; // Weighted sum of all positions (mod p3)
            long sh = 0; // Hash of the ledger ID
            long c = 0; // The checksum, as a single number
            long p3 = 26 * 26 * 26; // 3 digits in base 26
            long p5 = 26 * 26 * 26 * 26 * 26; // 5 digits in base 26
            long asciiA = Character.CodePointAt("a", 0); // 97
            long m = 1000003; // min prime greater than a million. Used for the final permutation.
            long w = 31; // Sum s of digit values weights them by powers of w. Should be coprime to p5.
            IList<Byte> h = new List(ledgerId.ToBytes().Length + 6);
            foreach (byte b in ledgerId.ToBytes())
            {
                h.Add(b);
            }

            for (int i = 0; i < 6; i++)
            {
                h.Add((byte)0);
            }

            for (var i = 0; i < addr.Length; i++)
            {
                d.Add(addr.CharAt(i) == '.' ? 10 : int.Parse(string.ValueOf(addr.CharAt(i)), 10));
            }

            for (var i = 0; i < d.Count; i++)
            {
                s = (w * s + d[i]) % p3;
                if (i % 2 == 0)
                {
                    s0 = (s0 + d[i]) % 11;
                }
                else
                {
                    s1 = (s1 + d[i]) % 11;
                }
            }

            foreach (byte b in h)
            {

                // byte is signed in java, have to fake it to make bytes act like they're unsigned
                sh = (w * sh + (b < 0 ? 256 + b : b)) % p5;
            }

            c = ((((addr.Length % 5) * 11 + s0) * 11 + s1) * p3 + s + sh) % p5;
            c = (c * m) % p5;
            for (var i = 0; i < 5; i++)
            {
                answer.Append((char)(asciiA + (c % 26)));
                c /= 26;
            }

            return answer.Reverse().ToString();
        }

        /// <summary>
        /// Validate the configured client.
        /// </summary>
        /// <param name="shard">the shard part</param>
        /// <param name="realm">the realm part</param>
        /// <param name="num">the num part</param>
        /// <param name="client">the configured client</param>
        /// <param name="checksum">the checksum</param>
        /// <exception cref="BadEntityIdException"></exception>
        public static void Validate(long shard, long realm, long num, Client client, string? checksum)
        {
            if (client.GetNetworkName() == null)
            {
                throw new InvalidOperationException("Can't validate checksum without knowing which network the ID is for.  Ensure client's network name is set.");
            }

            if (checksum != null)
            {
                string expectedChecksum = Utils.EntityIdHelper.Checksum(client.LedgerId, Utils.EntityIdHelper.ToString(shard, realm, num));
                if (!checksum.Equals(expectedChecksum))
                {
                    throw new BadEntityIdException(shard, realm, num, checksum, expectedChecksum);
                }
            }
        }

        /// <summary>
        /// Generate a string representation.
        /// </summary>
        /// <param name="shard">the shard part</param>
        /// <param name="realm">the realm part</param>
        /// <param name="num">the num part</param>
        /// <returns>the string representation</returns>
        public static string ToString(long shard, long realm, long num)
        {
            return "" + shard + "." + realm + "." + num;
        }

        /// <summary>
        /// Generate a string representation with a checksum.
        /// </summary>
        /// <param name="shard">the shard part</param>
        /// <param name="realm">the realm part</param>
        /// <param name="num">the num part</param>
        /// <param name="client">the configured client</param>
        /// <param name="checksum">the checksum</param>
        /// <returns>the string representation with checksum</returns>
        public static string ToStringWithChecksum(long shard, long realm, long num, Client client, string? checksum)
        {
            if (client.LedgerId != null)
            {
                return "" + shard + "." + realm + "." + num + "-" + Checksum(client.LedgerId, Utils.EntityIdHelper.ToString(shard, realm, num));
            }
            else
            {
                throw new InvalidOperationException("Can't derive checksum for ID without knowing which network the ID is for.  Ensure client's ledgerId is set.");
            }
        }

        /// <summary>
        /// Takes an address as `byte[]` and returns whether this is a long-zero address
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static bool IsLongZeroAddress(byte[] address)
        {
            for (int i = 0; i < 12; i++)
            {
                if (address[i] != 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Get AccountId num from mirror node using evm address.
        /// 
        /// <p>Note: This method requires API level 33 or higher. It will not work on devices running API versions below 33
        /// because it uses features introduced in API level 33 (Android 13).</p>*
        /// </summary>
        /// <param name="client"></param>
        /// <param name="evmAddress"></param>
        public static Task<long> GetAccountNumFromMirrorNodeAsync(Client client, string evmAddress)
        {
            string apiEndpoint = "/accounts/" + evmAddress;
            return PerformQueryToMirrorNodeAsync(client, apiEndpoint, null).ThenApply((response) => ParseNumFromMirrorNodeResponse(response, "account"));
        }

        /// <summary>
        /// Get EvmAddress from mirror node using account num.
        /// 
        /// <p>Note: This method requires API level 33 or higher. It will not work on devices running API versions below 33
        /// because it uses features introduced in API level 33 (Android 13).</p>*
        /// </summary>
        /// <param name="client"></param>
        /// <param name="num"></param>
        public static Task<EvmAddress> GetEvmAddressFromMirrorNodeAsync(Client client, long num)
        {
            string apiEndpoint = "/accounts/" + num;
            return PerformQueryToMirrorNodeAsync(client, apiEndpoint, null).ThenApply((response) => EvmAddress.FromString(ParseStringMirrorNodeResponse(response, "evm_address")));
        }

        /// <summary>
        /// Get ContractId num from mirror node using evm address.
        /// 
        /// <p>Note: This method requires API level 33 or higher. It will not work on devices running API versions below 33
        /// because it uses features introduced in API level 33 (Android 13).</p>*
        /// </summary>
        /// <param name="client"></param>
        /// <param name="evmAddress"></param>
        public static Task<long> GetContractNumFromMirrorNodeAsync(Client client, string evmAddress)
        {
            string apiEndpoint = "/contracts/" + evmAddress;
            Task<string> responseFuture = PerformQueryToMirrorNodeAsync(client, apiEndpoint, null);
            return responseFuture.ThenApply((response) => ParseNumFromMirrorNodeResponse(response, "contract_id"));
        }

        public static Task<string> PerformQueryToMirrorNodeAsync(Client client, string apiEndpoint, string jsonBody)
        {
            return PerformQueryToMirrorNodeAsync(client.GetMirrorRestBaseUrl(), apiEndpoint, jsonBody);
        }

        public static Task<string> PerformQueryToMirrorNodeAsync(string baseUrl, string apiEndpoint, string jsonBody)
        {
            string apiUrl = baseUrl + apiEndpoint;
            HttpClient httpClient = HttpClient.NewHttpClient();
            var httpBuilder = HttpRequest.NewBuilder().Timeout(MIRROR_NODE_CONNECTION_TIMEOUT).Uri(URI.Create(apiUrl));
            if (jsonBody != null)
            {
                httpBuilder.Header("Content-Type", "application/json").POST(HttpRequest.BodyPublishers.OfString(jsonBody));
            }

            var httpRequest = httpBuilder.Build();
            return httpClient.SendAsync(httpRequest, HttpResponse.BodyHandlers.OfString()).Handle((response, ex) =>
            {
                if (ex != null)
                {
                    if (ex is HttpTimeoutException)
                    {
                        throw new CompletionException(new Exception("Request to Mirror Node timed out", ex));
                    }
                    else
                    {
                        throw new CompletionException(new Exception("Failed to send request to Mirror Node", ex));
                    }
                }

                int statusCode = response.StatusCode();
                if (statusCode != 200)
                {
                    throw new CompletionException(new Exception("Received non-200 response from Mirror Node: " + response.Body()));
                }

                return response.Body();
            });
        }

        private static string ParseStringMirrorNodeResponse(string responseBody, string memberName)
        {
            JsonObject jsonObject = JsonParser.ParseString(responseBody).GetAsJsonObject();
            string evmAddress = jsonObject[memberName].GetAsString();
            return evmAddress.Substring(evmAddress.LastIndexOf(".") + 1);
        }

        private static long ParseNumFromMirrorNodeResponse(string responseBody, string memberName)
        {
            return long.Parse(ParseStringMirrorNodeResponse(responseBody, memberName));
        }
    }
}