// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Ids;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK.Queries
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
        private FileId fileId = null;
        /// <summary>
        /// Constructor.
        /// </summary>
        public FileContentsQuery()
        {
        }

        /// <summary>
        /// Extract the file id.
        /// </summary>
        /// <returns>                         the file id</returns>
        public FileId GetFileId()
        {
            return fileId;
        }

        /// <summary>
        /// Sets the file ID of the file whose contents are requested.
        /// </summary>
        /// <param name="fileId">The FileId to be set</param>
        /// <returns>{@code this}</returns>
        public FileContentsQuery SetFileId(FileId fileId)
        {
            ArgumentNullException.ThrowIfNull(fileId);
            fileId = fileId;
            return this;
        }

        public override Task<Hbar> GetCostAsync(Client client)
        {

            // deleted accounts return a COST_ANSWER of zero which triggers `INSUFFICIENT_TX_FEE`
            // if you set that as the query payment; 25 tinybar seems to be enough to get
            // `FILE_DELETED` back instead.
            return base.GetCostAsync(client).ThenApply((cost) => Hbar.FromTinybars(Math.Max(cost.ToTinybars(), 25)));
        }

        override void ValidateChecksums(Client client)
        {
            if (fileId != null)
            {
                fileId.ValidateChecksum(client);
            }
        }

        override void OnMakeRequest(Proto.Query queryBuilder, Proto.QueryHeader header)
        {
            var builder = new Proto.FileGetContentsQuery
            {
                Header = header
            };

            if (fileId != null)
            {
                builder.FileID = fileId.ToProtobuf();
            }

            queryBuilder.FileGetContents = builder;
        }

        override Proto.ResponseHeader MapResponseHeader(Proto.Response response)
        {
            return response.FileGetContents.Header;
        }

        override Proto.QueryHeader MapRequestHeader(Proto.Query request)
        {
            return request.FileGetContents.Header;
        }

        override ByteString MapResponse(Proto.Response response, AccountId nodeId, Proto.Query request)
        {
            return response.FileGetContents.FileContents.Contents;
        }

        override MethodDescriptor<Proto.Query, Proto.Response> GetMethodDescriptor()
        {
            return FileServiceGrpc.GetGetFileContentMethod();
        }
    }
}