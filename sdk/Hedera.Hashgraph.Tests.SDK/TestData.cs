namespace Hedera.Hashgraph.Tests.SDK
{
    public static class TestData
    {
        // Standard test IDs
        public const string DEFAULT_ENTITY_ID = "0.0.5005";
        public const string SECONDARY_ENTITY_ID = "0.0.5006";
        public const string TRANSFER_ACCOUNT_1 = "0.0.5007";
        public const string TRANSFER_ACCOUNT_2 = "0.0.5008";
        public const string STANDARD_ENTITY_ID = "0.0.3";
        public const string AUTO_RENEW_ACCOUNT = "0.0.30";
        public const string TEST_ID_WITH_CHECKSUM = "0.0.123";

        // Token and NFT IDs
        public const string TOKEN_ID_1 = "0.0.5";
        public const string TOKEN_ID_2 = "0.0.4";
        public const string TOKEN_ID_3 = "0.0.3";
        public const string TOKEN_ID_4 = "0.0.2";

        // Checksum validation data - Network specific checksums for ID "0.0.123"
        public const string MAINNET_CHECKSUM = "vfmkw";
        public const string TESTNET_CHECKSUM = "esxsf";
        public const string PREVIEWNET_CHECKSUM = "ogizo";
        public const string BAD_CHECKSUM = "ntjli";

        // Full IDs with checksums
        public static string TEST_ID_MAINNET => $"{TEST_ID_WITH_CHECKSUM}-{MAINNET_CHECKSUM}";
        public static string TEST_ID_TESTNET => $"{TEST_ID_WITH_CHECKSUM}-{TESTNET_CHECKSUM}";
        public static string TEST_ID_PREVIEWNET => $"{TEST_ID_WITH_CHECKSUM}-{PREVIEWNET_CHECKSUM}";
        public static string TEST_ID_BAD_CHECKSUM => $"{TEST_ID_WITH_CHECKSUM}-{BAD_CHECKSUM}";

        // Alias keys for testing
        public const string ALIAS_KEY_HEX = "0.0.302a300506032b6570032100114e6abc371b82dab5c15ea149f02d34a012087b163516dd70f44acafabf7777";
        public const string ECDSA_ALIAS_KEY_HEX = "0.0.302d300706052b8104000a032200035d348292bbb8b511fdbe24e3217ec099944b4728999d337f9a025f4193324525";
        public const string EVM_ADDRESS_SHORT = "0.0.302a300506032b6570032100114e6abc371b82da";

        // EVM addresses
        public const string EVM_ADDRESS_HEX = "302a300506032b6570032100114e6abc371b82da";
        public const string EVM_ADDRESS_NORMAL = "742d35Cc6634C0532925a3b844Bc454e4438f44e";
        public const string EVM_ADDRESS_LONG_ZERO = "00000000000000000000000000000000000004d2";
        public const string SOLIDITY_ADDRESS = "000000000000000000000000000000000000138D";
        public const string EVM_ALIAS = "0x5c562e90feaf0eebd33ea75d21024f249d451417";

        // Common Hbar amounts (in tinybars)
        public const long HBAR_100000 = 100000;
        public const long HBAR_1000 = 1000;
        public const long HBAR_450 = 450;
        public const long HBAR_400 = 400;
        public const long HBAR_800 = 800;

        // Malformed ID patterns for error testing
        public const string MALFORMED_ID_EMPTY = "0.0.";
        public const string MALFORMED_CHECKSUM_SHORT = "0.0.123-ntjl";
        public const string MALFORMED_CHECKSUM_LONG = "0.0.123-ntjl1";
        public const string MALFORMED_ALIAS_KEY = "0.0.302a300506032b6570032100114e6abc371b82dab5c15ea149f02d34a012087b163516dd70f44acafabf777";
        public const string MALFORMED_ALIAS_KEY_INVALID_CHAR = "0.0.302a300506032b6570032100114e6abc371b82dab5c15ea149f02d34a012087b163516dd70f44acafabf777g";
        public const string MALFORMED_ALIAS_KEY_EXTRA_BYTE = "0.0.303a300506032b6570032100114e6abc371b82dab5c15ea149f02d34a012087b163516dd70f44acafabf7777";

        // ED25519 key data
        public const string ED25519_PUBLIC_KEY_HEX = "302a300506032b6570032100114e6abc371b82dab5c15ea149f02d34a012087b163516dd70f44acafabf7777";
        public const string ED25519_RAW_KEY_HEX = "114e6abc371b82dab5c15ea149f02d34a012087b163516dd70f44acafabf7777";
    }
}
