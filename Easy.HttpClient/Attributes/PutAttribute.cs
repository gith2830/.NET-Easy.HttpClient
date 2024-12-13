using System;
using System.Collections.Generic;
using System.Text;

namespace Easy.HttpClient.Attributes
{
    public class PutAttribute : HttpMethodAttribute
    {
        public PutAttribute(string template = null, bool canCancel = false, int timeout = 3000) : base("Put", template, canCancel, timeout)
        {
        }
    }
}
