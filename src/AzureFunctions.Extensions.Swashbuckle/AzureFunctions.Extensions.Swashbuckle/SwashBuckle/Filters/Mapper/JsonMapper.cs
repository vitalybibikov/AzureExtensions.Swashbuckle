using System.Text.Json;
using System.Text.Json.Nodes;

namespace AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Filters.Mapper
{
    public static class JsonMapper
    {
        public static JsonNode? CreateFromJson(string str)
        {
            try
            {
                var json = JsonSerializer.Serialize(str);
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

                if (jsonElement.ValueKind == JsonValueKind.True || jsonElement.ValueKind == JsonValueKind.False)
                {
                    return JsonValue.Create(jsonElement.GetBoolean());
                }

                if (jsonElement.ValueKind == JsonValueKind.Number)
                {
                    if (jsonElement.TryGetInt32(out var intValue))
                    {
                        return JsonValue.Create(intValue);
                    }

                    if (jsonElement.TryGetInt64(out var longValue))
                    {
                        return JsonValue.Create(longValue);
                    }

                    if (jsonElement.TryGetSingle(out var floatValue) && !float.IsInfinity(floatValue))
                    {
                        return JsonValue.Create(floatValue);
                    }

                    if (jsonElement.TryGetDouble(out var doubleValue))
                    {
                        return JsonValue.Create(doubleValue);
                    }
                }

                if (jsonElement.ValueKind == JsonValueKind.String)
                {
                    return JsonValue.Create(jsonElement.ToString());
                }

                if (jsonElement.ValueKind == JsonValueKind.Null)
                {
                    return null;
                }

                if (jsonElement.ValueKind == JsonValueKind.Array)
                {
                    return CreateJsonArray(jsonElement.EnumerateArray());
                }
            }
            catch
            {
            }

            return null;
        }

        private static JsonNode CreateJsonArray(IEnumerable<JsonElement> jsonElements)
        {
            var jsonArray = new JsonArray();

            foreach (var jsonElement in jsonElements)
            {
                var json = jsonElement.ValueKind == JsonValueKind.String
                    ? $"\"{jsonElement}\""
                    : jsonElement.ToString();

                jsonArray.Add(CreateFromJson(json));
            }

            return jsonArray;
        }
    }
}
