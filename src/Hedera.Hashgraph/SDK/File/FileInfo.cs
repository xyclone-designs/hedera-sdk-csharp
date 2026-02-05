// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Keys;

using System.Collections.Generic;

namespace Hedera.Hashgraph.SDK.File
{
    /// <summary>
    /// Current information for a file, including its size.
    /// 
    /// See <a href="https://docs.hedera.com/guides/docs/sdks/file-storage/get-file-info">Hedera Documentation</a>
    /// </summary>
    public sealed class FileInfo
    {
        private FileInfo(FileId fileId, long size, Timestamp expirationTime, bool isDeleted, KeyList keys, string fileMemo, LedgerId ledgerId)
        {
            FileId = fileId;
            Size = size;
            ExpirationTime = expirationTime;
            IsDeleted = isDeleted;
            Keys = keys;
            FileMemo = fileMemo;
            LedgerId = ledgerId;
        }

        /// <summary>
        /// Create a file info object from a ptotobuf.
        /// </summary>
        /// <param name="fileInfo">the protobuf</param>
        /// <returns>                         the new file info object</returns>
        public static FileInfo FromProtobuf(Proto.FileGetInfoResponse.Types.FileInfo fileInfo)
        {
            return new FileInfo(
				FileId.FromProtobuf(fileInfo.FileID), 
				fileInfo.Size, 
				Utils.TimestampConverter.FromProtobuf(fileInfo.ExpirationTime), 
				fileInfo.Deleted,
				KeyList.FromProtobuf(fileInfo.Keys, null), 
				fileInfo.Memo, 
				LedgerId.FromByteString(fileInfo.LedgerId));
        }
        /// <summary>
        /// Create a file info object from a byte array.
        /// </summary>
        /// <param name="bytes">the byte array</param>
        /// <returns>                         the new file info object</returns>
        /// <exception cref="InvalidProtocolBufferException">when there is an issue with the protobuf</exception>
        public static FileInfo FromBytes(byte[] bytes)
        {
            return FromProtobuf(Proto.FileGetInfoResponse.Types.FileInfo.Parser.ParseFrom(bytes));
        }

		/// <summary>
		/// The ID of the file for which information is requested.
		/// </summary>
		public FileId FileId { get; }
		/// <summary>
		/// Number of bytes in contents.
		/// </summary>
		public long Size { get; }
		/// <summary>
		/// The current time at which this account is set to expire.
		/// </summary>
		public Timestamp ExpirationTime { get; }
		/// <summary>
		/// True if deleted but not yet expired.
		/// </summary>
		public bool IsDeleted { get; }
		/// <summary>
		/// One of these keys must sign in order to delete the file.
		/// All of these keys must sign in order to update the file.
		/// </summary>
		public KeyList Keys { get; }
		/// <summary>
		/// The memo associated with the file
		/// </summary>
		public string FileMemo { get; }
		/// <summary>
		/// The ledger ID the response was returned from; please see <a href="https://github.com/hashgraph/hedera-improvement-proposal/blob/master/HIP/hip-198.md">HIP-198</a> for the network-specific IDs.
		/// </summary>
		public LedgerId LedgerId { get; }

		/// <summary>
		/// Create the byte array.
		/// </summary>
		public byte[] ToBytes()
		{
			return ToProtobuf().ToByteArray();
		}
		/// <summary>
		/// Create the protobuf.
		/// </summary>
		public Proto.FileGetInfoResponse.Types.FileInfo ToProtobuf()
        {
            Proto.FileGetInfoResponse.Types.FileInfo proto = new()
            {
				FileID = FileId.ToProtobuf(),
				Size = Size,
				ExpirationTime = Utils.TimestampConverter.ToProtobuf(ExpirationTime),
				Deleted = IsDeleted,
				Memo = FileMemo,
				LedgerId = LedgerId.ToByteString(),
                Keys = new Proto.KeyList(),
			};

			if (Keys.Iterator() is IEnumerator<Key> keys)
				while (keys.MoveNext())
					proto.Keys.Keys.Add(keys.Current.ToProtobufKey());

			return proto;
        }
    }
}