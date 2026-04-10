// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Ethereum;
using Hedera.Hashgraph.SDK.Exceptions;
using Hedera.Hashgraph.SDK.Networking;

using Org.BouncyCastle.Utilities.Encoders;

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Utils
{
    /// <include file="EntityIdHelper.cs.xml" path='docs/member[@name="T:EntityIdHelper"]/*' />
    public class EntityIdHelper
    {
		public delegate R WithIdNums<out R>(long shard, long realm, long num, string? checksum);

		/// <include file="EntityIdHelper.cs.xml" path='docs/member[@name="F:EntityIdHelper.SOLIDITY_ADDRESS_LEN"]/*' />
		public static readonly int SOLIDITY_ADDRESS_LEN = 20;
        /// <include file="EntityIdHelper.cs.xml" path='docs/member[@name="F:EntityIdHelper.SOLIDITY_ADDRESS_LEN_HEX"]/*' />
        public static readonly int SOLIDITY_ADDRESS_LEN_HEX = SOLIDITY_ADDRESS_LEN * 2;
        private static readonly Regex ENTITY_ID_REGEX = new ("(0|[1-9]\\d*)\\.(0|[1-9]\\d*)\\.(0|[1-9]\\d*)(?:-([a-z]{5}))?$");
        public static readonly TimeSpan MIRROR_NODE_CONNECTION_TIMEOUT = TimeSpan.FromSeconds(30);
        /// <include file="EntityIdHelper.cs.xml" path='docs/member[@name="M:EntityIdHelper.#ctor"]/*' />
        private EntityIdHelper()
        {
        }

        /// <include file="EntityIdHelper.cs.xml" path='docs/member[@name="M:EntityIdHelper.FromString``1(System.String,WithIdNums{``0})"]/*' />
        public static R FromString<R>(string idString, WithIdNums<R> constructObjectWithIdNums)
        {
            MatchCollection match = ENTITY_ID_REGEX.Matches(idString);

            if (match.Count == 0)
            {
                throw new ArgumentException("Invalid ID \"" + idString + "\": format should look like 0.0.123 or 0.0.123-vfmkw");
            }

            return constructObjectWithIdNums.Invoke(long.Parse(match[1].Value), long.Parse(match[2].Value), long.Parse(match[3].Value), match[4]?.Value);
        }
        /// <include file="EntityIdHelper.cs.xml" path='docs/member[@name="M:EntityIdHelper.FromSolidityAddress``1(System.String,WithIdNums{``0})"]/*' />
        public static R FromSolidityAddress<R>(string address, WithIdNums<R> withAddress)
        {
            return FromSolidityAddress(DecodeEvmAddress(address), withAddress);
        }
        public static R FromSolidityAddress<R>(byte[] address, WithIdNums<R> withAddress)
        {
            if (address.Length != SOLIDITY_ADDRESS_LEN)
				throw new ArgumentException("Solidity addresses must be 20 bytes or 40 hex chars");

			var span = address.AsSpan();

			return withAddress.Invoke(
				BinaryPrimitives.ReadInt32BigEndian(span.Slice(00, 4)), 
                BinaryPrimitives.ReadInt64BigEndian(span.Slice(04, 8)),
                BinaryPrimitives.ReadInt64BigEndian(span.Slice(12, 8)),
                null);
        }

		/// <include file="EntityIdHelper.cs.xml" path='docs/member[@name="M:EntityIdHelper.Checksum(LedgerId,System.String)"]/*' />
		public static string Checksum(LedgerId ledgerId, string addr)
		{
			StringBuilder answer = new();
			List<int> d = []; // Digits with 10 for ".", so if addr == "0.0.123" then d == [0, 10, 0, 10, 1, 2, 3]
			long s0 = 0; // Sum of even positions (mod 11)
			long s1 = 0; // Sum of odd positions (mod 11)
			long s = 0; // Weighted sum of all positions (mod p3)
			long sh = 0; // Hash of the ledger ID
			long c = 0; // The checksum, as a single number
			long p3 = 26 * 26 * 26; // 3 digits in base 26
			long p5 = 26 * 26 * 26 * 26 * 26; // 5 digits in base 26
			long asciiA = 'a'; // 97
			long m = 1000003; // min prime greater than a million. Used for the final permutation.
			long w = 31; // Sum s of digit values weights them by powers of w. Should be coprime to p5.
			List<byte> h = new(ledgerId.ToBytes().Length + 6);
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
				d.Add(addr.ElementAt(i) == '.' ? 10 : int.TryParse(addr.ElementAt(i).ToString(), out int _value) ? _value : 10);
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

			return string.Join(string.Empty, answer.ToString().Reverse());
		}
		/// <include file="EntityIdHelper.cs.xml" path='docs/member[@name="M:EntityIdHelper.DecodeEvmAddress(System.String)"]/*' />
		public static byte[] DecodeEvmAddress(string address)
        {
            address = address.StartsWith("0x") ? address.Substring(2) : address;

            if (address.Length != SOLIDITY_ADDRESS_LEN_HEX)
				throw new ArgumentException("Solidity addresses must be 20 bytes or 40 hex chars");

			try
            {
                return Hex.Decode(address);
            }
            catch (Exception e)
            {
                throw new ArgumentException("failed to decode Solidity address as hex", e);
            }
        }
		/// <include file="EntityIdHelper.cs.xml" path='docs/member[@name="M:EntityIdHelper.IsLongZeroAddress(System.Byte[])"]/*' />
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

        /// <include file="EntityIdHelper.cs.xml" path='docs/member[@name="M:EntityIdHelper.ToString(System.Int64,System.Int64,System.Int64)"]/*' />
        public static string ToString(long shard, long realm, long num)
        {
            return "" + shard + "." + realm + "." + num;
        }
		/// <include file="EntityIdHelper.cs.xml" path='docs/member[@name="M:EntityIdHelper.ToSolidityAddress(System.Int64,System.Int64,System.Int64)"]/*' />
		public static string ToSolidityAddress(long shard, long realm, long num)
		{
			if (shard < 0 || shard > 0xFFFFFFFF)
				throw new InvalidOperationException("shard out of 32-bit range " + shard);

			byte[] bytes = new byte[20];

			BinaryPrimitives.WriteInt32BigEndian(bytes.AsSpan(0, 4), (int)shard);
			BinaryPrimitives.WriteInt64BigEndian(bytes.AsSpan(4, 8), realm);
			BinaryPrimitives.WriteInt64BigEndian(bytes.AsSpan(12, 8), num);

			return Convert.ToHexStringLower(bytes);
		}
		/// <include file="EntityIdHelper.cs.xml" path='docs/member[@name="M:EntityIdHelper.ToStringWithChecksum(System.Int64,System.Int64,System.Int64,Client,System.String)"]/*' />
		public static string ToStringWithChecksum(long shard, long realm, long num, Client client, string? checksum)
        {
            if (client.Network_.LedgerId != null)
            {
                return "" + shard + "." + realm + "." + num + "-" + Checksum(client.Network_.LedgerId, ToString(shard, realm, num));
            }
            else
            {
                throw new InvalidOperationException("Can't derive checksum for ID without knowing which network the ID is for.  Ensure client's ledgerId is set.");
            }
        }

		/// <include file="EntityIdHelper.cs.xml" path='docs/member[@name="M:EntityIdHelper.Validate(System.Int64,System.Int64,System.Int64,Client,System.String)"]/*' />
		public static void Validate(long shard, long realm, long num, Client client, string? checksum)
		{
			if (client.Network_.LedgerId == null)
			{
				throw new InvalidOperationException("Can't validate checksum without knowing which network the ID is for.  Ensure client's ledgerId is set.");
			}

			if (checksum != null)
			{
				string expectedChecksum = Checksum(client.Network_.LedgerId, ToString(shard, realm, num));

				if (!checksum.Equals(expectedChecksum))
				{
					throw new BadEntityIdException(shard, realm, num, checksum, expectedChecksum);
				}
			}
		}

		/// <include file="EntityIdHelper.cs.xml" path='docs/member[@name="M:EntityIdHelper.GetEvmAddressFromMirrorNodeAsync(Client,System.Int64)"]/*' />
		public static async Task<EvmAddress> GetEvmAddressFromMirrorNodeAsync(Client client, long num)
		{
			string apiEndpoint = "/accounts/" + num;
			string _ = await PerformQueryToMirrorNodeAsync(client, apiEndpoint, null);

			return EvmAddress.FromString(ParseStringMirrorNodeResponse(_, "evm_address"));
		}
		/// <include file="EntityIdHelper.cs.xml" path='docs/member[@name="M:EntityIdHelper.GetAccountNumFromMirrorNodeAsync(Client,System.String)"]/*' />
		public static async Task<long> GetAccountNumFromMirrorNodeAsync(Client client, string? evmAddress)
        {
            string apiEndpoint = "/accounts/" + evmAddress;
            string _ = await PerformQueryToMirrorNodeAsync(client, apiEndpoint, null);

			return ParseNumFromMirrorNodeResponse(_, "account");
        }
        /// <include file="EntityIdHelper.cs.xml" path='docs/member[@name="M:EntityIdHelper.GetContractNumFromMirrorNodeAsync(Client,System.String)"]/*' />
        public static async Task<long> GetContractNumFromMirrorNodeAsync(Client client, string evmAddress)
        {
            string apiEndpoint = "/contracts/" + evmAddress;
            string responseFuture = await PerformQueryToMirrorNodeAsync(client, apiEndpoint, null);

            return ParseNumFromMirrorNodeResponse(responseFuture, "contract_id");
        }

        public static Task<string> PerformQueryToMirrorNodeAsync(Client client, string apiEndpoint, string? jsonBody)
        {
            return PerformQueryToMirrorNodeAsync(client.MirrorRestBaseUrl, apiEndpoint, jsonBody);
        }
		public static async Task<string> PerformQueryToMirrorNodeAsync(string baseUrl, string apiEndpoint, string? jsonBody)
		{
			string apiUrl = baseUrl + apiEndpoint;

			using var httpClient = new HttpClient
			{
				Timeout = MIRROR_NODE_CONNECTION_TIMEOUT
			};

			using var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);

			if (!string.IsNullOrEmpty(jsonBody))
			{
				request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
			}

			try
			{
				using var response = await httpClient.SendAsync(request);

				string responseBody = await response.Content.ReadAsStringAsync();

				if (!response.IsSuccessStatusCode)
				{
					throw new Exception(
						$"Received non-200 response from Mirror Node: {responseBody}");
				}

				return responseBody;
			}
			catch (TaskCanceledException ex) when (!ex.CancellationToken.IsCancellationRequested)
			{
				// timeout
				throw new Exception("Request to Mirror Node timed out", ex);
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to send request to Mirror Node", ex);
			}
		}

		private static long ParseNumFromMirrorNodeResponse(string responseBody, string memberName)
		{
			return long.Parse(ParseStringMirrorNodeResponse(responseBody, memberName));
		}
		private static string ParseStringMirrorNodeResponse(string responseBody, string memberName)
        {
            JsonNode jsonnode = JsonNode.Parse(responseBody) ?? throw new ArgumentException();
            string evmAddress = jsonnode[memberName]?.GetValue<string>() ?? throw new ArgumentException();

            return evmAddress?[(evmAddress.LastIndexOf('.') + 1)..] ?? throw new ArgumentException();
        }
    }
}