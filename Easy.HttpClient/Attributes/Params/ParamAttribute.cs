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
        public bool IsBoxing { get; set; }
        public ParamAttribute(ParamType paramType,string name,bool isBoxing)
        {
            ParamType = paramType;
            Name = name;
            IsBoxing = isBoxing;
        }
    }
}
