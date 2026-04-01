// SPDX-License-Identifier: Apache-2.0
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hedera.Hashgraph.Tests.SDK
{
    sealed class Snapshot
    {
        private static readonly JsonSerializerOptions Options = BuildOptions();

        public static string AsJsonString(object obj)
        {
            try
            {
                return JsonSerializer.Serialize(obj, Options);
            }
            catch (Exception e)
            {
                throw new IOException("JSON serialization failed", e);
            }
        }

        /// <summary>
        /// Equivalent to Jackson ObjectMapper configuration
        /// </summary>
        private static JsonSerializerOptions BuildOptions()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = true, // Pretty printing
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, // Ignore nulls
                PropertyNamingPolicy = null, // Keep property names as-is
                DictionaryKeyPolicy = null // Keep dictionary keys as-is
            };
        }
    }
}