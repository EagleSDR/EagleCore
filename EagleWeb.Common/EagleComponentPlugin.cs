using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common
{
    public abstract class EagleComponentPlugin : EagleComponent
    {
        protected EagleComponentPlugin(EagleComponent parent, string id) : base(parent, id)
        {
        }

        public abstract void InitPlugin();
    }
}
