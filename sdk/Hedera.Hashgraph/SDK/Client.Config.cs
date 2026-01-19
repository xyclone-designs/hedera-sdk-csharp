using Google.Protobuf.WellKnownTypes;
using Hedera.Hashgraph.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Hedera.Hashgraph.SDK
{
    public sealed partial class Client 
    {
		private class ConfigOperator
		{
			private string? AccountId { get; set; }
			private string? PrivateKey { get; set; }
		}

		private class Config
		{
			private JsonElement? Network { get; set; }
			private JsonElement? NetworkName { get; set; }
			private ConfigOperator? Operator { get; set; }
			private JsonElement? MirrorNetwork { get; set; }
			private JsonElement? Shard { get; set; }
			private JsonElement? Realm { get; set; }

			private static Config FromString(string json)
			{
				//return new Gson().FromJson((Reader) new StringReader(json), Config.class);
			}
			private static Config FromJson(Reader json)
			{
				//return new Gson().FromJson(json, Config.class);
			}

			
			private Client InitializeWithNetwork()
			{
				if (network == null)
				{
					throw new Exception("Network is not set in provided json object");
				}

				Client client;
				if (network.isJsonObject())
				{
					client = clientFromNetworkJson();
				}
				else
				{
					client = clientFromNetworkString();
				}
				return client;
			}
			private Client ClientFromNetworkJson()
			{
				Client client;
				var networkJson = network.getAsJsonObject();
				Dictionary<string, AccountId> nodes = Client.getNetworkNodes(networkJson);
				var executor = createExecutor();
				var network = Network.forNetwork(executor, nodes);
				var mirrorNetwork = MirrorNetwork.forNetwork(executor, new ArrayList<>());
				var shardValue = Shard != null ? Shard.getAsLong() : 0;
				var realmValue = Realm != null ? Realm.getAsLong() : 0;
				client = new Client(executor, network, mirrorNetwork, null, true, null, shardValue, realmValue);
				setNetworkNameOn(client);
				return client;
			}
			private Client ClientFromNetworkString()
			{
				return switch (network.getAsString())
				{
					case MAINNET => Client.ForMainnet();
					case TESTNET => Client.ForTestnet();
					case PREVIEWNET => Client.ForPreviewnet();
					default -> throw new JsonParseException("Illegal argument for network.");
				}
				;
			}

			private List<string> GetListMirrors(JsonArray mirrors)
			{
				return [.. mirrors.Select(_ => _.GetValue<string>().Replace("\"", ""))];
			}

			private void SetOperatorOn(Client client)
			{
				if (Operator != null) {
					AccountId operatorAccount = AccountId.FromString(operator.accountId);
					PrivateKey privateKey = PrivateKey.FromString(operator.privateKey);
					client.setOperator(operatorAccount, privateKey);
				}
			}
			private void SetMirrorNetwork(Client client)
			{
				if (mirrorNetwork.isJsonArray())
				{
					setMirrorNetworksFromJsonArray(client);
				}
				else
				{
					setMirrorNetworkFromString(client);
				}
			}
			private void SetNetworkNameOn(Client client)
			{
				if (networkName != null)
				{
					var networkNameString = networkName.getAsString();
					try
					{
						client.setNetworkName(NetworkName.FromString(networkNameString));
					}
					catch (Exception ignored)
					{
						throw new ArgumentException("networkName in config was \"" + networkNameString
								+ "\", expected either \"mainnet\", \"testnet\" or \"previewnet\"");
					}
				}
			}
			private void SetMirrorNetworkOn(Client client)
			{
				if (mirrorNetwork != null)
				{
					setMirrorNetwork(client);
				}
			}
			private void SetMirrorNetworkFromString(Client client)
			{
				string mirror = mirrorNetwork.getAsString();
				switch (mirror)
				{
					case Client.MAINNET => client.mirrorNetwork = MirrorNetwork.forMainnet(client.executor);
					case Client.TESTNET => client.mirrorNetwork = MirrorNetwork.forTestnet(client.executor);
					case Client.PREVIEWNET => client.mirrorNetwork = MirrorNetwork.forPreviewnet(client.executor);
					default -> throw new JsonParseException("Illegal argument for mirrorNetwork.");
				}
			}
			private void SetMirrorNetworksFromJsonArray(Client client)
			{
				var mirrors = mirrorNetwork.getAsJsonArray();
				List<string> listMirrors = getListMirrors(mirrors);
				client.setMirrorNetwork(listMirrors);
			}

			private Client ToClient()
			{
				Client client = initializeWithNetwork();
				setOperatorOn(client);
				setMirrorNetworkOn(client);
				return client;
			}
		}
	}
}