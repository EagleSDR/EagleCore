using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.IO
{
    public delegate JObject EaglePortWrappedApiHandler(IEagleClient client, JObject payload);

    public static class EaglePortHelpers
    {
        public static JToken GetToken(this JObject ctx, string key)
        {
            if (ctx.TryGetValue(key, out JToken value))
                return value;
            throw new Exception($"Request missing required value \"{key}\".");
        }

        public static bool TryGetString(this JObject ctx, string key, out string result)
        {
            if (ctx.TryGetValue(key, out JToken value) && value.Type == JTokenType.String)
            {
                result = (string)value;
                return true;
            } else
            {
                result = null;
                return false;
            }
        }

        public static string GetString(this JObject ctx, string key)
        {
            if (TryGetString(ctx, key, out string result))
                return result;
            throw new Exception($"Request missing required string \"{key}\".");
        }

        public static bool TryGetInt(this JObject ctx, string key, out int result)
        {
            if (ctx.TryGetValue(key, out JToken value) && value.Type == JTokenType.Integer)
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
            if (TryGetInt(ctx, key, out int result))
                return result;
            throw new Exception($"Request missing required int \"{key}\".");
        }

        public static bool TryGetBool(this JObject ctx, string key, out bool result)
        {
            if (ctx.TryGetValue(key, out JToken value) && value.Type == JTokenType.Boolean)
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
            if (TryGetBool(ctx, key, out bool result))
                return result;
            throw new Exception($"Request missing required bool \"{key}\".");
        }

        public static bool TryGetObject(this JObject ctx, string key, out JObject result)
        {
            if (ctx.TryGetValue(key, out JToken value) && value.Type == JTokenType.Object)
            {
                result = (JObject)value;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        public static JObject GetObject(this JObject ctx, string key)
        {
            if (TryGetObject(ctx, key, out JObject result))
                return result;
            throw new Exception($"Request missing required object \"{key}\".");
        }

        public static IEaglePortIO BindWrappedApi(this IEaglePortIO ctx, EaglePortWrappedApiHandler handler)
        {
            ctx.OnReceive += (IEagleClient client, JObject payload) =>
            {
                JObject response = new JObject();
                try
                {
                    response["result"] = handler(client, payload);
                    response["ok"] = true;
                } catch (Exception ex)
                {
                    response["ok"] = false;
                    response["error"] = ex.Message;
                }
                ctx.Send(response, client);
            };
            return ctx;
        }
    }
}
