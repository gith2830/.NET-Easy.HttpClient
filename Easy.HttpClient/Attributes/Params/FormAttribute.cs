using System;
using System.Collections.Generic;
using System.Text;

namespace Easy.HttpClient.Attributes.Params
{
    public class FormAttribute: ParamAttribute
    {
        public FormAttribute(string name = null) :base(ParamType.Form,name)
        {
            
        }
    }
}
