using System;
using System.Collections.Generic;
using System.Text;

namespace Easy.HttpClient.Attributes
{
    public class PostAttribute : HttpMethodAttribute
    {
        public PostAttribute(string template, bool canCancel = false, int timeout = 3000) : base("Post", template, canCancel, timeout)
        {
        }
    }
}
