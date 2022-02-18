using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EagleWeb.Core.Web
{
    public static class EagleWebHelpers
    {
        public static bool TryGetString(this IQueryCollection collection, string key, out string result)
        {
            if (collection.ContainsKey(key))
            {
                result = (string)collection[key];
                return true;
            } else
            {
                result = null;
                return false;
            }
        }

        public static bool TryGetString(this IFormCollection collection, string key, out string result)
        {
            if (collection.ContainsKey(key))
            {
                result = (string)collection[key];
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        public static async Task<T> ReadAsJsonAsync<T>(this HttpRequest request)
        {
            //Read
            string raw;
            using (StreamReader sr = new StreamReader(request.Body))
                raw = await sr.ReadToEndAsync();

            //Decode as JSON
            return JsonConvert.DeserializeObject<T>(raw);
        }

        public static Task WriteJsonAsync<T>(this HttpResponse response, T value)
        {
            byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value));
            response.ContentType = "application/json";
            response.ContentLength = data.Length;
            return response.Body.WriteAsync(data, 0, data.Length);
        }

        private static RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
        private static readonly char[] CHARSET = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPLKJHGFDSAZXCVBNM1234567890".ToCharArray();

        public static string GenerateToken(int length)
        {
            byte[] bytes = new byte[length];
            crypto.GetBytes(bytes);

            char[] token = new char[bytes.Length];
            for (int i = 0; i < token.Length; i++)
                token[i] = CHARSET[bytes[i] % CHARSET.Length];

            return new string(token);
        }
    }
}
