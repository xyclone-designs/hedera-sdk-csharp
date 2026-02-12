// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Com.Hedera.Hashgraph.Sdk;
using Java.Util;
using Org.Bouncycastle.Util.Encoders;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.SDK.Tests.Integration
{
    public class TopicMessageSubmitIntegrationTest
    {
        virtual void CanSubmitATopicMessage()
        {
            using (var testEnv = new IntegrationTestEnv(1))
            {
                var response = new TopicCreateTransaction().SetAdminKey(testEnv.operatorKey).SetTopicMemo("[e2e::TopicCreateTransaction]").Execute(testEnv.client);
                var topicId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).topicId);
                var info = new TopicInfoQuery().SetTopicId(topicId).Execute(testEnv.client);
                Assert.Equal(info.topicId, topicId);
                Assert.Equal(info.topicMemo, "[e2e::TopicCreateTransaction]");
                Assert.Equal(info.sequenceNumber, 0);
                Assert.Equal(info.adminKey, testEnv.operatorKey);
                new TopicMessageSubmitTransaction().SetTopicId(topicId).SetMessage("Hello, from HCS!").Execute(testEnv.client).GetReceipt(testEnv.client);
                info = new TopicInfoQuery().SetTopicId(topicId).Execute(testEnv.client);
                Assert.Equal(info.topicId, topicId);
                Assert.Equal(info.topicMemo, "[e2e::TopicCreateTransaction]");
                Assert.Equal(info.sequenceNumber, 1);
                Assert.Equal(info.adminKey, testEnv.operatorKey);
                new TopicDeleteTransaction().SetTopicId(topicId).Execute(testEnv.client).GetReceipt(testEnv.client);
            }
        }

        virtual void CanSubmitALargeTopicMessage()
        {

            // Skip if using PreviewNet
            Assumptions.AssumeTrue(!System.GetProperty("HEDERA_NETWORK").Equals("previewnet"));
            AssertThatNoException().IsThrownBy(() =>
            {
                using (var testEnv = new IntegrationTestEnv(1))
                {
                    var response = new TopicCreateTransaction().SetAdminKey(testEnv.operatorKey).SetTopicMemo("[e2e::TopicCreateTransaction]").Execute(testEnv.client);
                    var topicId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).topicId);
                    Thread.Sleep(5000);
                    var info = new TopicInfoQuery().SetTopicId(topicId).Execute(testEnv.client);
                    Assert.Equal(info.topicId, topicId);
                    Assert.Equal(info.topicMemo, "[e2e::TopicCreateTransaction]");
                    Assert.Equal(info.sequenceNumber, 0);
                    Assert.Equal(info.adminKey, testEnv.operatorKey);
                    var responses = new TopicMessageSubmitTransaction().SetTopicId(topicId).SetMaxChunks(15).SetMessage(Contents.BIG_CONTENTS).ExecuteAll(testEnv.client);
                    foreach (var resp in responses)
                    {
                        resp.GetReceipt(testEnv.client);
                    }

                    info = new TopicInfoQuery().SetTopicId(topicId).Execute(testEnv.client);
                    Assert.Equal(info.topicId, topicId);
                    Assert.Equal(info.topicMemo, "[e2e::TopicCreateTransaction]");
                    Assert.Equal(info.sequenceNumber, 14);
                    Assert.Equal(info.adminKey, testEnv.operatorKey);
                    new TopicDeleteTransaction().SetTopicId(topicId).Execute(testEnv.client).GetReceipt(testEnv.client);
                }
            });
        }

        virtual void CannotSubmitMessageWhenTopicIDIsNotSet()
        {

            // Skip if using PreviewNet
            Assumptions.AssumeTrue(!System.GetProperty("HEDERA_NETWORK").Equals("previewnet"));
            AssertThatNoException().IsThrownBy(() =>
            {
                using (var testEnv = new IntegrationTestEnv(1))
                {
                    var response = new TopicCreateTransaction().SetAdminKey(testEnv.operatorKey).SetTopicMemo("[e2e::TopicCreateTransaction]").Execute(testEnv.client);
                    var topicId = Objects.RequireNonNull(response.GetReceipt(testEnv.client).topicId);
                    AssertThatExceptionOfType(typeof(PrecheckStatusException)).IsThrownBy(() =>
                    {
                        new TopicMessageSubmitTransaction().SetMessage(Contents.BIG_CONTENTS).SetMaxChunks(15).Execute(testEnv.client).GetReceipt(testEnv.client);
                    }).WithMessageContaining(Status.INVALID_TOPIC_ID.ToString());
                    new TopicDeleteTransaction().SetTopicId(topicId).Execute(testEnv.client).GetReceipt(testEnv.client);
                }
            });
        }

        virtual void DecodeHexRegressionTest()
        {
            string binaryHex = "2ac2010a580a130a0b08d38f8f880610a09be91512041899e11c120218041880c2d72f22020878da01330a0418a5a1201210303030303030313632373633373731351a190a130a0b08d38f8f880610a09be91512041899e11c1001180112660a640a20603edaec5d1c974c92cb5bee7b011310c3b84b13dc048424cd6ef146d6a0d4a41a40b6a08f310ee29923e5868aac074468b2bde05da95a806e2f4a4f452177f129ca0abae7831e595b5beaa1c947e2cb71201642bab33fece5184b04547afc40850a";
            byte[] transactionBytes = Hex.Decode(binaryHex);
            var transaction = Objects.RequireNonNull(Transaction.FromBytes(transactionBytes));
            string idString = Objects.RequireNonNull(transaction.GetTransactionId()).ToString();
            string transactionString = transaction.ToString();
        }
    }
}