using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace EagleWeb.Common
{
    public static class JsonHelpers
    {
        public static bool TryGetObject(this JObject ctx, string key, out JObject result)
        {
            if (ctx.TryGetValue(key, out JToken value) && value != null && value.Type == JTokenType.Object)
            {
                result = (JObject)value;
                return true;
            } else
            {
                result = null;
                return false;
            }
        }

        public static bool TryGetString(this JObject ctx, string key, out string result)
        {
            if (ctx.TryGetValue(key, out JToken value) && value != null && value.Type == JTokenType.String)
            {
                result = (string)value;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        public static bool TryGetEnum<T>(this JObject ctx, string key, out T result) where T : struct
        {
            if (ctx.TryGetString(key, out string value) && Enum.TryParse(value, out result))
            {
                return true;
            } else
            {
                result = default(T);
                return false;
            }
        }
    }
}
