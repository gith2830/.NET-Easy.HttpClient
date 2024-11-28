﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Easy.HttpClient.Attributes.Params
{
    public class QueryAttribute : ParamAttribute
    {
        public QueryAttribute(string name = null, bool isBoxing = false) : base(ParamType.Query, name, isBoxing)
        {
        }
    }
}
