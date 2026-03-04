// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf.Reflection;

using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Queries;

using System;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.File
{
    /// <include file="FileInfoQuery.cs.xml" path='docs/member[@name="T:FileInfoQuery"]/*' />
    public sealed class FileInfoQuery : Query<FileInfo, FileInfoQuery>
    {
        /// <include file="FileInfoQuery.cs.xml" path='docs/member[@name="P:FileInfoQuery.FileId"]/*' />
        public FileId? FileId { get; set; }

		public override void ValidateChecksums(Client client)
        {
            if (FileId != null)
            {
                FileId.ValidateChecksum(client);
            }
        }

        public override void OnMakeRequest(Proto.Query queryBuilder, Proto.QueryHeader header)
        {
            var builder = new Proto.FileGetInfoQuery
            {
                Header = header
            };

            if (FileId != null)
            {
                builder.FileID = FileId.ToProtobuf();
            }

            queryBuilder.FileGetInfo = builder;
        }

        public override Proto.ResponseHeader MapResponseHeader(Proto.Response response)
        {
            return response.FileGetInfo.Header;
        }

        public override Proto.QueryHeader MapRequestHeader(Proto.Query request)
        {
            return request.FileGetInfo.Header;
        }

        public override FileInfo MapResponse(Proto.Response response, AccountId nodeId, Proto.Query request)
        {
            return FileInfo.FromProtobuf(response.FileGetInfo.FileInfo);
        }

		public override MethodDescriptor GetMethodDescriptor()
		{
			string methodname = nameof(Proto.FileService.FileServiceClient.getFileInfo);

			return Proto.FileService.Descriptor.FindMethodByName(methodname);
		}

		public override async Task<Hbar> GetCostAsync(Client client)
		{
			// deleted accounts return a COST_ANSWER of zero which triggers `INSUFFICIENT_TX_FEE`
			// if you set that as the query payment; 25 tinybar seems to be enough to get
			// `Token_DELETED` back instead.

			Hbar cost = await base.GetCostAsync(client);

			return Hbar.FromTinybars(Math.Max(cost.ToTinybars(), 25));
		}
	}
}