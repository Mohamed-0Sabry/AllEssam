using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AlIssam.API.Dtos.Request;
using System;
using System.Collections.Generic;

public class QuantitiesConverter : JsonConverter<List<Quantity>>
{
    public override List<Quantity> ReadJson(JsonReader reader, Type objectType, List<Quantity> existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        // If the value is a string, deserialize it into a List<QuantityRequest>
        if (reader.TokenType == JsonToken.String)
        {
            var jsonString = reader.Value.ToString();
            return JsonConvert.DeserializeObject<List<Quantity>>(jsonString);
        }

        // If the value is already an array, deserialize it directly
        if (reader.TokenType == JsonToken.StartArray)
        {
            return serializer.Deserialize<List<Quantity>>(reader);
        }

        // Handle unexpected token types
        throw new JsonSerializationException("Unexpected token type for Quantities.");
    }

    public override void WriteJson(JsonWriter writer, List<Quantity> value, JsonSerializer serializer)
    {
        // Serialize the List<QuantityRequest> back to a JSON string
        var jsonString = JsonConvert.SerializeObject(value);
        writer.WriteValue(jsonString);
    }
}