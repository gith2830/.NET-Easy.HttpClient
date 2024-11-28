﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Easy.HttpClient.Attributes.Params
{
    public class PathAttribute : ParamAttribute
    {
        public PathAttribute(string name= null, bool isBoxing = false) : base(ParamType.Path, name, isBoxing)
        {
        }
    }
}
