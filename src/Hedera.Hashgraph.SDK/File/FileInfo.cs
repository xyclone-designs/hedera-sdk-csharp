// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Networking;
using System;

namespace Hedera.Hashgraph.SDK.File
{
    /// <include file="FileInfo.cs.xml" path='docs/member[@name="T:FileInfo"]/*' />
    public sealed class FileInfo
    {
        private FileInfo(FileId fileId, long size, DateTimeOffset expirationTime, bool isDeleted, KeyList keys, string fileMemo, LedgerId ledgerId)
        {
            FileId = fileId;
            Size = size;
            ExpirationTime = expirationTime;
            IsDeleted = isDeleted;
            Keys = keys;
            FileMemo = fileMemo;
            LedgerId = ledgerId;
        }

        /// <include file="FileInfo.cs.xml" path='docs/member[@name="M:FileInfo.FromProtobuf(Proto.Services.FileGetInfoResponse.Types.FileInfo)"]/*' />
        public static FileInfo FromProtobuf(Proto.Services.FileGetInfoResponse.Types.FileInfo fileInfo)
        {
            return new FileInfo(
				FileId.FromProtobuf(fileInfo.FileID), 
				fileInfo.Size, 
				fileInfo.ExpirationTime.ToDateTimeOffset(), 
				fileInfo.Deleted,
				KeyList.FromProtobuf(fileInfo.Keys, null), 
				fileInfo.Memo, 
				LedgerId.FromByteString(fileInfo.LedgerId));
        }
        /// <include file="FileInfo.cs.xml" path='docs/member[@name="M:FileInfo.FromBytes(System.Byte[])"]/*' />
        public static FileInfo FromBytes(byte[] bytes)
        {
            return FromProtobuf(Proto.Services.FileGetInfoResponse.Types.FileInfo.Parser.ParseFrom(bytes));
        }

		/// <include file="FileInfo.cs.xml" path='docs/member[@name="P:FileInfo.FileId"]/*' />
		public FileId FileId { get; }
		/// <include file="FileInfo.cs.xml" path='docs/member[@name="P:FileInfo.Size"]/*' />
		public long Size { get; }
		/// <include file="FileInfo.cs.xml" path='docs/member[@name="P:FileInfo.ExpirationTime"]/*' />
		public DateTimeOffset ExpirationTime { get; }
		/// <include file="FileInfo.cs.xml" path='docs/member[@name="P:FileInfo.IsDeleted"]/*' />
		public bool IsDeleted { get; }
		/// <include file="FileInfo.cs.xml" path='docs/member[@name="P:FileInfo.Keys"]/*' />
		public KeyList Keys { get; }
		/// <include file="FileInfo.cs.xml" path='docs/member[@name="P:FileInfo.FileMemo"]/*' />
		public string FileMemo { get; }
		/// <include file="FileInfo.cs.xml" path='docs/member[@name="P:FileInfo.LedgerId"]/*' />
		public LedgerId LedgerId { get; }

		/// <include file="FileInfo.cs.xml" path='docs/member[@name="M:FileInfo.ToBytes"]/*' />
		public byte[] ToBytes()
		{
			return ToProtobuf().ToByteArray();
		}
		/// <include file="FileInfo.cs.xml" path='docs/member[@name="M:FileInfo.ToProtobuf"]/*' />
		public Proto.Services.FileGetInfoResponse.Types.FileInfo ToProtobuf()
        {
            return new Proto.Services.FileGetInfoResponse.Types.FileInfo
			{
				FileID = FileId.ToProtobuf(),
				Size = Size,
				ExpirationTime = ExpirationTime.ToProtoTimestamp(),
				Deleted = IsDeleted,
				Memo = FileMemo,
				LedgerId = LedgerId.ToByteString(),
                Keys = Keys.ToProtobuf(),
			};
        }
    }
}
