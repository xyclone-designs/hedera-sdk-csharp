using Google.Protobuf;

using Org.BouncyCastle.Utilities.Encoders;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK
{
	/**
     * Utility class used internally by the sdk.
     */
    public class EntityIdHelper 
    {
		public delegate R WithIdNums<out R>(long shard, long realm, long num, string? Checksum);

		/**
         * The length of a Solidity address in bytes.
         */
		private static readonly int SOLIDITY_ADDRESS_LEN = 20;
		/**
         * The length of a hexadecimal-encoded Solidity address, in ASCII characters (bytes).
         */
		private static readonly int SOLIDITY_ADDRESS_LEN_HEX = SOLIDITY_ADDRESS_LEN * 2;
        private static readonly Regex ENTITY_ID_REGEX = new("(0|[1-9]\\d*)\\.(0|[1-9]\\d*)\\.(0|[1-9]\\d*)(?:-([a-z]{5}))?$");
		private static readonly TimeSpan MIRROR_NODE_CONNECTION_TIMEOUT = TimeSpan.FromSeconds(30);

        /**
         * Constructor.
         */
        private EntityIdHelper() {}

		/**
         * Generate an R object from a string.
         *
         * @param idString                  the id string
         * @param constructObjectWithIdNums the R object generator
         * @param <R>
         * @return the R type object
         */
		public static R FromString<R>(string idString, WithIdNums<R> constructObjectWithIdNums) 
        {
            MatchCollection matches = ENTITY_ID_REGEX.Matches(idString);
            if (matches.Count == 0) 
                throw new ArgumentException("Invalid ID \"" + idString + "\": format should look like 0.0.123 or 0.0.123-vfmkw");
            
            return constructObjectWithIdNums.Apply(
                    long.Parse(matches.ElementAt(1).Value),
                    long.Parse(matches.ElementAt(2).Value),
                    long.Parse(matches.ElementAt(3).Value),
                    matches.ElementAt(4).Value);
        }
        /**
         * Generate an R object from a solidity address.
         *
         * @param address     the string representation
         * @param withAddress the R object generator
         * @param <R>
         * @return the R type object
         */
        public static R FromSolidityAddress<R>(string address, WithIdNums<R> withAddress) 
        {
            return FromSolidityAddress(DecodeEvmAddress(address), withAddress);
        }
        public static R FromSolidityAddress<R>(byte[] address, WithIdNums<R> withAddress) {
            if (address.Length != SOLIDITY_ADDRESS_LEN) {
                throw new ArgumentException("Solidity addresses must be 20 bytes or 40 hex chars");
            }

            var buf = ByteBuffer.wrap(address);
            
            return withAddress.Invoke(buf.getInt(), buf.getLong(), buf.getLong(), null);
        }
		public static long ParseNumFromMirrorNodeResponse(string responseBody, string memberName)
		{
			return long.Parse(ParseStringMirrorNodeResponse(responseBody, memberName));
		}
		public static string ParseStringMirrorNodeResponse(string responseBody, string memberName)
		{
			JsonObject jsonObject = JsonParser.Parse(responseBody).getAsJsonObject();
			string evmAddress = jsonObject.get(memberName).getAsString();
			return evmAddress.Substring(evmAddress.LastIndexOf(".") + 1);
		}

		/**
         * Decode the solidity address from a string.
         *
         * @param address the string representation
         * @return the decoded address
         */
		public static byte[] DecodeEvmAddress(string address) 
        {
            address = address.StartsWith("0x") ? address.Substring(2) : address;

            if (address.Length != SOLIDITY_ADDRESS_LEN_HEX) throw new ArgumentException("Solidity addresses must be 20 bytes or 40 hex chars");

			try { return Hex.Decode(address); } 
            catch (Exception ex) { throw new ArgumentException("failed to decode Solidity address as hex", ex); }
        }

		/**
         * Validate the configured client.
         *
         * @param Shard    the Shard part
         * @param Realm    the Realm part
         * @param Num      the Num part
         * @param client   the configured client
         * @param Checksum the Checksum
         * @
         */
		public static void Validate(long shard, long realm, long num, Client client, string? Checksum)
		{
			if (client.getNetworkName() == null)
			{
				throw new InvalidOperationException(
						"Can't validate Checksum without knowing which network the ID is for.  Ensure client's network name is set.");
			}
			if (Checksum != null)
			{
				string expectedChecksum = EntityIdHelper.Checksum(client.LedgerId, EntityIdHelper.ToString(shard, realm, num));

				if (Checksum.Equals(expectedChecksum) is false)
					throw new BadEntityIdException(shard, realm, num, Checksum, expectedChecksum);
			}
		}

		/**
         * Takes an address as `byte[]` and returns whether this is a long-zero address
         * @param address
         * @return
         */
		public static bool IsLongZeroAddress(byte[] address)
		{
			for (int i = 0; i < 12; i++)
				if (address[i] != 0)
					return false;

			return true;
		}
		/**
         * Generate a Checksum.
         *
         * @param ledgerId the ledger id
         * @param addr     the address
         * @return the Checksum
         */
		public static string Checksum(LedgerId ledgerId, string addr) 
        {
            StringBuilder answer = new ();
            List<int> d = []; // Digits with 10 for ".", so if addr == "0.0.123" then d == [0, 10, 0, 10, 1, 2, 3]
            long s0 = 0; // Sum of even positions (mod 11)
            long s1 = 0; // Sum of odd positions (mod 11)
            long s = 0; // Weighted sum of all positions (mod p3)
            long sh = 0; // Hash of the ledger ID
            long c = 0; // The Checksum, as a single number
            long p3 = 26 * 26 * 26; // 3 digits in base 26
            long p5 = 26 * 26 * 26 * 26 * 26; // 5 digits in base 26
            long asciiA = char.ConvertToUtf32("a", 0); // 97
            long m = 1_000_003; // min prime greater than a million. Used for the readonly permutation.
            long w = 31; // Sum s of digit values weights them by powers of w. Should be coprime to p5.

            List<byte> h = new (ledgerId.ToBytes().Length + 6);
            
            foreach (byte b in ledgerId.ToBytes()) h.Add(b);

			for (int i = 0; i < 6; i++)
				h.Add((byte)0);

			for (var i = 0; i < addr.Length; i++)
				d.Add(addr.ElementAt(i) == '.' ? 10 : int.Parse(addr.ElementAt(i).ToString()));

			for (var i = 0; i < d.Count; i++) 
            {
                s = (w * s + d.ElementAt(i)) % p3;
                if (i % 2 == 0) {
                    s0 = (s0 + d.ElementAt(i)) % 11;
                } else {
                    s1 = (s1 + d.ElementAt(i)) % 11;
                }
            }
            foreach (byte b in h) 
            {
                // byte is signed in java, have to fake it to make bytes act like they're unsigned
                sh = (w * sh + (b < 0 ? 256 + b : b)) % p5;
            }

            c = ((((addr.Length % 5) * 11 + s0) * 11 + s1) * p3 + s + sh) % p5;
            c = (c * m) % p5;

            for (var i = 0; i < 5; i++) {
                answer.Append((char) (asciiA + (c % 26)));
                c /= 26;
            }

            return string.Join(string.Empty, answer.ToString().Reverse());
        }
		/**
         * Generate a string representation.
         *
         * @param Shard the Shard part
         * @param Realm the Realm part
         * @param Num   the Num part
         * @return the string representation
         */
		public static string ToString(long shard, long realm, long num)
        {
            return "" + shard + "." + realm + "." + num;
        }
		/**
         * Generate a solidity address.
         *
         * @param Shard the Shard part
         * @param Realm the Realm part
         * @param Num   the Num part
         * @return the solidity address
         */
		public static string ToSolidityAddress(long shard, long realm, long num)
		{
			if (shard != 0 && (63 - BitOperations.LeadingZeroCount((ulong)shard)) >= 5)
				throw new ArgumentException("Shard out of 32-bit range " + shard);

			return Hex.ToHexString(ByteBuffer.allocate(20)
					.PutInt((int)Shard)
					.PutLong(Realm)
					.PutLong(Num)
					.array());
		}
		/**
         * Generate a string representation with a Checksum.
         *
         * @param Shard    the Shard part
         * @param Realm    the Realm part
         * @param Num      the Num part
         * @param client   the configured client
         * @param Checksum the Checksum
         * @return the string representation with Checksum
         */
		public static string ToStringWithChecksum(long shard, long realm, long num, Client client, string? checksum) 
        {
            if (client.LedgerId is not null)
				throw new ArgumentException("Can't derive Checksum for ID without knowing which network the ID is for.  Ensure client's ledgerId is set.");

			return "" + shard + "." + realm + "." + num + "-" + Checksum(client.LedgerId, ToString(shard, realm, num));
		}
		/**
         * Get EvmAddress from mirror node using account Num.
         *
         * <p>Note: This method requires API level 33 or higher. It will not work on devices running API versions below 33
         * because it uses features introduced in API level 33 (Android 13).</p>*
         *
         * @param client
         * @param Num
         */
		public static async Task<EvmAddress> GetEvmAddressFromMirrorNodeAsync(Client client, long Num)
		{
			string apiEndpoint = "/accounts/" + Num;
            string result = await PerformQueryToMirrorNodeAsync(client, apiEndpoint, null);

            return EvmAddress.FromString(ParseStringMirrorNodeResponse(result, "evm_address");
		}
		/**
         * Get AccountId Num from mirror node using evm address.
         *
         * <p>Note: This method requires API level 33 or higher. It will not work on devices running API versions below 33
         * because it uses features introduced in API level 33 (Android 13).</p>*
         *
         * @param client
         * @param evmAddress
         */
		public static async Task<long> GetAccountNumFromMirrorNodeAsync(Client client, string evmAddress) 
        {
            string apiEndpoint = "/accounts/" + evmAddress;
            string result = await PerformQueryToMirrorNodeAsync(client, apiEndpoint, null);

            return ParseNumFromMirrorNodeResponse(result, "account");

		}
        /**
         * Get ContractId Num from mirror node using evm address.
         *
         * <p>Note: This method requires API level 33 or higher. It will not work on devices running API versions below 33
         * because it uses features introduced in API level 33 (Android 13).</p>*
         *
         * @param client
         * @param evmAddress
         */
        public static async Task<long> GetContractNumFromMirrorNodeAsync(Client client, string evmAddress) {
            string apiEndpoint = "/contracts/" + evmAddress;
            string response = await PerformQueryToMirrorNodeAsync(client, apiEndpoint, null);

            return ParseNumFromMirrorNodeResponse(response, "contract_id");
        }
        public static Task<string> PerformQueryToMirrorNodeAsync(Client client, string apiEndpoint, string? jsonBody) 
        {
            return PerformQueryToMirrorNodeAsync(client.GetMirrorRestBaseUrl(), apiEndpoint, jsonBody);
        }
        public static async Task<string> PerformQueryToMirrorNodeAsync(string baseUrl, string apiEndpoint, string? jsonBody) 
        {
            using HttpClient httpclient = new()
            {
                Timeout = MIRROR_NODE_CONNECTION_TIMEOUT,
			};
            using HttpRequestMessage httprequestmessage = new(HttpMethod.Post, new Uri(baseUrl + apiEndpoint)) 
            {
                Content = JsonContent.Create(jsonBody),
            };

            httprequestmessage.Headers.Add("Content-Type", "application/json");

            try
            {
				using HttpResponseMessage httpresponsemessage = await httpclient.SendAsync(httprequestmessage);

                if (httpresponsemessage.StatusCode != HttpStatusCode.OK)
                    throw new InvalidOperationException("Received non-200 response from Mirror Node: " + httpresponsemessage.Content);

                return httpresponsemessage.Content.ToString() ?? string.Empty;
			}
            catch (Exception) { throw; }
        }
    }
}