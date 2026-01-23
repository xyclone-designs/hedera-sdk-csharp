// SPDX-License-Identifier: Apache-2.0
using Com.Google.Common.Base;
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Proto;
using Java.Time;
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

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Current information for a file, including its size.
    /// 
    /// See <a href="https://docs.hedera.com/guides/docs/sdks/file-storage/get-file-info">Hedera Documentation</a>
    /// </summary>
    public sealed class FileInfo
    {
        /// <summary>
        /// The ID of the file for which information is requested.
        /// </summary>
        public readonly FileId fileId;
        /// <summary>
        /// Number of bytes in contents.
        /// </summary>
        public readonly long size;
        /// <summary>
        /// The current time at which this account is set to expire.
        /// </summary>
        public readonly Timestamp expirationTime;
        /// <summary>
        /// True if deleted but not yet expired.
        /// </summary>
        public readonly bool isDeleted;
        /// <summary>
        /// One of these keys must sign in order to delete the file.
        /// All of these keys must sign in order to update the file.
        /// </summary>
        public readonly KeyList keys;
        /// <summary>
        /// The memo associated with the file
        /// </summary>
        public readonly string fileMemo;
        /// <summary>
        /// The ledger ID the response was returned from; please see <a href="https://github.com/hashgraph/hedera-improvement-proposal/blob/master/HIP/hip-198.md">HIP-198</a> for the network-specific IDs.
        /// </summary>
        public readonly LedgerId ledgerId;
        private FileInfo(FileId fileId, long size, Timestamp expirationTime, bool isDeleted, KeyList keys, string fileMemo, LedgerId ledgerId)
        {
            fileId = fileId;
            size = size;
            expirationTime = expirationTime;
            isDeleted = isDeleted;
            keys = keys;
            fileMemo = fileMemo;
            ledgerId = ledgerId;
        }

        /// <summary>
        /// Create a file info object from a ptotobuf.
        /// </summary>
        /// <param name="fileInfo">the protobuf</param>
        /// <returns>                         the new file info object</returns>
        static FileInfo FromProtobuf(FileGetInfoResponse.FileInfo fileInfo)
        {
            KeyList keys = fileInfo.HasKeys() ? KeyList.FromProtobuf(fileInfo.GetKeys(), null) : null;
            return new FileInfo(FileId.FromProtobuf(fileInfo.GetFileID()), fileInfo.GetSize(), Utils.TimestampConverter.FromProtobuf(fileInfo.GetExpirationTime()), fileInfo.GetDeleted(), keys, fileInfo.GetMemo(), LedgerId.FromByteString(fileInfo.GetLedgerId()));
        }

        /// <summary>
        /// Create a file info object from a byte array.
        /// </summary>
        /// <param name="bytes">the byte array</param>
        /// <returns>                         the new file info object</returns>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        public static FileInfo FromBytes(byte[] bytes)
        {
            return FromProtobuf(FileGetInfoResponse.FileInfo.Parser.ParseFrom(bytes));
        }

        /// <summary>
        /// Create the protobuf.
        /// </summary>
        /// <returns>                         the protobuf representation</returns>
        FileGetInfoResponse.FileInfo ToProtobuf()
        {
            var fileInfoBuilder = FileGetInfoResponse.FileInfo.NewBuilder().SetFileID(fileId.ToProtobuf()).SetSize(size).SetExpirationTime(Utils.TimestampConverter.ToProtobuf(expirationTime)).SetDeleted(isDeleted).SetMemo(fileMemo).SetLedgerId(ledgerId.ToByteString());
            if (keys != null)
            {
                var keyList = Proto.KeyList.NewBuilder();
                foreach (Key key in keys)
                {
                    keyList.AddKeys(key.ToProtobufKey());
                }

                fileInfoBuilder.SetKeys(keyList);
            }

            return fileInfoBuilder.Build();
        }

        public override string ToString()
        {
            return MoreObjects.ToStringHelper(this).Add("fileId", fileId).Add("size", size).Add("expirationTime", expirationTime).Add("isDeleted", isDeleted).Add("keys", keys).Add("fileMemo", fileMemo).Add("ledgerId", ledgerId).ToString();
        }

        /// <summary>
        /// Create the byte array.
        /// </summary>
        /// <returns>                         the byte array representation</returns>
        public byte[] ToBytes()
        {
            return ToProtobuf().ToByteArray();
        }
    }
}