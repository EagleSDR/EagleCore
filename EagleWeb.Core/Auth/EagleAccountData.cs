using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.Auth
{
    public class EagleAccountData
    {
        public string username;
        public byte[] password_sha256;
        public byte[] salt;

        public bool is_admin;
    }
}
