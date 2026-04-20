// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;

using Hedera.Hashgraph.SDK;

namespace Hedera.Hashgraph.Tests.SDK
{
    public class StatusTest
    {
        [Fact]
        public virtual void StatusToResponseCode()
        {
            foreach (Proto.Services.ResponseCodeEnum code in Enum.GetValues<Proto.Services.ResponseCodeEnum>())
            {
                // not an actual value we want to handle
                // this is what we're given if an unexpected value was decoded
                if (code == Proto.Services.ResponseCodeEnum.Unknown)
                    continue;

                ResponseStatus status = (ResponseStatus)code;
                Assert.Equal((int)code, (int)status);
            }
        }

        [Fact]
        public virtual void NewHookAndLambdaStatusesRoundTrip()
        {
            KeyValuePair<ResponseStatus, Proto.Services.ResponseCodeEnum>[] pairs = new []
            {
                KeyValuePair.Create(ResponseStatus.InvalidSerializedTxMessageHashAlgorithm, Proto.Services.ResponseCodeEnum.InvalidSerializedTxMessageHashAlgorithm),
                KeyValuePair.Create(ResponseStatus.EvmHookGasThrottled, Proto.Services.ResponseCodeEnum.EvmHookGasThrottled),
                KeyValuePair.Create(ResponseStatus.HookIdInUse, Proto.Services.ResponseCodeEnum.HookIdInUse),
                KeyValuePair.Create(ResponseStatus.BadHookRequest, Proto.Services.ResponseCodeEnum.BadHookRequest),
                KeyValuePair.Create(ResponseStatus.RejectedByAccountAllowanceHook, Proto.Services.ResponseCodeEnum.RejectedByAccountAllowanceHook),
                KeyValuePair.Create(ResponseStatus.HookNotFound, Proto.Services.ResponseCodeEnum.HookNotFound),
                KeyValuePair.Create(ResponseStatus.LambdaStorageUpdateBytesTooLong, Proto.Services.ResponseCodeEnum.LambdaStorageUpdateBytesTooLong),
                KeyValuePair.Create(ResponseStatus.LambdaStorageUpdateBytesMustUseMinimalRepresentation, Proto.Services.ResponseCodeEnum.LambdaStorageUpdateBytesMustUseMinimalRepresentation),
                KeyValuePair.Create(ResponseStatus.InvalidHookId, Proto.Services.ResponseCodeEnum.InvalidHookId),
                KeyValuePair.Create(ResponseStatus.EmptyLambdaStorageUpdate, Proto.Services.ResponseCodeEnum.EmptyLambdaStorageUpdate),
                KeyValuePair.Create(ResponseStatus.HookIdRepeatedInCreationDetails, Proto.Services.ResponseCodeEnum.HookIdRepeatedInCreationDetails),
                KeyValuePair.Create(ResponseStatus.HooksNotEnabled, Proto.Services.ResponseCodeEnum.HooksNotEnabled),
                KeyValuePair.Create(ResponseStatus.HookIsNotALambda, Proto.Services.ResponseCodeEnum.HookIsNotALambda),
                KeyValuePair.Create(ResponseStatus.HookDeleted, Proto.Services.ResponseCodeEnum.HookDeleted),
                KeyValuePair.Create(ResponseStatus.TooManyLambdaStorageUpdates, Proto.Services.ResponseCodeEnum.TooManyLambdaStorageUpdates),
                KeyValuePair.Create(ResponseStatus.HookCreationBytesMustUseMinimalRepresentation, Proto.Services.ResponseCodeEnum.HookCreationBytesMustUseMinimalRepresentation),
                KeyValuePair.Create(ResponseStatus.HookCreationBytesTooLong, Proto.Services.ResponseCodeEnum.HookCreationBytesTooLong),
                KeyValuePair.Create(ResponseStatus.InvalidHookCreationSpec, Proto.Services.ResponseCodeEnum.InvalidHookCreationSpec),
                KeyValuePair.Create(ResponseStatus.HookExtensionEmpty, Proto.Services.ResponseCodeEnum.HookExtensionEmpty),
                KeyValuePair.Create(ResponseStatus.InvalidHookAdminKey, Proto.Services.ResponseCodeEnum.InvalidHookAdminKey),
                KeyValuePair.Create(ResponseStatus.HookDeletionRequiresZeroStorageSlots, Proto.Services.ResponseCodeEnum.HookDeletionRequiresZeroStorageSlots),
                KeyValuePair.Create(ResponseStatus.CannotSetHooksAndApproval, Proto.Services.ResponseCodeEnum.CannotSetHooksAndApproval),
                KeyValuePair.Create(ResponseStatus.TransactionRequiresZeroHooks, Proto.Services.ResponseCodeEnum.TransactionRequiresZeroHooks),
                KeyValuePair.Create(ResponseStatus.InvalidHookCall, Proto.Services.ResponseCodeEnum.InvalidHookCall),
                KeyValuePair.Create(ResponseStatus.HooksAreNotSupportedInAirdrops, Proto.Services.ResponseCodeEnum.HooksAreNotSupportedInAirdrops)
            };

            foreach (KeyValuePair<ResponseStatus, Proto.Services.ResponseCodeEnum> pair in pairs)
            {
                // enum holds the exact ResponseCodeEnum
                Assert.Equal(pair.Key, (ResponseStatus)pair.Value);

                // valueOf maps ResponseCodeEnum back to Status
                Assert.Equal((Proto.Services.ResponseCodeEnum)pair.Key, pair.Value);

                // toResponseCode preserves numeric value
                Assert.Equal((int)pair.Key, (int)pair.Value);

                // toString mirrors code.name()
                Assert.Equal(pair.Key.ToString(), pair.Value.ToString());
            }
        }
    }
}