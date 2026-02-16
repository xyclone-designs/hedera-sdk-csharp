// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Hedera.Hashgraph.Tests.SDK
{
    public interface Function<T, R>
    {
        R Apply(T t);
    }
}