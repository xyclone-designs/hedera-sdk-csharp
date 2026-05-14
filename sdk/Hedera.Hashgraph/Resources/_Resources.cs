// SPDX-License-Identifier: Apache-2.0
using System;
using System.IO;
using System.Reflection;

namespace Hedera.Hashgraph
{
	public static class Resources
	{
        private static string FullyQualified(string name) => string.Format("Hedera.Hashgraph.Resources.{0}", name);

		private const string BIP29_English_Name = "bip39-english.txt";
		private const string Legacy_English_Name = "legacy-english.txt";

        public static Stream BIP29_English_Stream => typeof(Resources).Assembly.GetManifestResourceStream(FullyQualified(BIP29_English_Name)) ?? throw new ArgumentNullException(BIP29_English_Name);
        public static Stream Legacy_English_Stream => typeof(Resources).Assembly.GetManifestResourceStream(FullyQualified(Legacy_English_Name)) ?? throw new ArgumentNullException(Legacy_English_Name);

        public static ManifestResourceInfo BIP29_English_Info => typeof(Resources).Assembly.GetManifestResourceInfo(FullyQualified(BIP29_English_Name)) ?? throw new ArgumentNullException(BIP29_English_Name);
        public static ManifestResourceInfo Legacy_English_Info => typeof(Resources).Assembly.GetManifestResourceInfo(FullyQualified(Legacy_English_Name)) ?? throw new ArgumentNullException(Legacy_English_Name);

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