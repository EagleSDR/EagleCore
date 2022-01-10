using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.IO.DataProperty
{
    public class EagleDataPropertySetException : Exception
    {
        public EagleDataPropertySetException(EagleDataPropertySetStatus status, string message) : base(status.ToString() + ": " + message)
        {
            propStatus = status;
            propMessage = message;
        }

        private EagleDataPropertySetStatus propStatus;
        private string propMessage;

        public EagleDataPropertySetStatus PropStatus => propStatus;
        public string PropMessage => propMessage;
    }
}
