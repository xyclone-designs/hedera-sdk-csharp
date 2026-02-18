// SPDX-License-Identifier: Apache-2.0
using Java.Util;
using Java.Util.Stream;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class NftMetadataGenerator
    {
        private NftMetadataGenerator()
        {
        }

        public static List<byte[]> Generate(byte metadataCount)
        {
            List<byte[]> metadatas = new ();
            for (byte i = 0; i < metadataCount; i++)
            {
                byte[] md = new[]
                {
                    i
                };
                metadatas.Add(md);
            }

            return metadatas;
        }

        public static List<byte[]> Generate(byte[] metadata, int count)
        {
            return IntStream.Range(0, count).MapToObj((i) => metadata.Clone()).Collect(Collectors.ToList());
        }

        public static List<byte[]> GenerateOneLarge()
        {
            return Collections.SingletonList(new byte[101]);
        }
    }
}