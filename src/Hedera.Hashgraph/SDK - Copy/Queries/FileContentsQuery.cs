// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Proto;
using Io.Grpc;
using Java.Util;
using Java.Util.Concurrent;
using Javax.Annotation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;
using static Hedera.Hashgraph.SDK.ExecutionState;
using static Hedera.Hashgraph.SDK.FeeAssessmentMethod;
using static Hedera.Hashgraph.SDK.FeeDataType;

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

        override void OnMakeRequest(Proto.Query.Builder queryBuilder, QueryHeader header)
        {
            var builder = FileGetContentsQuery.NewBuilder();
            if (fileId != null)
            {
                builder.FileID(fileId.ToProtobuf());
            }

            queryBuilder.SetFileGetContents(builder.Header(header));
        }

        override ResponseHeader MapResponseHeader(Response response)
        {
            return response.GetFileGetContents().GetHeader();
        }

        override QueryHeader MapRequestHeader(Proto.Query request)
        {
            return request.GetFileGetContents().GetHeader();
        }

        override ByteString MapResponse(Response response, AccountId nodeId, Proto.Query request)
        {
            return response.GetFileGetContents().GetFileContents().GetContents();
        }

        override MethodDescriptor<Proto.Query, Response> GetMethodDescriptor()
        {
            return FileServiceGrpc.GetGetFileContentMethod();
        }
    }
}