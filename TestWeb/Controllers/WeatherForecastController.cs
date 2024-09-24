using Microsoft.AspNetCore.Mvc;
using TestApp.Clients;

namespace TestWeb.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly EmployeeClient _employeeClient;

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, EmployeeClient employeeClient)
        {
            _logger = logger;
            _employeeClient = employeeClient;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public object Get()
        {
            Guid adminId = Guid.Parse("7bbc37a3-a8d3-44af-a073-60a9606f8a98");
            var result = _employeeClient.GetEmployeeByAdminId(adminId, "abcd");
            return result;
        }
    }
}
