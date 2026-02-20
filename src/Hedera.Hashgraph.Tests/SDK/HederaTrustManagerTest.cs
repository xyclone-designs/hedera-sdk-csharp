// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Google.Protobuf;
using Java.Io;
using Java.Nio.Charset;
using Java.Security.Cert;
using Java.Util;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK
{
    public class HederaTrustManagerTest
    {
        public static readonly string PREVIEWNET_CERT_NODE_3_STRING = "-----BEGIN CERTIFICATE-----\n" + "MIICnzCCAiWgAwIBAgIUenyqJ4UaFBbwokatcUqAwW3o3rswCgYIKoZIzj0EAwMw\n" + "gYQxCzAJBgNVBAYTAlVTMQswCQYDVQQIDAJUWDETMBEGA1UEBwwKUmljaGFyZHNv\n" + "bjEPMA0GA1UECgwGSGVkZXJhMQ8wDQYDVQQLDAZIZWRlcmExEDAOBgNVBAMMBzAw\n" + "MDAwMDAxHzAdBgkqhkiG9w0BCQEWEGFkbWluQGhlZGVyYS5jb20wIBcNMjEwODIz\n" + "MjIyMTU4WhgPMjI5NTA2MDcyMjIxNThaMIGEMQswCQYDVQQGEwJVUzELMAkGA1UE\n" + "CAwCVFgxEzARBgNVBAcMClJpY2hhcmRzb24xDzANBgNVBAoMBkhlZGVyYTEPMA0G\n" + "A1UECwwGSGVkZXJhMRAwDgYDVQQDDAcwMDAwMDAwMR8wHQYJKoZIhvcNAQkBFhBh\n" + "ZG1pbkBoZWRlcmEuY29tMHYwEAYHKoZIzj0CAQYFK4EEACIDYgAEm5b1+oG9R0qt\n" + "zM7UZnS5l/xxUNHIHq5+NAvtlviCpJL19jrW9+/UOy00Qqc6vS6tS1hS+dNJmpiZ\n" + "FN0EHew4VDR7ACnL4LDJKmIHWjQ0iwvZo5kCpO0r9BtPN5FvaSxyo1QwUjAPBgNV\n" + "HREECDAGhwR/AAABMAsGA1UdDwQEAwIEsDATBgNVHSUEDDAKBggrBgEFBQcDATAd\n" + "BgNVHQ4EFgQUeciBviJtjeuue0GPf1xllNw7qvYwCgYIKoZIzj0EAwMDaAAwZQIw\n" + "JeG0H2HdsI1VhOYmJmYlNeKCNgAk+LMorzPmsIInVBO2HK2IrKfpReWDS/m5j51V\n" + "AjEAxKBxDezJDqAZHTkTXCg+X9Q9V6J6M5yDy5IS90aCWEo+W8C1Hc6hkn2/NrvT\n" + "PhwK\n" + "-----END CERTIFICATE-----\n";
        public static readonly ByteArrayInputStream PREVIEWNET_CERT_NODE_3_BYTES = new ByteArrayInputStream(PREVIEWNET_CERT_NODE_3_STRING.GetBytes(StandardCharsets.UTF_8));
        public static readonly CertificateFactory CERTIFICATE_FACTORY;
        static HederaTrustManagerTest()
        {
            try
            {
                CERTIFICATE_FACTORY = CertificateFactory.GetInstance("X.509");
            }
            catch (CertificateException e)
            {
                throw new Exception(e);
            }
        }

        public static readonly X509Certificate PREVIEWNET_CERT_NODE_3;
        static HederaTrustManagerTest()
        {
            try
            {
                PREVIEWNET_CERT_NODE_3 = (X509Certificate)CERTIFICATE_FACTORY.GenerateCertificate(PREVIEWNET_CERT_NODE_3_BYTES);
            }
            catch (CertificateException e)
            {
                throw new Exception(e);
            }
        }

        static readonly X509Certificate[] CERTIFICATE_CHAIN = new X509Certificate[]
        {
            PREVIEWNET_CERT_NODE_3
        };
        public virtual void SkipsCheckIfVerificationIsDisabled()
        {
            new HederaTrustManager(ByteString.EMPTY, false).CheckServerTrusted(CERTIFICATE_CHAIN, "");
        }

        public virtual void SkipsCheckIfCertificateIsNotProvided()
        {
            new HederaTrustManager(null, false).CheckServerTrusted(CERTIFICATE_CHAIN, "");
        }

        public virtual void ThrowsErrorIfCertificateIsNotProvidedButVerificationIsRequired()
        {
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => new HederaTrustManager(null, true));
        }

        public virtual void ProperlyChecksCertificateAgainstCurrentNetworkAddressBook()
        {
            var client = Client.ForNetwork(Map.Of("0.previewnet.hedera.com:50211", new AccountId(0, 0, 3))).SetTransportSecurity(true).SetVerifyCertificates(true).SetLedgerId(LedgerId.PREVIEWNET);
            var nodeAddress = client.network.addressBook)[new AccountId(0, 0, 3)]);
            new HederaTrustManager(nodeAddress.GetCertHash(), client.IsVerifyCertificates()).CheckServerTrusted(CERTIFICATE_CHAIN, "");
        }

        public virtual void CertificateCheckFailWhenHashMismatches()
        {
            var client = Client.ForNetwork(Map.Of("0.previewnet.hedera.com:50211", new AccountId(0, 0, 3))).SetTransportSecurity(true).SetVerifyCertificates(true).SetLedgerId(LedgerId.PREVIEWNET);
            var nodeAddress = client.network.addressBook)[new AccountId(0, 0, 4)]);
            CertificateException exception = Assert.Throws<CertificateException>(() => new HederaTrustManager(nodeAddress.GetCertHash(), client.IsVerifyCertificates()).CheckServerTrusted(CERTIFICATE_CHAIN, ""));
        }
    }
}