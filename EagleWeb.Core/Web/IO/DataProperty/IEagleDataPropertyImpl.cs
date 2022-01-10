using EagleWeb.Common.Auth;
using EagleWeb.Common.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.Web.IO.DataProperty
{
    public interface IEagleDataPropertyImpl
    {
        string Id { get; }
        Type Type { get; }
        bool WebWritable { get; }
        IReadOnlyList<string> WebRequiredPermissions { get; }
    }
}
