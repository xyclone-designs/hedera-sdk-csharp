// SPDX-License-Identifier: Apache-2.0

namespace Hedera.Hashgraph.SDK
{
    /// <summary>
    /// Enum for the freeze types.
    /// </summary>
    public enum FreezeType
    {
		/// <summary>
		/// An invalid freeze type.
		/// <p>
		/// The first value in a protobuf enum is a default value. This default
		/// is RECOMMENDED to be an invalid value to aid in detecting unset fields.
		/// </summary>
		UnknownFreezeType = Proto.FreezeType.UnknownFreezeType,
		/// <summary>
		/// Freeze the network, and take no further action.
		/// <p>
		/// The `starttime` field is REQUIRED, MUST be strictly later than the
		/// consensus time when this transaction is handled, and SHOULD be between
		/// `300` and `3600` seconds after the transaction identifier
		/// `transactionValidStart` field.<br/>
		/// The fields `updatefile` and `filehash` SHALL be ignored.<br/>
		/// A `FreezeOnly` transaction SHALL NOT perform any network
		/// changes or upgrades.<br/>
		/// After this freeze is processed manual intervention is REQUIRED
		/// to restart the network.
		/// </summary>
		FreezeOnly = Proto.FreezeType.FreezeOnly,
		/// <summary>
		/// This freeze type does not freeze the network, but begins
		/// "preparation" to upgrade the network.
		/// <p>
		/// The fields `updatefile` and `filehash` are REQUIRED
		/// and MUST be valid.<br/>
		/// The `starttime` field SHALL be ignored.<br/>
		/// A `PrepareUpgrade` transaction SHALL NOT freeze the network or
		/// interfere with general transaction processing.<br/>
		/// If this freeze type is initiated after a `TelemetryUpgrade`, the
		/// prepared telemetry upgrade SHALL be reset and all telemetry upgrade
		/// artifacts in the filesystem SHALL be deleted.<br/>
		/// At some point after this freeze type completes (dependent on the size
		/// of the upgrade file), the network SHALL be prepared to complete
		/// a software upgrade of all nodes.
		/// </summary>
		PrepareUpgrade = Proto.FreezeType.PrepareUpgrade,
		/// <summary>
		/// Freeze the network to perform a software upgrade.
		/// <p>
		/// The `starttime` field is REQUIRED, MUST be strictly later than the
		/// consensus time when this transaction is handled, and SHOULD be between
		/// `300` and `3600` seconds after the transaction identifier
		/// `transactionValidStart` field.<br/>
		/// A software upgrade file MUST be prepared prior to this transaction.<br/>
		/// After this transaction completes, the network SHALL initiate an
		/// upgrade and restart of all nodes at the start time specified.
		/// </summary>
		FreezeUpgrade = Proto.FreezeType.FreezeUpgrade,
		/// <summary>
		/// Abort a pending network freeze operation.
		/// <p>
		/// All fields SHALL be ignored for this freeze type.<br/>
		/// This freeze type MAY be submitted after a `FreezeOnly`,
		/// `FreezeUpgrade`, or `TelemetryUpgrade` is initiated.<br/>
		/// This freeze type MUST be submitted and reach consensus
		/// before the `starttime` designated for the current pending
		/// freeze to be effective.<br/>
		/// After this freeze type is processed, the upgrade file hash
		/// and pending freeze start time stored in the network SHALL
		/// be reset to default (empty) values.
		/// </summary>
		FreezeAbort = Proto.FreezeType.FreezeAbort,
        /// <summary>
        /// Prepare an upgrade of auxiliary services and containers
        /// providing telemetry/metrics.
        /// <p>
        /// The `starttime` field is REQUIRED, MUST be strictly later than the
        /// consensus time when this transaction is handled, and SHOULD be between
        /// `300` and `3600` seconds after the transaction identifier
        /// `transactionValidStart` field.<br/>
        /// The `updatefile` field is REQUIRED and MUST be valid.<br/>
        /// A `TelemetryUpgrade` transaction SHALL NOT freeze the network or
        /// interfere with general transaction processing.<br/>
        /// This freeze type MUST NOT be initiated between a `PrepareUpgrade`
        /// and `FreezeUpgrade`. If this freeze type is initiated after a
        /// `PrepareUpgrade`, the prepared upgrade SHALL be reset and all software
        /// upgrade artifacts in the filesystem SHALL be deleted.<br/>
        /// At some point after this freeze type completes (dependent on the
        /// size of the upgrade file), the network SHALL automatically upgrade
        /// the telemetry/metrics services and containers as directed in
        /// the specified telemetry upgrade file.
        /// <blockquote> The condition that `starttime` is REQUIRED is an
        /// historical anomaly and SHOULD change in a future release.</blockquote>
        /// </summary>
        TelemetryUpgrade = Proto.FreezeType.TelemetryUpgrade,
    }
}