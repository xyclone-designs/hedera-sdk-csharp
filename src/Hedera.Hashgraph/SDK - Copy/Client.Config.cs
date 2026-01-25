// SPDX-License-Identifier: Apache-2.0
using Com.Google.Common.Annotations;
using Com.Google.Common.Util.Concurrent;
using Com.Google.Gson;
using Google.Protobuf.WellKnownTypes;
using Hedera.Hashgraph.SDK.Keys;
using Hedera.Hashgraph.SDK.HBar;
using Hedera.Hashgraph.SDK.Logger;
using Hedera.Hashgraph.SDK.Transactions.Account;
using Java.Io;
using Java.Nio.Charset;
using Java.Nio.File;
using Java.Time;
using Java.Util;
using Java.Util.Concurrent.Atomic;
using Java.Util.Function;
using Javax.Annotation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Hedera.Hashgraph.SDK.BadMnemonicReason;

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

            private static Config FromString(string json)
            {
                return new Gson().FromJson((Reader)new StringReader(json), typeof(Config));
            }

            private static Config FromJson(Reader json)
            {
                return new Gson().FromJson(json, typeof(Config));
            }

            private Client ToClient()
            {
                Client client = InitializeWithNetwork();
                SetOperatorOn(client);
                SetMirrorNetworkOn(client);
                return client;
            }

            private Client InitializeWithNetwork()
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

            private Client ClientFromNetworkJson()
            {
                Client client;
                var networkJson = network.GetAsJsonObject();
                Dictionary<string, AccountId> nodes = Client.GetNetworkNodes(networkJson);
                
                Executor executor = CreateExecutor();
				Network network = Network.ForNetwork(executor, nodes);
                MirrorNetwork mirrorNetwork = MirrorNetwork.ForNetwork(executor, []);
                long shardValue = shard?.GetAsLong() ?? 0;
                long realmValue = realm?.GetAsLong() ?? 0;
                
                client = new Client(executor, network, mirrorNetwork, null, true, null, shardValue, realmValue);
                
                SetNetworkNameOn(client);

                return client;
            }

            private void SetNetworkNameOn(Client client)
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

            private Client ClientFromNetworkString()
            {
                return network.GetAsString() switch
                {
                    MAINNET => ForMainnet(),
                    TESTNET => ForTestnet(),
                    PREVIEWNET => ForPreviewnet(),

                    _ => new JsonParseException("Illegal argument for network.")};
            }

            private void SetMirrorNetworkOn(Client client)
            {
                if (mirrorNetwork != null)
                {
                    SetMirrorNetwork(client);
                }
            }

            private void SetMirrorNetwork(Client client)
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

            private void SetMirrorNetworkFromString(Client client)
            {
                string mirror = mirrorNetwork.GetAsString();
                client.mirrorNetwork = mirror switch
                {
                    MAINNET => MirrorNetwork.ForMainnet(client.executor),
                    TESTNET => MirrorNetwork.ForTestnet(client.executor),
                    PREVIEWNET => MirrorNetwork.ForPreviewnet(client.executor),

                    _ => throw new JsonParseException("Illegal argument for mirrorNetwork."),
                };
            }

            private void SetMirrorNetworksFromJsonArray(Client client)
            {
                var mirrors = mirrorNetwork.GetAsJsonArray();
                IList<string> listMirrors = GetListMirrors(mirrors);
                client.SetMirrorNetwork(listMirrors);
            }

            private IList<string> GetListMirrors(JsonArray mirrors)
            {
                IList<string> listMirrors = new (mirrors.Count);
                for (var i = 0; i < mirrors.Count; i++)
                {
                    listMirrors.Add(mirrors[i].GetAsString().Replace("\"", ""));
                }

                return listMirrors;
            }

            private void SetOperatorOn(Client client)
            {
                if (oper8r != null)
                {
                    AccountId operatorAccount = AccountId.FromString(oper8r.AccountId);
                    PrivateKey privateKey = PrivateKey.FromString(oper8r.PrivateKey);
                    client.SetOperator(operatorAccount, privateKey);
                }
            }
        }
    }
}