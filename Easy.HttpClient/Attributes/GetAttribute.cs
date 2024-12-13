using System;
using System.Collections.Generic;
using System.Text;

namespace Easy.HttpClient.Attributes
{
    public class GetAttribute:HttpMethodAttribute
    {
        public GetAttribute(string template = null, bool canCancel = false, int timeout = 3000) :base("Get", template, canCancel, timeout)
        {
            
        }
    }
}
