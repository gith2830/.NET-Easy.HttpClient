using Easy.HttpClient.Attributes;
using Easy.HttpClient.Attributes.Params;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestApp.Models;

namespace TestApp.Clients
{
    [HttpClient("http://127.0.0.1")]
    public interface EmployeeClient
    {
        [Get("employee/{eid}")]
        public ApiResult GetEmployeeByAdminId([Path("eid")] Guid id, [Header("test")] string innerHeader);
    }
}
