// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;

using Hedera.Hashgraph.SDK.Logging;

using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.Utilities.Encoders;

using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Internal class used by node.
    /// </summary>
    internal class HederaTrustManager //: X509TrustManager
    {
        private static readonly string CERTIFICATE = "CERTIFICATE";
        private static readonly string PEM_HEADER = "-----BEGIN CERTIFICATE-----\n";
        private static readonly string PEM_FOOTER = "-----END CERTIFICATE-----\n";
        protected readonly Logger logger = LoggerFactory.GetLogger(typeof(HederaTrustManager));
        public readonly string? CertHash;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="certHash">a byte string of the certificate hash</param>
        /// <param name="verifyCertificate">should be verified</param>
        public HederaTrustManager(ByteString certHash, bool verifyCertificate)
        {
            if (certHash == null || certHash.Length == 0)
            {
                if (verifyCertificate)
                {
                    throw new InvalidOperationException("transport security and certificate verification are enabled, but no applicable address book was found");
                }

                logger.Warn("skipping certificate check since no cert hash was found");
				CertHash = null;
            }
            else
            {
				CertHash = Encoding.UTF8.GetString(certHash.ToByteArray());
            }
        }

        public virtual void CheckClientTrusted(X509Certificate[] chain, string authType)
        {
            throw new NotSupportedException("Attempted to use HederaTrustManager to verify a client, but this trust manager is for verifying server only");
        }
        public virtual void CheckServerTrusted(X509Certificate[] chain, string authType)
        {
            if (CertHash == null)
            {
                return;
            }

			foreach (var cert in chain)
			{
				byte[] pem;

				try
				{
                    using MemoryStream memoryStream = new ();
					using StreamWriter streamWriter = new (memoryStream, Encoding.UTF8, leaveOpen: true);
                    using PemWriter pemWriter = new (streamWriter);

					pemWriter.WriteObject(cert); // X509Certificate
					pemWriter.Writer.Flush();
					streamWriter.Flush();

					pem = memoryStream.ToArray();
				}
				catch (IOException ex)
				{
					logger.Warn("Failed to write PEM to byte array: ", ex);
					continue;
				}

				byte[] certHashBytes;

				try
                {
                    certHashBytes = SHA384.HashData(pem);
                }
                catch (Exception ex)
				{
					throw new InvalidOperationException("Failed to find SHA-384 digest for certificate hashing", ex);
				}

				var certHashHex = Hex.ToHexString(certHashBytes);

				if (string.Equals(CertHash, certHashHex, StringComparison.OrdinalIgnoreCase))
					return;
			}

            throw new CertificateException("Failed to confirm the server's certificate from a known address book");
        }

        public virtual X509Certificate[] GetAcceptedIssuers()
        {
            return new X509Certificate[0];
        }
    }
}