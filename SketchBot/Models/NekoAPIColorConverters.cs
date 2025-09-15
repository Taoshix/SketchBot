using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace SketchBot.Models
{
    public class NekoAPIColorConverter : JsonConverter<NekoAPIColor>
    {
        public override NekoAPIColor ReadJson(JsonReader reader, Type objectType, NekoAPIColor existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JArray arr = JArray.Load(reader);
            if (arr.Count != 3)
                throw new JsonSerializationException("Color array must have exactly 3 elements.");
            return new NekoAPIColor
            {
                R = arr[0].Value<int>(),
                G = arr[1].Value<int>(),
                B = arr[2].Value<int>()
            };
        }

        public override void WriteJson(JsonWriter writer, NekoAPIColor value, JsonSerializer serializer)
        {
            JArray arr = new JArray { value.R, value.G, value.B };
            arr.WriteTo(writer);
        }
    }

    public class NekoAPIColorListConverter : JsonConverter<List<NekoAPIColor>>
    {
        public override List<NekoAPIColor> ReadJson(JsonReader reader, Type objectType, List<NekoAPIColor> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JArray arr = JArray.Load(reader);
            var list = new List<NekoAPIColor>();
            foreach (var item in arr)
            {
                if (item is JArray colorArr && colorArr.Count == 3)
                {
                    list.Add(new NekoAPIColor
                    {
                        R = colorArr[0].Value<int>(),
                        G = colorArr[1].Value<int>(),
                        B = colorArr[2].Value<int>()
                    });
                }
                else
                {
                    throw new JsonSerializationException("Each color in the palette must be an array of 3 integers.");
                }
            }
            return list;
        }

        public override void WriteJson(JsonWriter writer, List<NekoAPIColor> value, JsonSerializer serializer)
        {
            JArray arr = new JArray();
            foreach (var color in value)
            {
                arr.Add(new JArray { color.R, color.G, color.B });
            }
            arr.WriteTo(writer);
        }
    }
}
