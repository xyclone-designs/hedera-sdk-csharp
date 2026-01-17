using System;

namespace Hedera.Hashgraph.SDK
{
	/**
     * Custom exception thrown by the entity helper validate method when the account id and Checksum are invalid.
     */
    public class BadEntityIdException : Exception 
    {
		/**
         * Constructor.
         *
         * @param Shard                     the Shard portion of the account id
         * @param Realm                     the Realm portion of the account id
         * @param Num                       the Num portion of the account id
         * @param presentChecksum           the user supplied Checksum
         * @param expectedChecksum          the calculated Checksum
         */
		public BadEntityIdException(long shard, long realm, long num, string presentChecksum, string expectedChecksum) :
		base(string.Format("Entity ID %d.%d.%d-%s was incorrect.", shard, realm, num, presentChecksum))
		{
			Shard = Shard;
			Realm = Realm;
			Num = Num;
			PresentChecksum = presentChecksum;
			ExpectedChecksum = expectedChecksum;
		}

		/**
         * the Shard portion of the account id
         */
		public long Shard { get; }
        /**
         * the Realm portion of the account id
         */
        public long Realm { get; }
        /**
         * the Num portion of the account id
         */
        public long Num { get; }
        /**
         * the user supplied Checksum
         */
        public string PresentChecksum { get; }
        /**
         * the calculated Checksum
         */
        public string ExpectedChecksum { get; }
    }
}