// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Queries;

using System;

namespace Hedera.Hashgraph.SDK.LiveHashes
{
    /// <include file="LiveHashQuery.cs.xml" path='docs/member[@name="M:Obsolete(&quot;Obsolete&quot;)"]/*' />
    [Obsolete("Obsolete")]
    public sealed class LiveHashQuery : Query<LiveHash, LiveHashQuery>
    {
        /// <include file="LiveHashQuery.cs.xml" path='docs/member[@name="P:.AccountId"]/*' />
        public AccountId? AccountId { get; set; }
        /// <include file="LiveHashQuery.cs.xml" path='docs/member[@name="M:CopyArray"]/*' />
        public byte[] Hash 
        { 
            get => field.CopyArray(); 
            set => value.CopyArray(); 

        } = [];

        public override void ValidateChecksums(Client client)
        {
			AccountId?.ValidateChecksum(client);
		}
        public override void OnMakeRequest(Proto.Query queryBuilder, Proto.QueryHeader header)
        {
            var builder = new Proto.CryptoGetLiveHashQuery
            {
                Header = header
            };

            if (AccountId != null)
            {
                builder.AccountID = AccountId.ToProtobuf();
            }

            builder.Hash = ByteString.CopyFrom(Hash);
            
            queryBuilder.CryptoGetLiveHash = builder;
        }
        public override Proto.QueryHeader MapRequestHeader(Proto.Query request)
		{
			return request.CryptoGetLiveHash.Header;
		}
		public override Proto.ResponseHeader MapResponseHeader(Proto.Response response)
        {
            return response.CryptoGetLiveHash.Header;
        }
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.CryptoService.CryptoServiceClient.cryptoGetBalance);

			return Proto.CryptoService.Descriptor.FindMethodByName(methodname);
		}
		public override LiveHash MapResponse(Proto.Response response, AccountId nodeId, Proto.Query request)
		{
			return LiveHash.FromProtobuf(response.CryptoGetLiveHash.LiveHash);
		}
	}
}