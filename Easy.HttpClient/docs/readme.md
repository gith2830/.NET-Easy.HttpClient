# Easy.HttpClient

### it's a simple RESTful interface request.NET package.

#### It is very simple to use. Declare an attribute tag of **[HttpClient]** on the interface. During compilation, all interfaces containing **[HttpClient]** will be scanned to generate their implementation classes. 

#### If the installed package still does not take effect, go to the project file (.csproj) and find 
```xml
<PackageReference Include="Easy.HttpClient" />
``` 
and add OutputItemType="Analyzer", such as 
```xml
<PackageReference Include="Easy.HttpClient" OutputItemType="Analyzer"/>
```

#### The methods defined in the interface are used to guide the generation of methods for implementing requests. You can use HTTP verb attribute tags to add tag declarations to methods <u>(only when tags such as **[Get]**, **[Post]**, **[Put]**, **[Delete]**, etc. are added will the code implementation part of the corresponding method be generated).</u> The parameters in the methods defined in the interface can also be added with attribute tags such as **[Body]**, **[Form]**, **[Header]**, **[Path]**, **[Query]**, etc. to declare which part of HTTP the parameters belong to. The return value of the methods defined in the interface can be declared as any type. If it is a reference type, it will be converted into the corresponding class object according to the returned json for return. **When the return value is declared as string, you can parse the returned result by yourself.**

# API Request Methods Definition

> ### 1. An example of a GET request is as follows:
``` csharp
[HttpClient("http://127.0.0.1/api")]
public interface TestClient
{
    [Get("/search/{keyword}")]
    public ApiResult Search([Path("keyword")]string keyword,[Query("page")]int page);
}
```
> ### 2. An example of a Post request is as follows:
``` csharp
[HttpClient("http://127.0.0.1/api")]
public interface TestClient
{
    [Post("Add")]
    public ApiResult AddNum([Form("count")]int count);
}
```
> ### 3. An example of a Put request is as follows:
``` csharp
[HttpClient("http://127.0.0.1/api")]
public interface TestClient
{
    [Put("/put/{id}")]
    public ApiResult Edit([Path("id")] Guid ida, [Body] int count);
}
```
> ### 4. An example of a Delete request is as follows:
``` csharp
[HttpClient("http://127.0.0.1/api")]
public interface TestClient
{
    [Delete("/delete/{id}")]
    public ApiResult Delete([Path] Guid id, [Query] int count, [Header("Authorization")]string token);
}
```
# Invocation of API request methods
> ### The format of the automatically generated code implementation class is the interface name suffixed with **Impl**, such as **TestImpl**.
``` csharp
TestClient testClient = new TestClientImpl();
var res = testClient.Search("cat",3);
res = testClient.AddNum(1);
res = testClient.Edit(Guid.NewGuid(), 4);
string token = "Bearer xxxxxx";
res = testClient.Delete(Guid.NewGuid(), 5, token);
```
> ### If it is an ASP.NET WEB project, the defined client can be automatically registered through **AddEasyHttpClient()**.It is registered as a service with a scoped lifecycle.If you want to call the method **AddEasyHttpClient()**, you must first import the namespace **"Easy.HttpClient.Extensions"**.
``` csharp
[HttpClient("http://127.0.0.1/api")]
public interface EmployeeClient
{
    [Get("employee/{eid}")]
    public ApiResult GetEmployeeById([Path] Guid eid, [Header("Authorization")] string token);
}
```
``` csharp
using Easy.HttpClient.Extensions;//The namespace "Easy.HttpClient.Extensions" must be imported.
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEasyHttpClient();//automatically registered
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
```
``` csharp
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
        Guid id = Guid.NewGuid();
        string token = "Bearer xxxxxx";
        var result = _employeeClient.GetEmployeeById(id, token);
        return result;
    }
}
```
