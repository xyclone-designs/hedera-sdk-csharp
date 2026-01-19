using Google.Protobuf;
using Google.Protobuf.Collections;

using System;
using System.Linq;

namespace Hedera.Hashgraph.SDK
{
	/**
     * Current information for a file, including its size.
     *
     * See <a href="https://docs.hedera.com/guides/docs/sdks/file-storage/get-file-info">Hedera Documentation</a>
     */
    public sealed class FileInfo {
        /**
         * The ID of the file for which information is requested.
         */
        public readonly FileId fileId;

        /**
         * Number of bytes in contents.
         */
        public readonly long size;

        /**
         * The current time at which this account is set to expire.
         */
        public readonly DateTimeOffset expirationTime;

        /**
         * True if deleted but not yet expired.
         */
        public readonly bool isDeleted;

        /**
         * One of these keys must sign in order to delete the file.
         * All of these keys must sign in order to update the file.
         */
        public readonly KeyList? keys;

        /**
         * The memo associated with the file
         */
        public readonly string fileMemo;

        /**
         * The ledger ID the response was returned from; please see <a href="https://github.com/hashgraph/hedera-improvement-proposal/blob/master/HIP/hip-198.md">HIP-198</a> for the network-specific IDs.
         */
        public readonly LedgerId ledgerId;

        private FileInfo(FileId fileId, long size, DateTimeOffset expirationTime, bool isDeleted, KeyList? keys, string fileMemo, LedgerId ledgerId) {
            this.fileId = fileId;
            this.size = size;
            this.expirationTime = expirationTime;
            this.isDeleted = isDeleted;
            this.keys = keys;
            this.fileMemo = fileMemo;
            this.ledgerId = ledgerId;
        }

        /**
         * Create a file info object from a ptotobuf.
         *
         * @param fileInfo                  the protobuf
         * @return                          the new file info object
         */
        static FileInfo FromProtobuf(Proto.FileGetInfoResponse.Types.FileInfo fileInfo) 
        {
            return new FileInfo(
                    FileId.FromProtobuf(fileInfo.FileID),
                    fileInfo.Size,
                    DateTimeOffsetConverter.FromProtobuf(fileInfo.ExpirationTime),
                    fileInfo.Deleted,
					KeyList.FromProtobuf(fileInfo.Keys, null),
                    fileInfo.Memo,
                    LedgerId.FromByteString(fileInfo.LedgerId));
        }

        /**
         * Create a file info object from a byte array.
         *
         * @param bytes                     the byte array
         * @return                          the new file info object
         * @   when there is an issue with the protobuf
         */
        public static FileInfo FromBytes(byte[] bytes)  
        {
            return FromProtobuf(Proto.FileGetInfoResponse.Types.FileInfo.Parser.ParseFrom(bytes));
        }

        /**
         * Create the protobuf.
         *
         * @return                          the protobuf representation
         */
        public Proto.FileGetInfoResponse.Types.FileInfo ToProtobuf()
        {
            Proto.FileGetInfoResponse.Types.FileInfo protobuf = new ()
            {
				FileID = fileId.ToProtobuf(),
				Size = size,
				ExpirationTime = DateTimeOffsetConverter.ToProtobuf(expirationTime),
				Deleted = isDeleted,
				Memo = fileMemo,
                Keys = new Proto.KeyList { }
			};

            protobuf.Keys.Keys.AddRange(keys?.Select(_ => _.ToProtobufKey()));

            return protobuf;
        }

        /**
         * Create the byte array.
         *
         * @return                          the byte array representation
         */
        public byte[] ToBytes() 
        {
            return ToProtobuf().ToByteArray();
        }
    }
}