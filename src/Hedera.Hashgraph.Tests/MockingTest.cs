// SPDX-License-Identifier: Apache-2.0
using Com.Google.Protobuf;
using Com.Hedera.Hashgraph.Sdk.Proto;
using Io.Grpc;
using Io.Grpc.Inprocess;
using Io.Grpc.Stub;
using Java.Nio.Charset;
using Java.Time;
using Java.Util;
using Java.Util.Concurrent;
using Java.Util.Function;
using Org.Junit.Jupiter.Api;
using Org.Junit.Jupiter.Params;
using Org.Junit.Jupiter.Params.Provider;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Com.Hedera.Hashgraph.Sdk
{
    class MockingTest
    {
        virtual void TestSucceedsWithCorrectHbars()
        {
            IList<object> responses1 = List.Of(Status.Code.UNAVAILABLE.ToStatus().AsRuntimeException(), (Function<object, object>)(o) => Status.Code.UNAVAILABLE.ToStatus().AsRuntimeException(), Response.NewBuilder().SetCryptogetAccountBalance(CryptoGetAccountBalanceResponse.NewBuilder().SetHeader(ResponseHeader.NewBuilder().SetNodeTransactionPrecheckCode(ResponseCodeEnum.OK).Build()).SetAccountID(AccountID.NewBuilder().SetAccountNum(10).Build()).SetBalance(100).Build()).Build());
            var responses = List.Of(responses1);
            using (var mocker = Mocker.WithResponses(responses))
            {
                var balance = new AccountBalanceQuery().SetAccountId(new AccountId(0, 0, 10)).Execute(mocker.client);
                Assertions.AssertEquals(balance.hbars, Hbar.FromTinybars(100));
            }
        }

        virtual string MakeBigString(int size)
        {
            char[] chars = new char[size];
            Arrays.Fill(chars, 'A');
            return new string (chars);
        }

        virtual void ShouldRetryExceptionallyFunctionsCorrectlyForPlatformNotActiveGetReceipt(string sync)
        {
            var service = new TestCryptoService();
            var server = new TestServer("getReceiptRetry" + sync, service);
            server.client.SetMaxAttempts(3);
            service.buffer.EnqueueResponse(TestResponse.Transaction(com.hedera.hashgraph.sdk.Status.PLATFORM_NOT_ACTIVE));
            service.buffer.EnqueueResponse(TestResponse.TransactionOk());
            com.hedera.hashgraph.sdk.TransactionResponse transactionResponse;
            if (sync.Equals("sync"))
            {
                transactionResponse = new AccountCreateTransaction().Execute(server.client);
            }
            else
            {
                transactionResponse = new AccountCreateTransaction().ExecuteAsync(server.client).Get();
            }

            service.buffer.EnqueueResponse(TestResponse.Query(Response.NewBuilder().SetTransactionGetReceipt(TransactionGetReceiptResponse.NewBuilder().SetHeader(ResponseHeader.NewBuilder().SetNodeTransactionPrecheckCode(ResponseCodeEnum.PLATFORM_NOT_ACTIVE)).SetReceipt(TransactionReceipt.NewBuilder().SetStatus(ResponseCodeEnum.SUCCESS).Build()).Build()).Build())).EnqueueResponse(TestResponse.Receipt(com.hedera.hashgraph.sdk.Status.PLATFORM_NOT_ACTIVE)).EnqueueResponse(TestResponse.SuccessfulReceipt());
            if (sync.Equals("sync"))
            {
                transactionResponse.GetReceipt(server.client);
            }
            else
            {
                transactionResponse.GetReceiptAsync(server.client).Get();
            }
        }

        virtual void ShouldRetryExceptionallyFunctionsCorrectlyForPlatformNotActiveGetRecord(string sync)
        {
            var service = new TestCryptoService();
            var server = new TestServer("getRecordRetry" + sync, service);
            server.client.SetMaxAttempts(3);
            service.buffer.EnqueueResponse(TestResponse.Transaction(com.hedera.hashgraph.sdk.Status.PLATFORM_NOT_ACTIVE));
            service.buffer.EnqueueResponse(TestResponse.TransactionOk());
            com.hedera.hashgraph.sdk.TransactionResponse transactionResponse;
            if (sync.Equals("sync"))
            {
                transactionResponse = new AccountCreateTransaction().Execute(server.client);
            }
            else
            {
                transactionResponse = new AccountCreateTransaction().ExecuteAsync(server.client).Get();
            }

            service.buffer.EnqueueResponse(TestResponse.SuccessfulReceipt()).EnqueueResponse(TestResponse.SuccessfulReceipt()).EnqueueResponse(TestResponse.Query(Response.NewBuilder().SetTransactionGetRecord(TransactionGetRecordResponse.NewBuilder().SetHeader(ResponseHeader.NewBuilder().SetNodeTransactionPrecheckCode(ResponseCodeEnum.PLATFORM_NOT_ACTIVE).Build()).SetTransactionRecord(TransactionRecord.NewBuilder().SetReceipt(TransactionReceipt.NewBuilder().SetStatus(ResponseCodeEnum.SUCCESS).Build()).Build()).Build()).Build())).EnqueueResponse(TestResponse.Query(Response.NewBuilder().SetTransactionGetRecord(TransactionGetRecordResponse.NewBuilder().SetTransactionRecord(TransactionRecord.NewBuilder().SetReceipt(TransactionReceipt.NewBuilder().SetStatus(ResponseCodeEnum.PLATFORM_NOT_ACTIVE).Build()).Build()).Build()).Build())).EnqueueResponse(TestResponse.Query(Response.NewBuilder().SetTransactionGetRecord(TransactionGetRecordResponse.NewBuilder().SetTransactionRecord(TransactionRecord.NewBuilder().SetReceipt(TransactionReceipt.NewBuilder().SetStatus(ResponseCodeEnum.SUCCESS).Build()).Build()).Build()).Build()));
            if (sync.Equals("sync"))
            {
                transactionResponse.GetRecord(server.client);
            }
            else
            {
                transactionResponse.GetRecordAsync(server.client).Get();
            }
        }

        virtual void ContractCreateFlowFunctions(string versionToTest, string stakeType)
        {
            var BIG_BYTECODE = MakeBigString(ContractCreateFlow.FILE_CREATE_MAX_BYTES + 1000);
            var adminKey = PrivateKey.GenerateED25519().GetPublicKey();
            var cryptoService = new TestCryptoService();
            var fileService = new TestFileService();
            var contractService = new TestContractService();
            var server = new TestServer("contractCreateFlow" + versionToTest + stakeType, cryptoService, fileService, contractService);
            var fileId = FileId.FromString("1.2.3");
            var maxAutomaticTokenAssociations = 101;
            var stakedAccountId = AccountId.FromString("4.3.2");
            var stakedNode = 13;
            var declineStakingReward = true;
            cryptoService.buffer.EnqueueResponse(TestResponse.Query(Response.NewBuilder().SetTransactionGetReceipt(TransactionGetReceiptResponse.NewBuilder().SetReceipt(TransactionReceipt.NewBuilder().SetFileID(fileId.ToProtobuf()).SetStatus(ResponseCodeEnum.SUCCESS).Build()).Build()).Build())).EnqueueResponse(TestResponse.SuccessfulReceipt()).EnqueueResponse(TestResponse.SuccessfulReceipt());
            fileService.buffer.EnqueueResponse(TestResponse.TransactionOk()).EnqueueResponse(TestResponse.TransactionOk()).EnqueueResponse(TestResponse.TransactionOk());
            contractService.buffer.EnqueueResponse(TestResponse.TransactionOk());
            var flow = new ContractCreateFlow().SetBytecode(BIG_BYTECODE).SetContractMemo("memo goes here").SetConstructorParameters(new byte[] { 1, 2, 3 }).SetAutoRenewPeriod(Duration.OfMinutes(1)).SetAdminKey(adminKey).SetGas(100).SetInitialBalance(new Hbar(3)).SetMaxAutomaticTokenAssociations(maxAutomaticTokenAssociations).SetDeclineStakingReward(declineStakingReward);
            if (stakeType.Equals("stakedAccount"))
            {
                flow.SetStakedAccountId(stakedAccountId);
            }
            else
            {
                flow.SetStakedNodeId(stakedNode);
            }

            if (versionToTest.Equals("sync"))
            {
                flow.Execute(server.client);
            }
            else
            {
                flow.ExecuteAsync(server.client).Get();
            }

            Thread.Sleep(1000);
            Assertions.AssertEquals(3, cryptoService.buffer.queryRequestsReceived.Count);
            Assertions.AssertEquals(3, fileService.buffer.transactionRequestsReceived.Count);
            Assertions.AssertEquals(1, contractService.buffer.transactionRequestsReceived.Count);
            var transactions = new List<com.hedera.hashgraph.sdk.Transaction<?>>();
            foreach (var request in fileService.buffer.transactionRequestsReceived)
            {
                transactions.Add(com.hedera.hashgraph.sdk.Transaction.FromBytes(request.ToByteArray()));
            }

            transactions.Add(com.hedera.hashgraph.sdk.Transaction.FromBytes(contractService.buffer.transactionRequestsReceived[0].ToByteArray()));
            Assertions.AssertInstanceOf(typeof(FileCreateTransaction), transactions[0]);
            Assertions.AssertEquals(ContractCreateFlow.FILE_CREATE_MAX_BYTES, ((FileCreateTransaction)transactions[0]).GetContents().Count);
            Assertions.AssertTrue(cryptoService.buffer.queryRequestsReceived[0].HasTransactionGetReceipt());
            Assertions.AssertInstanceOf(typeof(FileAppendTransaction), transactions[1]);
            var fileAppendTx = (FileAppendTransaction)transactions[1];
            Assertions.AssertEquals(fileId, fileAppendTx.GetFileId());
            Assertions.AssertEquals(BIG_BYTECODE.Length - ContractCreateFlow.FILE_CREATE_MAX_BYTES, fileAppendTx.GetContents().Count);
            Assertions.AssertInstanceOf(typeof(ContractCreateTransaction), transactions[3]);
            var contractCreateTx = (ContractCreateTransaction)transactions[3];
            Assertions.AssertEquals("memo goes here", contractCreateTx.GetContractMemo());
            Assertions.AssertEquals(fileId, contractCreateTx.GetBytecodeFileId());
            Assertions.AssertEquals(ByteString.CopyFrom(new byte[] { 1, 2, 3 }), contractCreateTx.GetConstructorParameters());
            Assertions.AssertEquals(Duration.OfMinutes(1), contractCreateTx.GetAutoRenewPeriod());
            Assertions.AssertEquals(adminKey, contractCreateTx.GetAdminKey());
            Assertions.AssertEquals(100, contractCreateTx.GetGas());
            Assertions.AssertEquals(new Hbar(3), contractCreateTx.GetInitialBalance());
            Assertions.AssertEquals(maxAutomaticTokenAssociations, contractCreateTx.GetMaxAutomaticTokenAssociations());
            Assertions.AssertEquals(declineStakingReward, contractCreateTx.GetDeclineStakingReward());
            if (stakeType.Equals("stakedAccount"))
            {
                Assertions.AssertEquals(stakedAccountId, contractCreateTx.GetStakedAccountId());
            }
            else
            {
                Assertions.AssertEquals(stakedNode, contractCreateTx.GetStakedNodeId());
            }

            Assertions.AssertInstanceOf(typeof(FileDeleteTransaction), transactions[2]);
            server.Dispose();
        }

        virtual void AccountInfoFlowFunctions()
        {
            var BIG_BYTES = MakeBigString(1000).GetBytes(StandardCharsets.UTF_8);
            var privateKey = PrivateKey.GenerateED25519();
            var otherPrivateKey = PrivateKey.GenerateED25519();
            var accountId = AccountId.FromString("1.2.3");
            var cost = Hbar.From(1);
            Supplier<TokenMintTransaction> makeTx = () => new TokenMintTransaction().SetTokenId(TokenId.FromString("1.2.3")).SetAmount(5).SetTransactionId(TransactionId.Generate(accountId)).SetNodeAccountIds(List.Of(AccountId.FromString("0.0.3"))).Freeze();
            var properlySignedTx = makeTx.Get().Sign(privateKey);
            var improperlySignedTx = makeTx.Get().Sign(otherPrivateKey);
            var properBigBytesSignature = privateKey.Sign(BIG_BYTES);
            var improperBigBytesSignature = otherPrivateKey.Sign(BIG_BYTES);
            var cryptoService = new TestCryptoService();
            var server = new TestServer("accountInfoFlow", cryptoService);
            for (int i = 0; i < 8; i++)
            {
                cryptoService.buffer.EnqueueResponse(TestResponse.Query(Response.NewBuilder().SetCryptoGetInfo(CryptoGetInfoResponse.NewBuilder().SetHeader(ResponseHeader.NewBuilder().SetCost(cost.ToTinybars()).Build()).Build()).Build()));
                cryptoService.buffer.EnqueueResponse(TestResponse.Query(Response.NewBuilder().SetCryptoGetInfo(CryptoGetInfoResponse.NewBuilder().SetAccountInfo(CryptoGetInfoResponse.AccountInfo.NewBuilder().SetKey(privateKey.GetPublicKey().ToProtobufKey()).Build()).Build()).Build()));
            }

            Assertions.AssertTrue(AccountInfoFlow.VerifyTransactionSignature(server.client, accountId, properlySignedTx));
            Assertions.Assert.False(AccountInfoFlow.VerifyTransactionSignature(server.client, accountId, improperlySignedTx));
            Assertions.AssertTrue(AccountInfoFlow.VerifySignature(server.client, accountId, BIG_BYTES, properBigBytesSignature));
            Assertions.Assert.False(AccountInfoFlow.VerifySignature(server.client, accountId, BIG_BYTES, improperBigBytesSignature));
            Assertions.AssertTrue(AccountInfoFlow.VerifyTransactionSignatureAsync(server.client, accountId, properlySignedTx).Get());
            Assertions.Assert.False(AccountInfoFlow.VerifyTransactionSignatureAsync(server.client, accountId, improperlySignedTx).Get());
            Assertions.AssertTrue(AccountInfoFlow.VerifySignatureAsync(server.client, accountId, BIG_BYTES, properBigBytesSignature).Get());
            Assertions.Assert.False(AccountInfoFlow.VerifySignatureAsync(server.client, accountId, BIG_BYTES, improperBigBytesSignature).Get());
            Assertions.AssertEquals(16, cryptoService.buffer.queryRequestsReceived.Count);
            for (int i = 0; i < 16; i += 2)
            {
                var costQueryRequest = cryptoService.buffer.queryRequestsReceived[i];
                var queryRequest = cryptoService.buffer.queryRequestsReceived[i + 1];
                Assertions.AssertTrue(costQueryRequest.HasCryptoGetInfo());
                Assertions.AssertTrue(costQueryRequest.GetCryptoGetInfo().HasHeader());
                Assertions.AssertTrue(costQueryRequest.GetCryptoGetInfo().GetHeader().HasPayment());
                Assertions.AssertTrue(queryRequest.HasCryptoGetInfo());
                Assertions.AssertTrue(queryRequest.GetCryptoGetInfo().HasAccountID());
                Assertions.AssertEquals(accountId, AccountId.FromProtobuf(queryRequest.GetCryptoGetInfo().GetAccountID()));
            }

            server.Dispose();
        }

        virtual void ExitOnAborted()
        {
            IList<object> responses1 = List.Of();
            var responses = List.Of(responses1);
            using (var mocker = Mocker.WithResponses(responses))
            {
                Assertions.await Assert.ThrowsAsync<Exception>(() => new AccountBalanceQuery().SetAccountId(new AccountId(0, 0, 10)).Execute(mocker.client));
            }
        }

        virtual void ShouldRetryExceptionallyFunctionsCorrectly(Status.Code code, string description, string sync)
        {
            var service = new TestCryptoService();
            var server = new TestServer("executableRetry" + code + (description != null ? description.Replace(" ", "") : "NULL") + sync, service);
            var exception = Status.FromCode(code).WithDescription(description).AsRuntimeException();
            service.buffer.EnqueueResponse(TestResponse.Error(exception)).EnqueueResponse(TestResponse.TransactionOk());
            if (sync.Equals("sync"))
            {
                new AccountCreateTransaction().SetNodeAccountIds(List.Of(AccountId.FromString("1.1.1"), AccountId.FromString("1.1.2"))).Execute(server.client);
            }
            else
            {
                new AccountCreateTransaction().SetNodeAccountIds(List.Of(AccountId.FromString("1.1.1"), AccountId.FromString("1.1.2"))).ExecuteAsync(server.client).Get();
            }

            Assertions.AssertEquals(2, service.buffer.transactionRequestsReceived.Count);
            AssertFirstTwoRequestsNotDirectedAtSameNode(service);
            server.Dispose();
        }

        virtual void HitsTxMaxAttemptsCorrectly(int numberOfErrors, int maxAttempts, string sync)
        {
            var service = new TestCryptoService();
            var server = new TestServer("executableMaxAttemptsSync" + numberOfErrors + maxAttempts + sync, service);
            var exception = Status.UNAVAILABLE.AsRuntimeException();
            for (var i = 0; i < numberOfErrors; i++)
            {
                service.buffer.EnqueueResponse(TestResponse.Error(exception));
            }

            service.buffer.EnqueueResponse(TestResponse.TransactionOk());
            if (sync.Equals("sync"))
            {
                Assertions.AssertThrows(typeof(MaxAttemptsExceededException), () =>
                {
                    new AccountCreateTransaction().SetMaxAttempts(maxAttempts).SetNodeAccountIds(List.Of(AccountId.FromString("1.1.1"), AccountId.FromString("1.1.2"))).Execute(server.client);
                });
            }
            else
            {
                new AccountCreateTransaction().SetMaxAttempts(maxAttempts).SetNodeAccountIds(List.Of(AccountId.FromString("1.1.1"), AccountId.FromString("1.1.2"))).ExecuteAsync(server.client).Handle((response, error) =>
                {
                    Assertions.AssertNotNull(error);
                    System.@out.Println(error);
                    Assertions.AssertTrue(error.GetCause() is MaxAttemptsExceededException);
                    return null;
                }).Get();
            }

            Assertions.AssertEquals(2, service.buffer.transactionRequestsReceived.Count);
            server.Dispose();
        }

        virtual void ShouldRetryFunctionsCorrectly(com.hedera.hashgraph.sdk.Status status, int numberOfErrors, string sync)
        {
            var service = new TestCryptoService();
            var server = new TestServer("shouldRetryFunctionsCorrectly" + status + numberOfErrors + sync, service);
            for (var i = 0; i < numberOfErrors; i++)
            {
                service.buffer.EnqueueResponse(TestResponse.Transaction(status));
            }

            service.buffer.EnqueueResponse(TestResponse.TransactionOk());
            server.client.SetMaxAttempts(4);
            if (sync.Equals("sync"))
            {
                new AccountCreateTransaction().SetNodeAccountIds(List.Of(AccountId.FromString("1.1.1"), AccountId.FromString("1.1.2"))).Execute(server.client);
            }
            else
            {
                new AccountCreateTransaction().SetNodeAccountIds(List.Of(AccountId.FromString("1.1.1"), AccountId.FromString("1.1.2"))).ExecuteAsync(server.client).Get();
            }

            Assertions.AssertEquals(numberOfErrors + 1, service.buffer.transactionRequestsReceived.Count);

            // For BUSY and TRANSACTION_EXPIRED we retry on the same node; otherwise we expect a different node
            if (status != com.hedera.hashgraph.sdk.Status.BUSY && status != com.hedera.hashgraph.sdk.Status.TRANSACTION_EXPIRED)
            {
                AssertFirstTwoRequestsNotDirectedAtSameNode(service);
            }

            server.Dispose();
        }

        virtual void HitsClientMaxAttemptsCorrectly(com.hedera.hashgraph.sdk.Status status, string sync)
        {
            var service = new TestCryptoService();
            var server = new TestServer("shouldRetryFunctionsCorrectly" + status + sync, service);
            for (var i = 0; i < 2; i++)
            {
                service.buffer.EnqueueResponse(TestResponse.Transaction(status));
            }

            server.client.SetMaxAttempts(2);
            if (sync.Equals("sync"))
            {
                Assertions.AssertThrows(typeof(MaxAttemptsExceededException), () =>
                {
                    new AccountCreateTransaction().SetNodeAccountIds(List.Of(AccountId.FromString("1.1.1"), AccountId.FromString("1.1.2"))).Execute(server.client);
                });
            }
            else
            {
                new AccountCreateTransaction().SetNodeAccountIds(List.Of(AccountId.FromString("1.1.1"), AccountId.FromString("1.1.2"))).ExecuteAsync(server.client).Handle((response, error) =>
                {
                    Assertions.AssertNotNull(error);
                    Assertions.AssertTrue(error.GetCause() is MaxAttemptsExceededException);
                    return null;
                }).Get();
            }

            Assertions.AssertEquals(2, service.buffer.transactionRequestsReceived.Count);

            // BUSY retries stay on the same node; others advance to a different node
            if (status != com.hedera.hashgraph.sdk.Status.BUSY)
            {
                AssertFirstTwoRequestsNotDirectedAtSameNode(service);
            }

            server.Dispose();
        }

        private static void AssertFirstTwoRequestsNotDirectedAtSameNode(TestCryptoService service)
        {
            var requests = service.buffer.transactionRequestsReceived;
            var signedTx0 = SignedTransaction.ParseFrom(requests[0].GetSignedTransactionBytes());
            var signedTx1 = SignedTransaction.ParseFrom(requests[1].GetSignedTransactionBytes());
            var txBody0 = TransactionBody.ParseFrom(signedTx0.GetBodyBytes());
            var txBody1 = TransactionBody.ParseFrom(signedTx1.GetBodyBytes());
            Assertions.AssertNotEquals(txBody0.GetNodeAccountID(), txBody1.GetNodeAccountID());
        }

        virtual void DefaultMaxTransactionFeeTest()
        {
            var service = new TestCryptoService();
            var server = new TestServer("maxTransactionFee", service);
            service.buffer.EnqueueResponse(TestResponse.TransactionOk()).EnqueueResponse(TestResponse.TransactionOk()).EnqueueResponse(TestResponse.TransactionOk()).EnqueueResponse(TestResponse.TransactionOk());
            new AccountDeleteTransaction().Execute(server.client);
            new AccountDeleteTransaction().SetMaxTransactionFee(new Hbar(5)).Execute(server.client);
            server.client.SetDefaultMaxTransactionFee(new Hbar(1));
            new AccountDeleteTransaction().Execute(server.client);
            new AccountDeleteTransaction().SetMaxTransactionFee(new Hbar(3)).Execute(server.client);
            Assertions.AssertEquals(4, service.buffer.transactionRequestsReceived.Count);
            var transactions = new List<com.hedera.hashgraph.sdk.Transaction<?>>();
            foreach (var request in service.buffer.transactionRequestsReceived)
            {
                transactions.Add(com.hedera.hashgraph.sdk.Transaction.FromBytes(request.ToByteArray()));
            }

            Assertions.AssertEquals(new Hbar(2), transactions[0].GetMaxTransactionFee());
            Assertions.AssertEquals(new Hbar(5), transactions[1].GetMaxTransactionFee());
            Assertions.AssertEquals(new Hbar(1), transactions[2].GetMaxTransactionFee());
            Assertions.AssertEquals(new Hbar(3), transactions[3].GetMaxTransactionFee());
            server.Dispose();
        }

        virtual void DefaultMaxQueryPaymentTest()
        {
            var service = new TestCryptoService();
            var server = new TestServer("queryPayment", service);
            var response = Response.NewBuilder().SetCryptogetAccountBalance(new AccountBalance(new Hbar(0), new HashMap<TokenId, long>(), new HashMap<TokenId, int>()).ToProtobuf()).Build();
            service.buffer.EnqueueResponse(TestResponse.Query(response)).EnqueueResponse(TestResponse.Query(response)).EnqueueResponse(TestResponse.Query(response));

            // TODO: this will take some work, since I have to contend with Query's getCost behavior
            // TODO: actually, because AccountBalanceQuery is free, I'll need some other query type to test this.
            //       Perhaps getAccountInfo?
            server.Dispose();
        }

        virtual void SignerDoesNotSignTwice()
        {
            var service = new TestCryptoService();
            var server = new TestServer("signerDoesNotSignTwice", service);
            service.buffer.EnqueueResponse(TestResponse.TransactionOk());
            var aliceKey = PrivateKey.GenerateED25519();
            var transaction = new AccountCreateTransaction().SetTransactionId(TransactionId.Generate(Objects.RequireNonNull(server.client.GetOperatorAccountId()))).SetNodeAccountIds(server.client.network.GetNodeAccountIdsForExecute()).Freeze().Sign(aliceKey);

            // This will cause the SDK Transaction to populate the sigPairLists list
            transaction.GetTransactionHashPerNode();

            // This will clear the outerTransactions list while keeping the sigPairLists list
            transaction.SignWithOperator(server.client);

            // If Transaction.signTransaction() is not programmed correctly, it will add Alice's signature to the
            // sigPairList a second time here.
            transaction.Execute(server.client);

            // Now we must go through the laborious process of digging info out of the response.  =(
            Assertions.AssertEquals(1, service.buffer.transactionRequestsReceived.Count);
            var request = service.buffer.transactionRequestsReceived[0];
            var sigPairList = SignedTransaction.ParseFrom(request.GetSignedTransactionBytes()).GetSigMap().GetSigPairList();
            Assertions.AssertEquals(2, sigPairList.Count);
            Assertions.AssertNotEquals(sigPairList[0].GetEd25519().ToString(), sigPairList[1].GetEd25519().ToString());
            server.Dispose();
        }

        virtual void CanCancelExecuteAsync()
        {
            var service = new TestCryptoService();
            var server = new TestServer("canCancelExecuteAsync", service);
            server.client.SetMaxBackoff(Duration.OfSeconds(8));
            server.client.SetMinBackoff(Duration.OfSeconds(1));
            var noReceiptResponse = TestResponse.Query(Response.NewBuilder().SetTransactionGetReceipt(TransactionGetReceiptResponse.NewBuilder().SetHeader(ResponseHeader.NewBuilder().SetNodeTransactionPrecheckCode(com.hedera.hashgraph.sdk.Status.RECEIPT_NOT_FOUND.code))).Build());
            service.buffer.EnqueueResponse(noReceiptResponse);
            service.buffer.EnqueueResponse(noReceiptResponse);
            service.buffer.EnqueueResponse(noReceiptResponse);
            var future = new TransactionReceiptQuery().ExecuteAsync(server.client);
            Thread.Sleep(1500);
            future.Cancel(true);
            Thread.Sleep(5000);
            Assertions.AssertEquals(2, service.buffer.queryRequestsReceived.Count);
            server.Dispose();
        }

        private class TestCryptoService : CryptoServiceImplBase, TestService
        {
            public Buffer buffer = new Buffer();
            public override Buffer GetBuffer()
            {
                return buffer;
            }

            public override void CreateAccount(Transaction request, StreamObserver<TransactionResponse> responseObserver)
            {
                RespondToTransactionFromQueue(request, responseObserver);
            }

            public override void CryptoDelete(Transaction request, StreamObserver<TransactionResponse> responseObserver)
            {
                RespondToTransactionFromQueue(request, responseObserver);
            }

            public override void CryptoGetBalance(Query request, StreamObserver<Response> responseObserver)
            {
                RespondToQueryFromQueue(request, responseObserver);
            }

            public override void GetTransactionReceipts(Query request, StreamObserver<Response> responseObserver)
            {
                RespondToQueryFromQueue(request, responseObserver);
            }

            public override void GetTxRecordByTxID(Query request, StreamObserver<Response> responseObserver)
            {
                RespondToQueryFromQueue(request, responseObserver);
            }

            public override void GetAccountInfo(Query request, StreamObserver<Response> responseObserver)
            {
                RespondToQueryFromQueue(request, responseObserver);
            }
        }

        private class TestFileService : FileServiceImplBase, TestService
        {
            public Buffer buffer = new Buffer();
            public override Buffer GetBuffer()
            {
                return buffer;
            }

            public override void CreateFile(Transaction request, StreamObserver<TransactionResponse> responseObserver)
            {
                RespondToTransactionFromQueue(request, responseObserver);
            }

            public override void AppendContent(Transaction request, StreamObserver<TransactionResponse> responseObserver)
            {
                RespondToTransactionFromQueue(request, responseObserver);
            }

            public override void DeleteFile(Transaction request, StreamObserver<TransactionResponse> responseObserver)
            {
                RespondToTransactionFromQueue(request, responseObserver);
            }
        }

        private class TestContractService : SmartContractServiceImplBase, TestService
        {
            public Buffer buffer = new Buffer();
            public override Buffer GetBuffer()
            {
                return buffer;
            }

            public override void CreateContract(Transaction request, StreamObserver<TransactionResponse> responseObserver)
            {
                RespondToTransactionFromQueue(request, responseObserver);
            }
        }

        virtual void TestMetadataInterceptor()
        {
            var metadataCaptor = new MetadataCapturingInterceptor();
            IList<object> responses1 = List.Of((Function<object, object>)(request) =>
            {
                var metadata = metadataCaptor.GetLastMetadata();
                Assertions.AssertNotNull(metadata, "No metadata was captured");
                var userAgent = metadata[Metadata.Key.Of("x-user-agent", Metadata.ASCII_STRING_MARSHALLER)];
                Assertions.AssertNotNull(userAgent, "User agent header was not found");
                Assertions.AssertTrue(userAgent.StartsWith("hiero-sdk-java/"), "User agent header does not match expected format: " + userAgent);
                return Response.NewBuilder().SetCryptogetAccountBalance(CryptoGetAccountBalanceResponse.NewBuilder().SetHeader(ResponseHeader.NewBuilder().SetNodeTransactionPrecheckCode(ResponseCodeEnum.OK).Build()).SetAccountID(AccountID.NewBuilder().SetAccountNum(10).Build()).SetBalance(100).Build()).Build();
            });
            var responses = List.Of(responses1);
            using (var mocker = new AnonymousMocker(responses))
            {

                // Execute query to trigger metadata interceptor
                new AccountBalanceQuery().SetAccountId(new AccountId(0, 0, 10)).Execute(mocker.client);
            }
        }

        private sealed class AnonymousMocker : Mocker
        {
            public AnonymousMocker(TestContractService parent)
            {
                this.parent = parent;
            }

            private readonly TestContractService parent;
            protected void ConfigureServerBuilder(InProcessServerBuilder builder)
            {
                builder.Intercept(metadataCaptor);
            }
        }

        private class MetadataCapturingInterceptor : ServerInterceptor
        {
            private Metadata lastMetadata;
            public virtual ServerCall.Listener<ReqT> InterceptCall<ReqT, RespT>(ServerCall<ReqT, RespT> call, Metadata metadata, ServerCallHandler<ReqT, RespT> next)
            {
                this.lastMetadata = metadata;
                return next.StartCall(call, metadata);
            }

            public virtual Metadata GetLastMetadata()
            {
                return lastMetadata;
            }
        }
    }
}