using EagleWeb.Core.Misc;
using EagleWeb.Core.Web;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace EagleWeb.Core.Auth
{
    public class EagleSessionManager
    {
        public EagleSessionManager(EagleAuthManager auth, string filename)
        {
            this.auth = auth;
            sessions = new DataFile<Dictionary<string, string>>(filename, new Dictionary<string, string>());
        }

        private EagleAuthManager auth;
        private DataFile<Dictionary<string, string>> sessions;
        private Dictionary<string, EagleAccount> cache = new Dictionary<string, EagleAccount>();

        //private ConcurrentDictionary<string, EagleAccount> sessions = new ConcurrentDictionary<string, EagleAccount>();

        public bool Authenticate(string token, out EagleAccount account)
        {
            lock (cache)
            {
                //First, check the cache
                if (cache.ContainsKey(token))
                {
                    account = cache[token];
                    return true;
                }

                //Next, try checking our data file
                lock (sessions)
                {
                    if (sessions.Data.ContainsKey(token) && auth.FindAccountByUsername(sessions.Data[token], out account))
                    {
                        cache.Add(token, account);
                        return true;
                    }
                }
            }

            account = null;
            return false;
        }

        public string CreateSession(EagleAccount account)
        {
            string token;
            lock (cache)
            {
                lock (sessions)
                {
                    //Generate token
                    do
                    {
                        token = EagleWebHelpers.GenerateToken(32);
                    } while (sessions.Data.ContainsKey(token) || cache.ContainsKey(token));

                    //Add to cache
                    cache.Add(token, account);

                    //Add to data file and save
                    sessions.Data.Add(token, account.Username);
                    sessions.Save();
                }
            }
            return token;
        }
    }
}
