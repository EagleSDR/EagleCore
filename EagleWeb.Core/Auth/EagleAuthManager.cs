using EagleWeb.Common;
using EagleWeb.Core.Misc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace EagleWeb.Core.Auth
{
    public class EagleAuthManager
    {
        public EagleAuthManager(EagleContext ctx, string authFilename)
        {
            this.ctx = ctx;
            accounts = new DataFile<List<EagleAccountData>>(authFilename, new List<EagleAccountData>());
        }

        private readonly EagleContext ctx;

        private DataFile<List<EagleAccountData>> accounts;

        public int AccountCount => accounts.Data.Count;

        public void Save()
        {
            accounts.Save();
        }

        private void Log(EagleLogLevel level, string topic, string message)
        {
            ctx.Log(level, "EagleAuthManager-" + topic, message);
        }

        public bool FindAccountByUsername(string username, out EagleAccount account)
        {
            if (FindAccountDataByUsername(username, out EagleAccountData data))
            {
                account = new EagleAccount(data, this);
                return true;
            } else
            {
                account = null;
                return false;
            }
        }

        public bool FindAccountDataByUsername(string username, out EagleAccountData data)
        {
            lock(accounts)
            {
                foreach (EagleAccountData account in accounts.Data)
                {
                    if (account.username == username)
                    {
                        data = account;
                        return true;
                    }
                }
                data = null;
                return false;
            }
        }

        public bool Authenticate(string username, string password, out EagleAccount account)
        {
            //Search for an account with this username
            account = null;
            if (!FindAccountDataByUsername(username, out EagleAccountData data))
                return false;

            //Verify password
            byte[] challenge = HashPassword(password, data.salt);
            if (!CompareBytes(challenge, data.password_sha256))
                return false;

            //OK! Wrap
            account = new EagleAccount(data, this);
            return true;
        }

        public bool CreateUser(string username, string password, out EagleAccount account)
        {
            //Generate salt
            byte[] salt = GenerateSalt();

            //Create the account data
            EagleAccountData data = new EagleAccountData
            {
                username = username,
                password_sha256 = HashPassword(password, salt),
                salt = salt
            };

            //Enter lock for accounts
            lock(accounts)
            {
                //Make sure the user doesn't already exist...
                account = null;
                if (FindAccountDataByUsername(username, out EagleAccountData foundData))
                    return false;

                //Add and save
                accounts.Data.Add(data);
                accounts.Save();
            }

            //Wrap
            account = new EagleAccount(data, this);

            //Log
            Log(EagleLogLevel.INFO, "CreateUser", $"Account \"{username}\" created successfully.");

            return true;
        }

        public byte[] HashPassword(string password, byte[] salt)
        {
            //Convert the password to bytes...
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            //Append the salt to this
            byte[] challenge = new byte[passwordBytes.Length + salt.Length];
            passwordBytes.CopyTo(challenge, 0);
            salt.CopyTo(challenge, passwordBytes.Length);

            //Apply SHA-256 hash
            byte[] hash;
            using (SHA256 sha = SHA256.Create())
                hash = sha.ComputeHash(challenge);

            return hash;
        }

        public byte[] GenerateSalt()
        {
            byte[] salt = new byte[16];
            using (RNGCryptoServiceProvider gen = new RNGCryptoServiceProvider())
                gen.GetBytes(salt);
            return salt;
        }

        private bool CompareBytes(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                    return false;
            }
            return true;
        }
    }
}
