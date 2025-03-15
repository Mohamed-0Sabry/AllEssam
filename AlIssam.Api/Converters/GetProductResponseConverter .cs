//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using AlIssam.API.Dtos.Response;
//using System;
//using System.Linq;

//public class GetProductResponseConverter : JsonConverter<GetProductResponse>
//{
//    public override void WriteJson(JsonWriter writer, GetProductResponse value, JsonSerializer serializer)
//    {
//        // Create the root object
//        var root = new JObject
//        {
//            ["image"] = value.Cover_Image,
//            ["ar"] = new JObject
//            {
//                ["name"] = value.Name_Ar,
//                ["description"] = value.Description_Ar
//            },
//            ["en"] = new JObject
//            {
//                ["name"] = value.Name_En,
//                ["description"] = value.Description_En
//            },
//            ["quantities"] = JArray.FromObject(value.Quantities.Select(q => new
//            {
//                ar = new
//                {
//                    name = q.Name_Ar // Arabic name
//                },
//                en = new
//                {
//                    name = q.Name_En // English name
//                },
//                Price = q.Price // Shared price
//            })),
//            ["imagesPath"] = JArray.FromObject(value.ImagesPath)
//        };

//        // Write the root object to the JSON writer
//        root.WriteTo(writer);
//    }

//    public override GetProductResponse ReadJson(JsonReader reader, Type objectType, GetProductResponse existingValue, bool hasExistingValue, JsonSerializer serializer)
//    {
//        throw new NotImplementedException(); // Not needed for serialization
//    }
//}