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
            private JsonElement network;
            private JsonElement networkName;
            private ConfigOperator oper8r;
            private JsonElement mirrorNetwork;
            private JsonElement shard;
            private JsonElement realm;
            private class ConfigOperator
            {
                private string accountId;
                private string privateKey;
            }

            internal static Config FromString(string json)
            {
                return new Gson().FromJson((Reader)new StringReader(json), typeof(Config));
            }

			internal static Config FromJson(Io json)
            {
                return new Gson().FromJson(json, typeof(Config));
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
                if (network == null)
                {
                    throw new Exception("Network is not set in provided json object");
                }

                Client client;
                if (network.IsJsonObject())
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
                var networkJson = network.GetAsJsonObject();
                Dictionary<string, AccountId> nodes = networkJson.AsEnumerable()
					.Where(_ => _.Value is not null)
					.ToDictionary(_ => _.Value.ToString().Replace("\"", ""), _ => AccountId.FromString(_.Key.Replace("\"", ""))); 

				Executor executor = CreateExecutor();
				Network network = Network.ForNetwork(executor, nodes);
                MirrorNetwork mirrorNetwork = MirrorNetwork.ForNetwork(executor, []);
                long shardValue = shard?.GetAsLong() ?? 0;
                long realmValue = realm?.GetAsLong() ?? 0;
                
                client = new Client(executor, network, mirrorNetwork, null, true, null, shardValue, realmValue);
                
                SetNetworkNameOn(client);

                return client;
            }
            internal Client ClientFromNetworkString()
            {
                return network.GetAsString() switch
                {
                    MAINNET => ForMainnet(),
                    TESTNET => ForTestnet(),
                    PREVIEWNET => ForPreviewnet(),

                    _ => new JsonException("Illegal argument for network.")};
            }
			internal void SetNetworkNameOn(Client client)
			{
				if (networkName != null)
				{
					var networkNameString = networkName.GetAsString();
					try
					{
						client.SetNetworkName(NetworkName.Parse(networkNameString));
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
                if (mirrorNetwork.IsJsonArray())
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
                client.MirrorNetwork_ = mirrorNetwork.GetString() switch
                {
                    MAINNET => MirrorNetwork.ForMainnet(client.executor),
                    TESTNET => MirrorNetwork.ForTestnet(client.executor),
                    PREVIEWNET => MirrorNetwork.ForPreviewnet(client.executor),

                    _ => throw new JsonException("Illegal argument for mirrorNetwork."),
                };
            }
            internal void SetMirrorNetworksFromJsonArray(Client client)
            {
                var mirrors = mirrorNetwork.GetAsJsonArray();
                IList<string> listMirrors = GetListMirrors(mirrors);
                client.SetMirrorNetwork(listMirrors);
            }
            internal IList<string> GetListMirrors(JsonArray mirrors)
            {
                IList<string> listMirrors = new (mirrors.Count);
                for (var i = 0; i < mirrors.Count; i++)
                {
                    listMirrors.Add(mirrors[i].ToString().Replace("\"", ""));
                }

                return listMirrors;
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