using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hedera.Hashgraph.SDK.Utils
{
    internal class JsonHelper
    {
        public static readonly JsonSerializerOptions Options = new ()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        };

        public static string ToJsonString(object obj, JsonSerializerOptions? options = null)
        {
            return JsonSerializer.Serialize(obj, options ?? Options);
        }
	}
}
