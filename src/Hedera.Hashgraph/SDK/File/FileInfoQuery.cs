// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Ids;
using Hedera.Hashgraph.SDK.Queries;

using System;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.File
{
    /// <summary>
    /// Get all of the information about a file, except for its contents.
    /// <p>
    /// When a file expires, it no longer exists, and there will be no info about it, and the fileInfo field
    /// will be blank.
    /// <p>
    /// If a transaction or smart contract deletes the file, but it has not yet expired, then the
    /// fileInfo field will be non-empty, the deleted field will be true, its size will be 0,
    /// and its contents will be empty. Note that each file has a FileID, but does not have a filename.
    /// </summary>
    public sealed class FileInfoQuery : Query<FileInfo, FileInfoQuery>
    {
        /// <summary>
        /// Sets the file ID for which information is requested.
        /// </summary>
        /// <param name="FileId">The FileId to be set</param>
        /// <returns>{@code this}</returns>
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

        public override MethodDescriptor<Proto.Query, Proto.Response> GetMethodDescriptor()
        {
            return FileServiceGrpc.GetGetFileInfoMethod();
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