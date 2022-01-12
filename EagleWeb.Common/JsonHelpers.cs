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

        public static JObject GetObject(this JObject ctx, string key)
        {
            if (ctx.TryGetObject(key, out JObject result))
                return result;
            throw new Exception($"Required Object field \"{key}\" is missing.");
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

        public static string GetString(this JObject ctx, string key)
        {
            if (ctx.TryGetString(key, out string result))
                return result;
            throw new Exception($"Required String field \"{key}\" is missing.");
        }

        public static bool TryGetInt(this JObject ctx, string key, out int result)
        {
            if (ctx.TryGetValue(key, out JToken value) && value != null && value.Type == JTokenType.Integer)
            {
                result = (int)value;
                return true;
            }
            else
            {
                result = 0;
                return false;
            }
        }

        public static int GetInt(this JObject ctx, string key)
        {
            if (ctx.TryGetInt(key, out int result))
                return result;
            throw new Exception($"Required Int field \"{key}\" is missing.");
        }

        public static bool TryGetFloat(this JObject ctx, string key, out float result)
        {
            if (ctx.TryGetValue(key, out JToken value) && value != null && (value.Type == JTokenType.Integer || value.Type == JTokenType.Float))
            {
                result = (float)value;
                return true;
            }
            else
            {
                result = 0;
                return false;
            }
        }

        public static float GetFloat(this JObject ctx, string key)
        {
            if (ctx.TryGetFloat(key, out float result))
                return result;
            throw new Exception($"Required Float field \"{key}\" is missing.");
        }

        public static bool TryGetBool(this JObject ctx, string key, out bool result)
        {
            if (ctx.TryGetValue(key, out JToken value) && value != null && value.Type == JTokenType.Boolean)
            {
                result = (bool)value;
                return true;
            }
            else
            {
                result = false;
                return false;
            }
        }

        public static bool GetBool(this JObject ctx, string key)
        {
            if (ctx.TryGetBool(key, out bool result))
                return result;
            throw new Exception($"Required Boolean field \"{key}\" is missing.");
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

        public static T GetEnum<T>(this JObject ctx, string key) where T : struct
        {
            if (ctx.TryGetEnum(key, out T result))
                return result;
            throw new Exception($"Required Enum field \"{key}\" is missing.");
        }
    }
}
