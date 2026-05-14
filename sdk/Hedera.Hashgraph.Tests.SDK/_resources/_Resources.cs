// SPDX-License-Identifier: Apache-2.0
using System;
using System.IO;
using System.Reflection;

namespace Hedera.Hashgraph.Tests.SDK
{
	public static class Resources
	{
        private static string FullyQualified(string name) => string.Format("Hedera.Hashgraph.Tests.SDK.{0}", name);

		private const string ClientConfig_Name = "client-config.json";
		private const string ClientConfigWithOperator_Name = "client-config-with-operator.json";
		private const string ClientConfigWithShardRealm_Name = "client-config-with-shard-realm.json";
		private const string TestKeystore_Name = "test-keystore.bin";
		private const string TestKeystore2_Name = "test-keystore2.bin";

        public static Stream ClientConfig_Stream => typeof(Resources).Assembly.GetManifestResourceStream(FullyQualified(ClientConfig_Name)) ?? throw new ArgumentNullException(ClientConfig_Name);
        public static Stream ClientConfigWithOperator_Stream => typeof(Resources).Assembly.GetManifestResourceStream(FullyQualified(ClientConfigWithOperator_Name)) ?? throw new ArgumentNullException(ClientConfigWithOperator_Name);
        public static Stream ClientConfigWithShardRealm_Stream => typeof(Resources).Assembly.GetManifestResourceStream(FullyQualified(ClientConfigWithShardRealm_Name)) ?? throw new ArgumentNullException(ClientConfigWithShardRealm_Name);
        public static Stream TestKeystore_Stream => typeof(Resources).Assembly.GetManifestResourceStream(FullyQualified(TestKeystore_Name)) ?? throw new ArgumentNullException(TestKeystore_Name);
        public static Stream TestKeystore2_Stream => typeof(Resources).Assembly.GetManifestResourceStream(FullyQualified(TestKeystore2_Name)) ?? throw new ArgumentNullException(TestKeystore2_Name);

        public static ManifestResourceInfo ClientConfig_Info => typeof(Resources).Assembly.GetManifestResourceInfo(FullyQualified(ClientConfig_Name)) ?? throw new ArgumentNullException(ClientConfig_Name);
        public static ManifestResourceInfo ClientConfigWithOperator_Info => typeof(Resources).Assembly.GetManifestResourceInfo(FullyQualified(ClientConfigWithOperator_Name)) ?? throw new ArgumentNullException(ClientConfigWithOperator_Name);
        public static ManifestResourceInfo ClientConfigWithShardRealm_Info => typeof(Resources).Assembly.GetManifestResourceInfo(FullyQualified(ClientConfigWithShardRealm_Name)) ?? throw new ArgumentNullException(ClientConfigWithShardRealm_Name);
        public static ManifestResourceInfo TestKeystore_Info => typeof(Resources).Assembly.GetManifestResourceInfo(FullyQualified(TestKeystore_Name)) ?? throw new ArgumentNullException(TestKeystore_Name);
        public static ManifestResourceInfo TestKeystore2_Info => typeof(Resources).Assembly.GetManifestResourceInfo(FullyQualified(TestKeystore2_Name)) ?? throw new ArgumentNullException(TestKeystore2_Name);

		public static class AddressBook
        {
            private const string Mainnet_Name = "mainnet.pb";
            private const string Previewnet_Name = "previewnet.pb";
            private const string Testnet_Name = "testnet.pb";

            private static readonly string Mainnet_FullName = string.Format("{0}.{1}", nameof(AddressBook), Mainnet_Name);
            private static readonly string Previewnet_FullName = string.Format("{0}.{1}", nameof(AddressBook), Previewnet_Name);
            private static readonly string Testnet_FullName = string.Format("{0}.{1}", nameof(AddressBook), Testnet_Name);

            public static Stream Mainnet_Stream => typeof(Resources).Assembly.GetManifestResourceStream(FullyQualified(Mainnet_FullName)) ?? throw new ArgumentNullException(Mainnet_FullName);
            public static Stream Previewnet_Stream => typeof(Resources).Assembly.GetManifestResourceStream(FullyQualified(Previewnet_FullName)) ?? throw new ArgumentNullException(Previewnet_FullName);
            public static Stream Testnet_Stream => typeof(Resources).Assembly.GetManifestResourceStream(FullyQualified(Testnet_FullName)) ?? throw new ArgumentNullException(Testnet_FullName);

            public static ManifestResourceInfo Mainnet_Info => typeof(Resources).Assembly.GetManifestResourceInfo(FullyQualified(Mainnet_FullName)) ?? throw new ArgumentNullException(Mainnet_FullName);
            public static ManifestResourceInfo Previewnet_Info => typeof(Resources).Assembly.GetManifestResourceInfo(FullyQualified(Previewnet_FullName)) ?? throw new ArgumentNullException(Previewnet_FullName);
            public static ManifestResourceInfo Testnet_Info => typeof(Resources).Assembly.GetManifestResourceInfo(FullyQualified(Testnet_FullName)) ?? throw new ArgumentNullException(Testnet_FullName);

            public static Stream StreamFromFileName(string fileName)
            {
                return fileName switch
                {
                    Mainnet_Name => Mainnet_Stream,
                    Previewnet_Name => Previewnet_Stream,
                    Testnet_Name => Testnet_Stream,

                    _ => throw new ArgumentNullException(fileName),
                };
            }
            public static ManifestResourceInfo InfoFromFileName(string fileName)
            {
                return fileName switch
                {
                    Mainnet_Name => Mainnet_Info,
                    Previewnet_Name => Previewnet_Info,
                    Testnet_Name => Testnet_Info,

                    _ => throw new ArgumentNullException(fileName),
                };
            }
        }
    }
}