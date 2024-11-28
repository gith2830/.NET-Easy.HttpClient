using System;
using System.Collections.Generic;
using System.Text;

namespace Easy.HttpClient.Attributes.Params
{
    public class BodyAttribute : ParamAttribute
    {
        public BodyAttribute(string name = null, bool isBoxing = false) : base(ParamType.Body, name, isBoxing)
        {
        }
    }
}
