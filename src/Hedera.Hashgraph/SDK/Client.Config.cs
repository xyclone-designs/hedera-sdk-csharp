// SPDX-License-Identifier: Apache-2.0
using Hedera.Hashgraph.SDK.Account;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.Networking;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Hedera.Hashgraph.SDK
{
    public sealed partial class Client 
    {
        private class Config
        {
            private JsonObject? network;
            private JsonObject? networkName;
            private ConfigOperator? oper8r;
            private JsonObject? mirrorNetwork;
            private JsonObject? shard;
            private JsonObject? realm;

            private class ConfigOperator
            {
                public string? AccountId;
                public string? PrivateKey;
            }

            internal static Config FromString(string json)
            {
                return FromJson(JsonNode.Parse(json) ?? throw new JsonException("error paring json"));
            }

			internal static Config FromJson(JsonNode json)
            {
				return new Config
                {
                    network = json[nameof(network)]?.AsObject(),
                    networkName = json[nameof(networkName)]?.AsObject(),
                    mirrorNetwork = json[nameof(mirrorNetwork)]?.AsObject(),
                    shard = json[nameof(shard)]?.AsObject(),
                    realm = json[nameof(realm)]?.AsObject(),
				};
            }

			internal Client ToClient()
            {
                Client client = InitializeWithNetwork();
                SetOperatorOn(client);
                SetMirrorNetworkOn(client);
                return client;
            }
			internal Client InitializeWithNetwork()
            {
                if (network == null) throw new Exception("Network is not set in provided json object");

				Client client;

                if (network.GetValueKind() is JsonValueKind.Object)
                {
                    client = ClientFromNetworkJson();
                }
                else
                {
                    client = ClientFromNetworkString();
                }

                return client;
            }
			internal Client ClientFromNetworkJson()
            {
                Client client;

                Dictionary<string, AccountId> nodes = network?
                    .OfType<KeyValuePair<string, JsonNode>>()
                    .ToDictionary(_ => _.Value.ToString().Replace("\"", ""), _ => AccountId.FromString(_.Key.Replace("\"", ""))) ?? [];
				
				ExecutorService executor = new ();
				
                Network _network = Network.ForNetwork(executor, nodes);
                MirrorNetwork _mirrorNetwork = MirrorNetwork.ForNetwork(executor, []);

                long shardValue = shard?.GetValue<long>() ?? 0;
                long realmValue = realm?.GetValue<long>() ?? 0;
                
                client = new Client(executor, _network, _mirrorNetwork, null, true, null, shardValue, realmValue);
                
                SetNetworkNameOn(client);

                return client;
            }
            internal Client ClientFromNetworkString()
            {
                return network?.GetValue<string>() switch
                {
                    MAINNET => ForMainnet(),
                    TESTNET => ForTestnet(),
                    PREVIEWNET => ForPreviewnet(),

                    _ => throw new JsonException("Illegal argument for network.")};
            }
			internal void SetNetworkNameOn(Client client)
			{
				if (networkName?.GetValue<string>() is string networkNameString)
				{
					try
					{
						client.Network_.LedgerId = LedgerId.FromString(networkNameString);
					}
					catch (Exception)
					{
						throw new ArgumentException("networkName in config was \"" + networkNameString + "\", expected either \"mainnet\", \"testnet\" or \"previewnet\"");
					}
				}
			}
			internal void SetMirrorNetworkOn(Client client)
            {
                if (mirrorNetwork != null)
                {
                    SetMirrorNetwork(client);
                }
            }
            internal void SetMirrorNetwork(Client client)
            {
                if (mirrorNetwork?.GetValueKind() is JsonValueKind.Array)
                {
                    SetMirrorNetworksFromJsonArray(client);
                }
                else
                {
                    SetMirrorNetworkFromString(client);
                }
            }
            internal void SetMirrorNetworkFromString(Client client)
            {
                client.MirrorNetwork_ = mirrorNetwork?.GetValue<string>() switch
                {
                    MAINNET => MirrorNetwork.ForMainnet(client.Executor),
                    TESTNET => MirrorNetwork.ForTestnet(client.Executor),
                    PREVIEWNET => MirrorNetwork.ForPreviewnet(client.Executor),

                    _ => throw new JsonException("Illegal argument for mirrorNetwork."),
                };
            }
            internal void SetMirrorNetworksFromJsonArray(Client client)
            {
                if (mirrorNetwork is not null)
                    client.MirrorNetwork_.Network = [.. GetListMirrors(mirrorNetwork.AsArray())];
            }
            internal IEnumerable<string> GetListMirrors(JsonArray mirrors)
            {
                return mirrors
                    .OfType<JsonNode>()
                    .Select(_ => _.ToString().Replace("\"", ""));
            }
            internal void SetOperatorOn(Client client)
            {
                if (oper8r != null)
                {
                    AccountId operatorAccount = AccountId.FromString(oper8r.AccountId);
                    PrivateKey privateKey = PrivateKey.FromString(oper8r.PrivateKey);

                    client.OperatorSet(operatorAccount, privateKey);
                }
            }
        }
    }
}