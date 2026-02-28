// SPDX-License-Identifier: Apache-2.0
using Org.Assertj.Core.Api.Assertions;
using Proto;
using Org.Junit.Jupiter.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Grpc.Core;

namespace Hedera.Hashgraph.Tests.SDK
{
    class StatusTest
    {
        public virtual void StatusToResponseCode()
        {
            foreach (ResponseCodeEnum code in Proto.ResponseCodeEnum.Values())
            {

                // not an actual value we want to handle
                // this is what we're given if an unexpected value was decoded
                if (code == Proto.ResponseCodeEnum.UNRECOGNIZED)
                {
                    continue;
                }

                Status status = Status.ValueOf(code);
                Assert.Equal(code.GetNumber(), status.code.GetNumber());
            }
        }

        public virtual void StatusUnrecognized()
        {
            Assert.Throws<ArgumentException>(() => Status.ValueOf(Proto.ResponseCodeEnum.UNRECOGNIZED)).WithMessage("network returned unrecognized response code; your SDK may be out of date");
        }

        public virtual void NewHookAndLambdaStatusesRoundTrip()
        {
            object[, ] pairs = new object[]
            {
                new[]
                {
                    Status.INVALID_SERIALIZED_TX_MESSAGE_HASH_ALGORITHM,
                    Proto.ResponseCodeEnum.INVALID_SERIALIZED_TX_MESSAGE_HASH_ALGORITHM
                },
                new[]
                {
                    Status.EVM_HOOK_GAS_THROTTLED,
                    Proto.ResponseCodeEnum.EVM_HOOK_GAS_THROTTLED
                },
                new[]
                {
                    Status.HOOK_ID_IN_USE,
                    Proto.ResponseCodeEnum.HOOK_ID_IN_USE
                },
                new[]
                {
                    Status.BAD_HOOK_REQUEST,
                    Proto.ResponseCodeEnum.BAD_HOOK_REQUEST
                },
                new[]
                {
                    Status.REJECTED_BY_AccountAllowanceHook,
                    Proto.ResponseCodeEnum.REJECTED_BY_AccountAllowanceHook
                },
                new[]
                {
                    Status.HOOK_NOT_FOUND,
                    Proto.ResponseCodeEnum.HOOK_NOT_FOUND
                },
                new[]
                {
                    Status.LAMBDA_STORAGE_UPDATE_BYTES_TOO_LONG,
                    Proto.ResponseCodeEnum.LAMBDA_STORAGE_UPDATE_BYTES_TOO_LONG
                },
                new[]
                {
                    Status.LAMBDA_STORAGE_UPDATE_BYTES_MUST_USE_MINIMAL_REPRESENTATION,
                    Proto.ResponseCodeEnum.LAMBDA_STORAGE_UPDATE_BYTES_MUST_USE_MINIMAL_REPRESENTATION
                },
                new[]
                {
                    Status.INVALID_HOOK_ID,
                    Proto.ResponseCodeEnum.INVALID_HOOK_ID
                },
                new[]
                {
                    Status.EMPTY_LAMBDA_STORAGE_UPDATE,
                    Proto.ResponseCodeEnum.EMPTY_LAMBDA_STORAGE_UPDATE
                },
                new[]
                {
                    Status.HOOK_ID_REPEATED_IN_CREATION_DETAILS,
                    Proto.ResponseCodeEnum.HOOK_ID_REPEATED_IN_CREATION_DETAILS
                },
                new[]
                {
                    Status.HOOKS_NOT_ENABLED,
                    Proto.ResponseCodeEnum.HOOKS_NOT_ENABLED
                },
                new[]
                {
                    Status.HOOK_IS_NOT_A_LAMBDA,
                    Proto.ResponseCodeEnum.HOOK_IS_NOT_A_LAMBDA
                },
                new[]
                {
                    Status.HOOK_DELETED,
                    Proto.ResponseCodeEnum.HOOK_DELETED
                },
                new[]
                {
                    Status.TOO_MANY_LAMBDA_STORAGE_UPDATES,
                    Proto.ResponseCodeEnum.TOO_MANY_LAMBDA_STORAGE_UPDATES
                },
                new[]
                {
                    Status.HOOK_CREATION_BYTES_MUST_USE_MINIMAL_REPRESENTATION,
                    Proto.ResponseCodeEnum.HOOK_CREATION_BYTES_MUST_USE_MINIMAL_REPRESENTATION
                },
                new[]
                {
                    Status.HOOK_CREATION_BYTES_TOO_LONG,
                    Proto.ResponseCodeEnum.HOOK_CREATION_BYTES_TOO_LONG
                },
                new[]
                {
                    Status.INVALID_HOOK_CREATION_SPEC,
                    Proto.ResponseCodeEnum.INVALID_HOOK_CREATION_SPEC
                },
                new[]
                {
                    Status.HOOK_EXTENSION_EMPTY,
                    Proto.ResponseCodeEnum.HOOK_EXTENSION_EMPTY
                },
                new[]
                {
                    Status.INVALID_HOOK_ADMIN_KEY,
                    Proto.ResponseCodeEnum.INVALID_HOOK_ADMIN_KEY
                },
                new[]
                {
                    Status.HOOK_DELETION_REQUIRES_ZERO_STORAGE_SLOTS,
                    Proto.ResponseCodeEnum.HOOK_DELETION_REQUIRES_ZERO_STORAGE_SLOTS
                },
                new[]
                {
                    Status.CANNOT_SET_HOOKS_AND_APPROVAL,
                    Proto.ResponseCodeEnum.CANNOT_SET_HOOKS_AND_APPROVAL
                },
                new[]
                {
                    Status.TRANSACTION_REQUIRES_ZERO_HOOKS,
                    Proto.ResponseCodeEnum.TRANSACTION_REQUIRES_ZERO_HOOKS
                },
                new[]
                {
                    Status.INVALID_HOOK_CALL,
                    Proto.ResponseCodeEnum.INVALID_HOOK_CALL
                },
                new[]
                {
                    Status.HOOKS_ARE_NOT_SUPPORTED_IN_AIRDROPS,
                    Proto.ResponseCodeEnum.HOOKS_ARE_NOT_SUPPORTED_IN_AIRDROPS
                }
            };
            foreach (Object[] pair in pairs)
            {
                Status status = (Status)pair[0];
                ResponseCodeEnum code = (ResponseCodeEnum)pair[1];

                // enum holds the exact ResponseCodeEnum
                Assert.Equal(status.code, code);

                // valueOf maps ResponseCodeEnum back to Status
                Assert.Equal(ResponseStatus.ValueOf(code), status);

                // toResponseCode preserves numeric value
                Assert.Equal(status.ToResponseCode(), code.GetNumber());

                // toString mirrors code.name()
                Assert.Equal(status.ToString(), code.Name());
            }
        }
    }
}