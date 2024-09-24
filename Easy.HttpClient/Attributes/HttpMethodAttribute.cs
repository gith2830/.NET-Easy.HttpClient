using System;
using System.Collections.Generic;
using System.Text;

namespace Easy.HttpClient.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class HttpMethodAttribute:Attribute
    {
        public string Method { get; }
        public string Template { get; }
        public bool CanCancel { get; set; }
        public int Timeout { get; set; }
        protected HttpMethodAttribute(string method,string template, bool canCancel, int timeout)
        {
            Method = method;
            Template = template;
            CanCancel = canCancel;
            Timeout = timeout;
        }
    }
}
