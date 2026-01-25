// SPDX-License-Identifier: Apache-2.0
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
        private FileId fileId = null;
        /// <summary>
        /// Constructor.
        /// </summary>
        public FileInfoQuery()
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
        /// Sets the file ID for which information is requested.
        /// </summary>
        /// <param name="fileId">The FileId to be set</param>
        /// <returns>{@code this}</returns>
        public FileInfoQuery SetFileId(FileId fileId)
        {
            Objects.RequireNonNull(fileId);
            fileId = fileId;
            return this;
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
            var builder = FileGetInfoQuery.NewBuilder();
            if (fileId != null)
            {
                builder.SetFileID(fileId.ToProtobuf());
            }

            queryBuilder.SetFileGetInfo(builder.SetHeader(header));
        }

        override ResponseHeader MapResponseHeader(Response response)
        {
            return response.GetFileGetInfo().GetHeader();
        }

        override QueryHeader MapRequestHeader(Proto.Query request)
        {
            return request.GetFileGetInfo().GetHeader();
        }

        override FileInfo MapResponse(Response response, AccountId nodeId, Proto.Query request)
        {
            return FileInfo.FromProtobuf(response.GetFileGetInfo().GetFileInfo());
        }

        override MethodDescriptor<Proto.Query, Response> GetMethodDescriptor()
        {
            return FileServiceGrpc.GetGetFileInfoMethod();
        }

        public override Task<Hbar> GetCostAsync(Client client)
        {

            // deleted accounts return a COST_ANSWER of zero which triggers `INSUFFICIENT_TX_FEE`
            // if you set that as the query payment; 25 tinybar seems to be enough to get
            // `FILE_DELETED` back instead.
            return base.GetCostAsync(client).ThenApply((cost) => Hbar.FromTinybars(Math.Max(cost.ToTinybars(), 25)));
        }
    }
}