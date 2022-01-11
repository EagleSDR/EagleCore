using EagleWeb.Common.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.Auth
{
    public class EagleAccount : IEagleAccount
    {
        public EagleAccount(EagleAccountData data, EagleAuthManager auth)
        {
            this.data = data;
            this.auth = auth;
        }

        private readonly EagleAccountData data;
        private readonly EagleAuthManager auth;

        public string Username { get => data.username; }
        public bool IsAdmin
        {
            get => data.is_admin;
            set
            {
                data.is_admin = value;
                auth.Save();
            }
        }

        public bool ChangeUsername(string username)
        {
            //Make sure it doesn't already exist
            if (auth.FindAccountDataByUsername(username, out EagleAccountData found))
                return false;

            //Apply
            data.username = username;
            auth.Save();

            return true;
        }

        public void ChangePassword(string password)
        {
            data.salt = auth.GenerateSalt();
            data.password_sha256 = auth.HashPassword(password, data.salt);
            auth.Save();
        }

        public bool HasPermission(string permission)
        {
            //Admin accounts have all permissions...
            if (IsAdmin)
                return true;

            //TODO!!
            return false;
        }

        public void EnsureHasPermission(string permission)
        {
            if (!HasPermission(permission))
                throw new Exception($"Requires permission \"{permission}\", but {Username} doesn't have this!");
        }
    }
}
