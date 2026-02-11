// SPDX-License-Identifier: Apache-2.0
using Google.Protobuf;
using Hedera.Hashgraph.SDK.Logging;
using Java.Io;
using Java.Nio.Charset;
using Java.Security;
using Java.Security.Cert;
using Javax.Annotation;
using Javax.Net.Ssl;
using Org.Bouncycastle.Util.Encoders;
using Org.Bouncycastle.Util.Io.Pem;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Utilities.IO.Pem;
using Org.Slf4j;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;
using static Hedera.Hashgraph.SDK.ExecutionState;
using static Hedera.Hashgraph.SDK.FeeAssessmentMethod;
using static Hedera.Hashgraph.SDK.FeeDataType;
using static Hedera.Hashgraph.SDK.FreezeType;
using static Hedera.Hashgraph.SDK.FungibleHookType;
using static Hedera.Hashgraph.SDK.HbarUnit;

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Internal class used by node.
    /// </summary>
    internal class HederaTrustManager : X509TrustManager
    {
        private static readonly string CERTIFICATE = "CERTIFICATE";
        private static readonly string PEM_HEADER = "-----BEGIN CERTIFICATE-----\n";
        private static readonly string PEM_FOOTER = "-----END CERTIFICATE-----\n";
        protected readonly Logger logger = LoggerFactory.GetLogger(GetType());
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
                    using (var outputStream = new ByteArrayOutputStream())
                    using (var pemWriter = new PemWriter(new OutputStreamWriter(outputStream, StandardCharsets.UTF_8)))
                    {
                        pemWriter.WriteObject(new PemObject(CERTIFICATE, cert.GetEncoded()));
                        pemWriter.Flush();
                        pem = outputStream.ToByteArray();
                    }
                }
                catch (IOException e)
                {
                    logger.Warn("Failed to write PEM to byte array: ", e);
                    continue;
                }

                var certHashBytes = new byte[0];
                try
                {
                    certHashBytes = MessageDigest.GetInstance("SHA-384").Digest(pem);
                }
                catch (NoSuchAlgorithmException e)
                {
                    throw new InvalidOperationException("Failed to find SHA-384 digest for certificate hashing", e);
                }

                if (CertHash.Equals(Hex.ToHexString(certHashBytes)))
                {
                    return;
                }
            }

            throw new CertificateException("Failed to confirm the server's certificate from a known address book");
        }

        public virtual X509Certificate[] GetAcceptedIssuers()
        {
            return new X509Certificate[0];
        }
    }
}