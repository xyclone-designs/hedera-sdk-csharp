// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class NftMetadataGenerator
    {
        private NftMetadataGenerator() { }

        public static List<byte[]> Generate(byte metadataCount)
        {
            List<byte[]> metadatas = new ();
            for (byte i = 0; i < metadataCount; i++)
            {
                byte[] md = [i];
                metadatas.Add(md);
            }

            return metadatas;
        }

        public static List<byte[]> Generate(byte[] metadata, int count)
        {
            return [.. Enumerable.Range(0, count).Select(_ => metadata.CopyArray())];
        }

        public static List<byte[]> GenerateOneLarge()
        {
            return [new byte[101]];
        }
    }
}