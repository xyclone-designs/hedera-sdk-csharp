using System.Security.Cryptography.X509Certificates;

namespace Hedera.Hashgraph.SDK
{
	/**
     * Internal class used by node.
     */
    class HederaTrustManager : X509TrustManager {
        private static readonly string CERTIFICATE = "CERTIFICATE";
        private static readonly string PEM_HEADER = "-----BEGIN CERTIFICATE-----\n";
        private static readonly string PEM_FOOTER = "-----END CERTIFICATE-----\n";
        protected readonly Logger logger = LoggerFactory.getLogger(getClass());

        private readonly string? certHash;

        /**
         * Constructor.
         *
         * @param certHash                  a byte string of the certificate hash
         * @param verifyCertificate         should be verified
         */
        HederaTrustManager(@Nullable ByteString certHash, bool verifyCertificate) {
            if (certHash == null || certHash.isEmpty()) {
                if (verifyCertificate) {
                    throw new IllegalStateException(
                            "transport security and certificate verification are enabled, but no applicable address book was found");
                }

                logger.warn("skipping certificate check since no cert hash was found");
                this.certHash = null;
            } else {
                this.certHash = new string(certHash.ToByteArray(), StandardCharsets.UTF_8);
            }
        }

        @Override
        public void checkClientTrusted(X509Certificate[] chain, string authType) {
            throw new UnsupportedOperationException(
                    "Attempted to use HederaTrustManager to verify a client, but this trust manager is for verifying server only");
        }

        @Override
        public void checkServerTrusted(X509Certificate[] chain, string authType)  {
            if (certHash == null) {
                return;
            }

            for (var cert : chain) {
                byte[] pem;

                try (var outputStream = new ByteArrayOutputStream();
                        var pemWriter = new PemWriter(new OutputStreamWriter(outputStream, StandardCharsets.UTF_8))) {
                    pemWriter.writeObject(new PemObject(CERTIFICATE, cert.getEncoded()));
                    pemWriter.flush();

                    pem = outputStream.ToByteArray();
                } catch (IOException e) {
                    logger.warn("Failed to write PEM to byte array: ", e);
                    continue;
                }

                var certHashBytes = new byte[0];

                try {
                    certHashBytes = MessageDigest.getInstance("SHA-384").digest(pem);
                } catch (NoSuchAlgorithmException e) {
                    throw new IllegalStateException("Failed to find SHA-384 digest for certificate hashing", e);
                }

                if (this.certHash.equals(Hex.toHexString(certHashBytes))) {
                    return;
                }
            }

            throw new CertificateException("Failed to confirm the server's certificate from a known address book");
        }

        @Override
        public X509Certificate[] getAcceptedIssuers() {
            return new X509Certificate[0];
        }
    }
}