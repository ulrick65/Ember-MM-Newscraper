using System;
using System.Linq;
using Newtonsoft.Json;

namespace XBMCRPC.List.Filter
{
    [JsonConverter(typeof(OperatorEnumConverter))]
    public enum Operators
   {
       contains,
       doesnotcontain,
       Is,
       isnot,
       startswith,
       endswith,
       greaterthan,
       lessthan,
       after,
       before,
       inthelast,
       notinthelast,
       True,
       False,
       between,
   }
}

public class OperatorEnumConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        XBMCRPC.List.Filter.Operators op = (XBMCRPC.List.Filter.Operators)value;
        writer.WriteValue(op.ToString().ToLower());
    }
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.Value == null)
            return null;

        var values = Enum.GetNames(typeof(XBMCRPC.List.Filter.Operators));
        var enumValue = values.FirstOrDefault(v => v.ToLower().Equals(reader.Value.ToString().ToLower()));

        if (Enum.TryParse(enumValue, out XBMCRPC.List.Filter.Operators op))
        {
            return op;
        }
        return null;
    }
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(string);
    }
}
