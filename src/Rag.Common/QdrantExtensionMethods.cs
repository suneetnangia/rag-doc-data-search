namespace Rag.Common;

using System.Text.Json;
using Google.Protobuf.Collections;
using Qdrant.Client.Grpc;

internal static class QdrantExtensionMethods
{
    public static T ConvertToBaseVectorDbRecord<T>(this MapField<string, Value> payload)
    where T : BaseVectorDbRecord
    {
        ArgumentNullException.ThrowIfNull(payload);

        var dictionary = new Dictionary<string, object>();
        foreach (var kvp in payload)
        {
            dictionary[kvp.Key] = kvp.Value.KindCase switch
            {
                Value.KindOneofCase.StringValue => kvp.Value.StringValue,
                Value.KindOneofCase.IntegerValue => kvp.Value.IntegerValue,
                Value.KindOneofCase.DoubleValue => kvp.Value.DoubleValue,
                Value.KindOneofCase.StructValue => kvp.Value.StructValue,
                Value.KindOneofCase.BoolValue => kvp.Value.BoolValue,
                _ => throw new InvalidOperationException($"Unsupported value type {kvp.Value.KindCase}.")
            };
        }

        var jsonString = JsonSerializer.Serialize(dictionary);
        var dataVectorDbRecord = JsonSerializer.Deserialize<T>(jsonString);

        return dataVectorDbRecord ?? throw new InvalidOperationException("DataVectorDbRecord is null.");
    }

    public static MapField<string, Value> ConvertToMapField(this BaseVectorDbRecord obj)
    {
        string jsonString = obj switch
        {
            DocumentVectorDbRecord documentVectorDbRecord => JsonSerializer.Serialize(documentVectorDbRecord),
            DataVectorDbRecord dataVectorDbRecord => JsonSerializer.Serialize(dataVectorDbRecord),
            _ => JsonSerializer.Serialize(obj),
        };

        // Deserialize the JSON string into a dictionary
        var jsonDictionary = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonString);
        var mapField = new MapField<string, Value>();

        if (jsonDictionary != null)
        {
            foreach (var kvp in jsonDictionary)
            {
                mapField[kvp.Key] = ConvertJsonElementToValue(kvp.Value);
            }
        }

        return mapField;
    }

    private static Value ConvertJsonElementToValue(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                return new Value { StringValue = element.GetString() };
            case JsonValueKind.Number:
                if (element.TryGetInt64(out var l))
                {
                    return new Value { IntegerValue = l };
                }

                if (element.TryGetDouble(out var d))
                {
                    return new Value { DoubleValue = d };
                }

                break;
            case JsonValueKind.True:
            case JsonValueKind.False:
                return new Value { BoolValue = element.GetBoolean() };
            case JsonValueKind.Null:
                return new Value { NullValue = NullValue.NullValue };
            case JsonValueKind.Object:
                var obj = new Struct();
                foreach (var prop in element.EnumerateObject())
                {
                    obj.Fields[prop.Name] = ConvertJsonElementToValue(prop.Value);
                }

                return new Value { StructValue = obj };
            case JsonValueKind.Array:
                var array = new List<Value>();
                foreach (var item in element.EnumerateArray())
                {
                    array.Add(ConvertJsonElementToValue(item));
                }

                return new Value { ListValue = new ListValue { Values = { array } } };
        }

        throw new InvalidOperationException($"Unsupported JsonValueKind: {element.ValueKind}");
    }
}
