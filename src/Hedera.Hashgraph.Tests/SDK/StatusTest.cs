// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;

using Hedera.Hashgraph.SDK;

namespace Hedera.Hashgraph.Tests.SDK
{
    class StatusTest
    {
        public virtual void StatusToResponseCode()
        {
            foreach (Proto.ResponseCodeEnum code in Enum.GetValues<Proto.ResponseCodeEnum>())
            {
                // not an actual value we want to handle
                // this is what we're given if an unexpected value was decoded
                if (code == Proto.ResponseCodeEnum.Unknown)
                    continue;

                ResponseStatus status = (ResponseStatus)code;
                Assert.Equal((int)code, (int)status);
            }
        }

        public virtual void NewHookAndLambdaStatusesRoundTrip()
        {
            KeyValuePair<ResponseStatus, Proto.ResponseCodeEnum>[] pairs = new []
            {
                KeyValuePair.Create(ResponseStatus.InvalidSerializedTxMessageHashAlgorithm, Proto.ResponseCodeEnum.InvalidSerializedTxMessageHashAlgorithm),
                KeyValuePair.Create(ResponseStatus.EvmHookGasThrottled, Proto.ResponseCodeEnum.EvmHookGasThrottled),
                KeyValuePair.Create(ResponseStatus.HookIdInUse, Proto.ResponseCodeEnum.HookIdInUse),
                KeyValuePair.Create(ResponseStatus.BadHookRequest, Proto.ResponseCodeEnum.BadHookRequest),
                KeyValuePair.Create(ResponseStatus.RejectedByAccountAllowanceHook, Proto.ResponseCodeEnum.RejectedByAccountAllowanceHook),
                KeyValuePair.Create(ResponseStatus.HookNotFound, Proto.ResponseCodeEnum.HookNotFound),
                KeyValuePair.Create(ResponseStatus.LambdaStorageUpdateBytesTooLong, Proto.ResponseCodeEnum.LambdaStorageUpdateBytesTooLong),
                KeyValuePair.Create(ResponseStatus.LambdaStorageUpdateBytesMustUseMinimalRepresentation, Proto.ResponseCodeEnum.LambdaStorageUpdateBytesMustUseMinimalRepresentation),
                KeyValuePair.Create(ResponseStatus.InvalidHookId, Proto.ResponseCodeEnum.InvalidHookId),
                KeyValuePair.Create(ResponseStatus.EmptyLambdaStorageUpdate, Proto.ResponseCodeEnum.EmptyLambdaStorageUpdate),
                KeyValuePair.Create(ResponseStatus.HookIdRepeatedInCreationDetails, Proto.ResponseCodeEnum.HookIdRepeatedInCreationDetails),
                KeyValuePair.Create(ResponseStatus.HooksNotEnabled, Proto.ResponseCodeEnum.HooksNotEnabled),
                KeyValuePair.Create(ResponseStatus.HookIsNotALambda, Proto.ResponseCodeEnum.HookIsNotALambda),
                KeyValuePair.Create(ResponseStatus.HookDeleted, Proto.ResponseCodeEnum.HookDeleted),
                KeyValuePair.Create(ResponseStatus.TooManyLambdaStorageUpdates, Proto.ResponseCodeEnum.TooManyLambdaStorageUpdates),
                KeyValuePair.Create(ResponseStatus.HookCreationBytesMustUseMinimalRepresentation, Proto.ResponseCodeEnum.HookCreationBytesMustUseMinimalRepresentation),
                KeyValuePair.Create(ResponseStatus.HookCreationBytesTooLong, Proto.ResponseCodeEnum.HookCreationBytesTooLong),
                KeyValuePair.Create(ResponseStatus.InvalidHookCreationSpec, Proto.ResponseCodeEnum.InvalidHookCreationSpec),
                KeyValuePair.Create(ResponseStatus.HookExtensionEmpty, Proto.ResponseCodeEnum.HookExtensionEmpty),
                KeyValuePair.Create(ResponseStatus.InvalidHookAdminKey, Proto.ResponseCodeEnum.InvalidHookAdminKey),
                KeyValuePair.Create(ResponseStatus.HookDeletionRequiresZeroStorageSlots, Proto.ResponseCodeEnum.HookDeletionRequiresZeroStorageSlots),
                KeyValuePair.Create(ResponseStatus.CannotSetHooksAndApproval, Proto.ResponseCodeEnum.CannotSetHooksAndApproval),
                KeyValuePair.Create(ResponseStatus.TransactionRequiresZeroHooks, Proto.ResponseCodeEnum.TransactionRequiresZeroHooks),
                KeyValuePair.Create(ResponseStatus.InvalidHookCall, Proto.ResponseCodeEnum.InvalidHookCall),
                KeyValuePair.Create(ResponseStatus.HooksAreNotSupportedInAirdrops, Proto.ResponseCodeEnum.HooksAreNotSupportedInAirdrops)
            };

            foreach (KeyValuePair<ResponseStatus, Proto.ResponseCodeEnum> pair in pairs)
            {
                // enum holds the exact ResponseCodeEnum
                Assert.Equal(pair.Key, (ResponseStatus)pair.Value);

                // valueOf maps ResponseCodeEnum back to Status
                Assert.Equal((Proto.ResponseCodeEnum)pair.Key, pair.Value);

                // toResponseCode preserves numeric value
                Assert.Equal((int)pair.Key, (int)pair.Value);

                // toString mirrors code.name()
                Assert.Equal(pair.Key.ToString(), pair.Value.ToString());
            }
        }
    }
}