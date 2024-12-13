using System;
using System.Collections.Generic;
using System.Text;

namespace Easy.HttpClient.Attributes
{
    public class DeleteAttribute : HttpMethodAttribute
    {
        public DeleteAttribute(string template = null, bool canCancel = false, int timeout = 3000) : base("Delete", template,canCancel,timeout)
        {
        }
    }
}
