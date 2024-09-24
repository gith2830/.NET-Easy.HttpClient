using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp.Models
{
    public class ApiResult
    {
        public bool Success { get; set; }
        public int Code { get; set; }
        public string[] Msg { get; set; }
        public object Data {  get; set; }
    }
}
