// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Io.Github.JsonSnapshot;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK.Token
{
    public class TokenRelationshipTest
    {
        public static void BeforeAll()
        {
            SnapshotMatcher.Start(Snapshot.AsJsonString());
        }

        public static void AfterAll()
        {
            SnapshotMatcher.ValidateSnapshots();
        }

        public virtual TokenRelationship SpawnTokenRelationshipExample()
        {
            return new TokenRelationship(TokenId.FromString("1.2.3"), "ABC", 55, true, true, 4, true);
        }

        public virtual void ShouldSerializeTokenRelationship()
        {
            var originalTokenRelationship = SpawnTokenRelationshipExample();
            byte[] tokenRelationshipBytes = originalTokenRelationship.ToBytes();
            var copyTokenRelationship = TokenRelationship.FromBytes(tokenRelationshipBytes);
            Assert.Equal(copyTokenRelationship.ToString().ReplaceAll("@[A-Za-z0-9]+", ""), originalTokenRelationship.ToString().ReplaceAll("@[A-Za-z0-9]+", ""));
            SnapshotMatcher.Expect(originalTokenRelationship.ToString().ReplaceAll("@[A-Za-z0-9]+", "")).ToMatchSnapshot();
        }
    }
}