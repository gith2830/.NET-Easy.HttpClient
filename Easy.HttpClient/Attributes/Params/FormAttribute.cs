using System;
using System.Collections.Generic;
using System.Text;

namespace Easy.HttpClient.Attributes.Params
{
    public class FormAttribute: ParamAttribute
    {
        public FormAttribute(string name = null,bool isBoxing = false) :base(ParamType.Form,name, isBoxing)
        {
            
        }
    }
}
