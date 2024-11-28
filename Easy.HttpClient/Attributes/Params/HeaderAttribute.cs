using System;
using System.Collections.Generic;
using System.Text;

namespace Easy.HttpClient.Attributes.Params
{
    public class HeaderAttribute : ParamAttribute
    {
        public HeaderAttribute(string name = null, bool isBoxing = false) : base(ParamType.Header, name, isBoxing)
        {
        }
    }
}
