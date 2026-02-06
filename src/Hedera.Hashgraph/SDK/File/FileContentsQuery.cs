// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Queries;

using System;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.File
{
    /// <summary>
    /// Get the contents of a file. The content field is empty (no bytes) if the
    /// file is empty.
    /// 
    /// A query to get the contents of a file. Queries do not change the state of
    /// the file or require network consensus. The information is returned from a
    /// single node processing the query.
    /// 
    /// See <a href="https://docs.hedera.com/guides/docs/sdks/file-storage/get-file-contents">Hedera Documentation</a>
    /// </summary>
    public sealed class FileContentsQuery : Query<ByteString, FileContentsQuery>
    {
        /// <summary>
        /// Sets the file ID of the file whose contents are requested.
        /// </summary>
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
        public override void OnMakeRequest(Proto.Query queryBuilder, Proto.QueryHeader header)
        {
            var builder = new Proto.FileGetContentsQuery
            {
                Header = header
            };

            if (FileId != null)
            {
                builder.FileID = FileId.ToProtobuf();
            }

            queryBuilder.FileGetContents = builder;
        }
        public override Proto.ResponseHeader MapResponseHeader(Proto.Response response)
        {
            return response.FileGetContents.Header;
        }
        public override Proto.QueryHeader MapRequestHeader(Proto.Query request)
        {
            return request.FileGetContents.Header;
        }
        public override ByteString MapResponse(Proto.Response response, AccountId nodeId, Proto.Query request)
        {
            return response.FileGetContents.FileContents.Contents;
        }

        public override MethodDescriptor<Proto.Query, Proto.Response> GetMethodDescriptor()
        {
            return FileServiceGrpc.GetGetFileContentMethod();
        }
    }
}