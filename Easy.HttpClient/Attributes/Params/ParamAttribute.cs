using System;
using System.Collections.Generic;
using System.Text;

namespace Easy.HttpClient.Attributes.Params
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ParamAttribute:Attribute
    {
        public ParamType ParamType { get; set; }
        public string Name { get; set; }
        public ParamAttribute(ParamType paramType,string name)
        {
            ParamType = paramType;
            Name = name;
        }
    }
}
