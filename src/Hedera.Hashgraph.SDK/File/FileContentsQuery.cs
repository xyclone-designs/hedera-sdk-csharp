// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Queries;

using System;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.File
{
    /// <include file="FileContentsQuery.cs.xml" path='docs/member[@name="T:FileContentsQuery"]/*' />
    public sealed class FileContentsQuery : Query<ByteString, FileContentsQuery>
    {
        /// <include file="FileContentsQuery.cs.xml" path='docs/member[@name="P:FileContentsQuery.FileId"]/*' />
        public FileId? FileId { get; set; }

		public override async Task<Hbar> GetCostAsync(Client client)
		{
			// deleted accounts return a COST_ANSWER of zero which triggers `INSUFFICIENT_TX_FEE`
			// if you set that as the query payment; 25 tinybar seems to be enough to get
			// `Token_DELETED` back instead.

			Hbar cost = await base.GetCostAsync(client);

			return Hbar.FromTinybars(Math.Max(cost.ToTinybars(), 25));

		}

		public override void ValidateChecksums(Client client)
        {
            FileId?.ValidateChecksum(client);
        }
        public override void OnMakeRequest(Proto.Services.Query queryBuilder, Proto.Services.QueryHeader header)
        {
            var builder = new Proto.Services.FileGetContentsQuery
            {
                Header = header
            };

            if (FileId != null)
            {
                builder.FileId = FileId.ToProtobuf();
            }

            queryBuilder.FileGetContents = builder;
        }
        public override Proto.Services.ResponseHeader MapResponseHeader(Proto.Services.Response response)
        {
            return response.FileGetContents.Header;
        }
        public override Proto.Services.QueryHeader MapRequestHeader(Proto.Services.Query request)
        {
            return request.FileGetContents.Header;
        }
        public override ByteString MapResponse(Proto.Services.Response response, AccountId nodeId, Proto.Services.Query request)
        {
            return response.FileGetContents.FileContents.Contents;
        }
		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.Services.FileService.FileServiceClient.getFileContent);

			return Proto.Services.FileService.Descriptor.FindMethodByName(methodname);
		}
	}
}
