using System;
using System.Collections.Generic;
using System.Text;

namespace Easy.HttpClient.Attributes
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class HttpClientAttribute:Attribute
    {
        public string ApiUrl { get; set; }
        public HttpClientAttribute()
        {

        }
        public HttpClientAttribute(string apiUrl)
        {
            ApiUrl = apiUrl;
        }
    }
}
