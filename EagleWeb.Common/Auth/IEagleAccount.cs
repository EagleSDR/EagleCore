using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.Auth
{
    public interface IEagleAccount
    {
        /// <summary>
        /// Gets the account username.
        /// </summary>
        string Username { get; }
        
        /// <summary>
        /// Gets/sets if this account is an admin account. 
        /// </summary>
        bool IsAdmin { get; set; }

        /// <summary>
        /// Returns true/false if this account has this permission.
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        bool HasPermission(string permission);

        /// <summary>
        /// Throws an exception if this account does NOT have this permission. Otherwise, does nothing.
        /// </summary>
        /// <param name="permission"></param>
        void EnsureHasPermission(string permission);
    }
}
