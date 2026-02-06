// SPDX-License-Identifier: Apache-2.0
using System;

namespace Hedera.Hashgraph.SDK.Networking
{
	/// <summary>
	/// Enum for the network names.
	/// </summary>
	public enum NetworkName
    {
        /// <summary>
        /// The mainnet network
        /// </summary>
        [Obsolete]
        MainNet = 0,
        /// <summary>
        /// The testnet network
        /// </summary>
        [Obsolete]
        TestNet = 1,
        /// <summary>
        /// The previewnet network
        /// </summary>
        [Obsolete]
        PreviewNet = 2,
        /// <summary>
        /// Other network
        /// </summary>
        [Obsolete]
        Other = int.MaxValue
    }
}