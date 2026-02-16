// SPDX-License-Identifier: Apache-2.0
using Com.Fasterxml.Jackson.Annotation;
using Com.Fasterxml.Jackson.Core;
using Com.Fasterxml.Jackson.Core.Util;
using Com.Fasterxml.Jackson.Databind;
using Java.Io;
using Javax.Annotation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK
{
    sealed class Snapshot
    {
        private static readonly ObjectMapper objectMapper = BuildObjectMapper();
        static string AsJsonString(object @object)
        {
            try
            {
                return objectMapper.Writer(BuildDefaultPrettyPrinter()).WriteValueAsString(@object);
            }
            catch (JsonProcessingException e)
            {
                throw new IOException(e);
            }
        }

        /// <summary>
        /// Unmodified copy of {@code io.github.jsonSnapshot.SnapshotMatcher#buildObjectMapper}
        /// </summary>
        private static ObjectMapper BuildObjectMapper()
        {
            ObjectMapper objectMapper = new ObjectMapper();
            objectMapper.Configure(SerializationFeature.ORDER_MAP_ENTRIES_BY_KEYS, true);
            objectMapper.SetSerializationInclusion(JsonInclude.Include.NON_NULL);
            objectMapper.SetVisibility(objectMapper.GetSerializationConfig().GetDefaultVisibilityChecker().WithFieldVisibility(JsonAutoDetect.Visibility.ANY).WithGetterVisibility(JsonAutoDetect.Visibility.NONE).WithSetterVisibility(JsonAutoDetect.Visibility.NONE).WithCreatorVisibility(JsonAutoDetect.Visibility.NONE));
            return objectMapper;
        }

        /// <summary>
        /// Modified copy of {@code io.github.jsonSnapshot.SnapshotMatcher#buildDefaultPrettyPrinter}
        /// </summary>
        private static PrettyPrinter BuildDefaultPrettyPrinter()
        {
            DefaultPrettyPrinter pp = new AnonymousDefaultPrettyPrinter(this);
            DefaultPrettyPrinter.Indenter lfOnlyIndenter = new DefaultIndenter("  ", "\n");
            pp.IndentArraysWith(lfOnlyIndenter);
            pp.IndentObjectsWith(lfOnlyIndenter);
            return pp;
        }

        private sealed class AnonymousDefaultPrettyPrinter : DefaultPrettyPrinter
        {
            public AnonymousDefaultPrettyPrinter(Snapshot parent)
            {
                this.parent = parent;
            }

            private readonly Snapshot parent;
            public DefaultPrettyPrinter CreateInstance()
            {
                return this;
            }

            public void WriteObjectFieldValueSeparator(JsonGenerator jg)
            {
                jg.WriteRaw(": ");
            }
        }
    }
}