using System;
using System.Collections.Generic;
using System.Text;

namespace Easy.HttpClient.Attributes.Params
{
    public class BodyAttribute : ParamAttribute
    {
        public BodyAttribute(string name = null) : base(ParamType.Body, name)
        {
        }
    }
}
