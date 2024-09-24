using Easy.HttpClient.Attributes;
using Easy.HttpClient.Attributes.Params;
using Easy.HttpClient.Receivers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;

namespace Easy.HttpClient.Generators
{
    [Generator]
    public class RestHttpGenerator : ISourceGenerator
    {
        private const string _httpAttrBase = "Easy.HttpClient.Attributes";
        private const string CLASS_SUFFIX = "Impl";

        private string ConvertParamsStr(ImmutableArray<IParameterSymbol> args)
        {
            StringBuilder builder = new StringBuilder();
            for(int i = 0;i<args.Length-1;++i)
            {
                var item = args[i];
                builder.Append($"{item.Type.Name} {item.Name},");
            }
            var last = args[args.Length-1];
            builder.Append($"{last.Type.Name} {last.Name}");
            return builder.ToString();
        }
        private ParamAttribute FindParamAttr(IEnumerable<AttributeData> attributeDatas)
        {
            var formAttr = attributeDatas.FirstOrDefault(x=>x.AttributeClass.Name == typeof(FormAttribute).Name);
            if(formAttr != null)
            {
                return Map2Type<FormAttribute>(formAttr);
            }
            var pathAttr = attributeDatas.FirstOrDefault(x => x.AttributeClass.Name == typeof(PathAttribute).Name);
            if (pathAttr != null)
            {
                return Map2Type<PathAttribute>(pathAttr);
            }
            var bodyAttr = attributeDatas.FirstOrDefault(x => x.AttributeClass.Name == typeof(BodyAttribute).Name);
            if (bodyAttr != null)
            {
                return Map2Type<BodyAttribute>(bodyAttr);
            }
            var queryAttr = attributeDatas.FirstOrDefault(x => x.AttributeClass.Name == typeof(QueryAttribute).Name);
            if (queryAttr != null)
            {
                return Map2Type<QueryAttribute>(queryAttr);
            }
            var headerAttr = attributeDatas.FirstOrDefault(x => x.AttributeClass.Name == typeof(HeaderAttribute).Name);
            if (headerAttr != null)
            {
                return Map2Type<HeaderAttribute>(headerAttr);
            }
            return null;
        }
        public ParamAttribute GetForGetMethodAttr(ParamAttribute paramAttr)
        {
            if(paramAttr == null)
            {
                return new QueryAttribute(null);
            }
            switch(paramAttr.ParamType)
            {
                case ParamType.Query:
                case ParamType.Path:
                case ParamType.Header:
                    return paramAttr;
                default:
                    return new QueryAttribute(null);
            }
        }
        public ParamAttribute GetForOtherMethodAttr(ParamAttribute paramAttr)
        {
            if (paramAttr == null)
            {
                return new BodyAttribute(null);
            }
            switch (paramAttr.ParamType)
            {
                case ParamType.Form:
                case ParamType.Path:
                case ParamType.Body:
                case ParamType.Header:
                    return paramAttr;
                default:
                    return new BodyAttribute(null);
            }
        }
        public ParamAttribute FindParamAttrByHttpMethod(string httpMethod, IEnumerable<AttributeData> attributeDatas)
        {
            ParamAttribute paramAttr = FindParamAttr(attributeDatas);
            switch (httpMethod)
            {
                case "Get":
                    paramAttr = GetForGetMethodAttr(paramAttr);
                    break;
                default:
                    paramAttr = GetForOtherMethodAttr(paramAttr);
                    break;
            }
            return paramAttr;
        }
        private string GetSendByHttpMethod(string httpMethod,string dataName, bool canCancel)
        {
            StringBuilder builder = new StringBuilder($"            var response = httpClient.{httpMethod}Async(url");
            switch (httpMethod)//添加参数
            {
                case "Get":
                case "Delete":
                    //不需要添加参数
                    break;
                case "Post":
                case "Put":
                    builder.Append($", {dataName}");
                    break;
            }
            if (canCancel)
            {
                builder.Append(", _easy_rest_cancel_token_v1_00).GetAwaiter().GetResult();");
            }
            else
            {
                builder.Append(").GetAwaiter().GetResult();");
            }
            
            return builder.ToString();
        }
        private string GenerateSetParamCode(IParameterSymbol arg,out string dataArgName)
        {
            StringBuilder builder = new StringBuilder();
            if (arg.Type.IsReferenceType && arg.Type.Name != "String")
            {
                dataArgName = $"{arg.Name}Json";
                builder.AppendLine($"            var {dataArgName} = JsonConvert.SerializeObject({arg.Name});");
            }
            else
            {
                dataArgName = $"{arg.Name}Str";
                builder.AppendLine($"            var {dataArgName} = {arg.Name}.ToString();");
            }
            return builder.ToString();
        }
        private string GetMethodParamName(IParameterSymbol arg, ParamAttribute paramAttribute)
        {
            if(paramAttribute == null || string.IsNullOrEmpty(paramAttribute.Name))
            {
                return arg.Name;
            }
            return paramAttribute.Name;
        }
        private string AddSendCode(string httpMethod,string url,ImmutableArray<IParameterSymbol> args, bool canCancel)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"            string url = \"{url}\";");
            //builder.AppendLine("            var formDataContent = new MultipartFormDataContent();");
            //builder.Append("formDataContent.Add(\"ContentType\", \"multipart/form-data\");");
            builder.AppendLine("            dynamic data = new ExpandoObject();");
            bool hasForm = false;
            bool hasBody = false;
            bool hasQuery = false;
            bool hasPath = false;
            StringBuilder queryBuilder = new StringBuilder("            string queryParams = $\"");
            StringBuilder pathBuilder = new StringBuilder();
            StringBuilder formBuilder = new StringBuilder();
            formBuilder.AppendLine("            var formDataList = new List<KeyValuePair<string, string>>();");
            foreach (var arg in args)
            {
                var attrs = arg.GetAttributes();
                ParamAttribute paramAttribute = FindParamAttrByHttpMethod(httpMethod, attrs);
                string dataArgName;
                string paramName = GetMethodParamName(arg,paramAttribute);
                builder.Append(GenerateSetParamCode(arg, out dataArgName));
                switch (paramAttribute.ParamType)
                {
                    case ParamType.Form:
                        hasForm = true;
                        //builder.AppendLine($"            formDataContent.Add(new StringContent(\"{paramName}\"), {dataArgName}.ToString());");
                        formBuilder.AppendLine($"            formDataList.Add(new KeyValuePair<string, string>(\"{paramName}\", {dataArgName}));");
                        break;
                    case ParamType.Body:
                        hasBody = true;
                        builder.AppendLine($"            data.{paramName} = {dataArgName};");
                        break;
                    case ParamType.Query:
                        hasQuery = true;
                        queryBuilder.Append($"{paramName}={{{dataArgName}}}&");
                        break;
                    case ParamType.Path:
                        hasPath = true;
                        pathBuilder.AppendLine($"            url = url.Replace(\"{{{paramName}}}\",{dataArgName});");
                        break;
                    case ParamType.Header:
                        builder.AppendLine($"            httpClient.DefaultRequestHeaders.Add(\"{paramName}\", {dataArgName});");
                        break;
                }
            }
            string sendParamName;
            if ( hasForm && hasBody || hasBody)
            {
                builder.AppendLine("            var json = JsonConvert.SerializeObject(data);");
                builder.AppendLine("            using StringContent stringContent = new StringContent(json,Encoding.UTF8,\"application/json\");");
                //builder.AppendLine("            stringContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(\"application/json\");");
                sendParamName = "stringContent";
            }
            //else if (hasForm)
            else
            {
                formBuilder.AppendLine("            var formDataContent = new FormUrlEncodedContent(formDataList);");
                builder.Append(formBuilder.ToString());
                sendParamName = "formDataContent";
            }
            if (hasQuery)
            {
                queryBuilder.AppendLine("\";");
                builder.Append(queryBuilder.ToString());
                builder.AppendLine($"            url = $\"{{url}}?{{queryParams}}\";");
            }
            if (hasPath)
            {
                builder.Append(pathBuilder.ToString());
            }
            string sendCode = GetSendByHttpMethod(httpMethod, sendParamName, canCancel);
            builder.AppendLine(sendCode);
            return builder.ToString();
        }
        private void GenerateReturnTypeMethod(StringBuilder builder, string methodName, ImmutableArray<IParameterSymbol> args, HttpMethodAttribute restMehtodAttribute, string url,ITypeSymbol returnTypeSymbol)
        {
            string methodStr = restMehtodAttribute.Method;
            var returnType = returnTypeSymbol.ToDisplayString();
            bool isReturnVoid = returnTypeSymbol == null || returnTypeSymbol.SpecialType == SpecialType.System_Void;
            if (isReturnVoid)//如果没有返回值则替换成void
            {
                returnType = "void";
            }
            var argsStr = ConvertParamsStr(args);
            builder.AppendLine($"        public {returnType} {methodName}({argsStr})");
            builder.AppendLine("        {");
            if (restMehtodAttribute.CanCancel)
            {
                builder.AppendLine($"            CancellationTokenSource tokenSource = new CancellationTokenSource();");
                builder.AppendLine($"            tokenSource.CancelAfter({restMehtodAttribute.Timeout});");
                builder.AppendLine($"            CancellationToken _easy_rest_cancel_token_v1_00 = tokenSource.Token;");
            }
            builder.AppendLine("            using HttpClient httpClient = new HttpClient();");            
            if (args.Length > 0)
            {
                builder.Append(AddSendCode(methodStr, url, args, restMehtodAttribute.CanCancel));
            }
            if (isReturnVoid)//如果是void返回值则不需要读取内容
            {
                builder.AppendLine("            return;");
            }
            else
            {
                builder.AppendLine("            var resultStr = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();");
                if (returnType != "string")
                {
                    builder.AppendLine($"           return JsonConvert.DeserializeObject<{returnType}>(resultStr);");
                }
                else
                {
                    builder.AppendLine("            return resultStr;");
                }
            }
            builder.AppendLine("       }");
        }
        private string ConcatUrl(string apiUrl,params string[] urls)
        {
            const string httpPrefix = "http://";
            const string httpsPrefix = "https://";
            string str;
            bool isHttp = apiUrl.StartsWith(httpPrefix);
            bool isHttps = apiUrl.StartsWith(httpsPrefix);
            string urlsStr = string.Join("/", urls);
            if (!isHttp && !isHttps)
            {
                str = string.Concat(httpPrefix,$"{apiUrl}/", urlsStr).Trim();
                isHttp = true;
            }
            else
            {
                str = string.Concat($"{apiUrl}/", urlsStr).Trim();
            }
            string prefixStr;
            if (isHttp)
            {
                prefixStr = str.Substring(0, httpPrefix.Length);
            }
            else
            {
                prefixStr = str.Substring(0, httpsPrefix.Length);
            }
            
            string afterUrl = str.Substring(httpsPrefix.Length-1).Replace("//","/");
            return string.Concat(prefixStr, afterUrl);
        }
        private void GenerateClassMethod(StringBuilder builder,IMethodSymbol methodSymbol,HttpMethodAttribute restMehtodAttribute, HttpClientAttribute httpClientAttr)
        {
            var returnTypeSymbol = methodSymbol.ReturnType;
            string methodNameStr = methodSymbol.Name;
            string url = restMehtodAttribute.Template;
            var args = methodSymbol.Parameters;
            string apiUrl = ConcatUrl(httpClientAttr.ApiUrl, url);
            GenerateReturnTypeMethod(builder, methodNameStr, args, restMehtodAttribute, apiUrl, returnTypeSymbol);
        }

        private static IEnumerable<object> GetActualConstuctorParams(AttributeData attributeData)
        {
            foreach (var arg in attributeData.ConstructorArguments)
            {
                if (arg.Kind == TypedConstantKind.Array)
                {
                    yield return arg.Values.Select(a => a.Value).OfType<string>().ToArray();
                }
                else
                {
                    yield return arg.Value;
                }
            }
        }
        private T Map2Type<T>(AttributeData attributeData) where T : Attribute
        {
            T attribute;
            if (attributeData.AttributeConstructor != null && attributeData.ConstructorArguments.Length > 0)
            {
                attribute = (T)Activator.CreateInstance(typeof(T), GetActualConstuctorParams(attributeData).ToArray());
            }
            else
            {
                attribute = (T)Activator.CreateInstance(typeof(T));
            }
            foreach (var p in attributeData.NamedArguments)
            {
                var type = typeof(T);
                var field = type.GetProperty(p.Key);
                field.SetValue(attribute, p.Value.Value);
            }
            return attribute;
        }
        private HttpMethodAttribute GetHttpMethod(IMethodSymbol methodSymbol)
        {
            var attrs = methodSymbol.GetAttributes();
            var httpGetAttr = attrs.FirstOrDefault(x => x.AttributeClass.ToDisplayString() == $"{_httpAttrBase}.GetAttribute");
            if (httpGetAttr != null)
            {
                var restAttr = Map2Type<GetAttribute>(httpGetAttr);
                return restAttr;
            }
            var httpPostAttr = attrs.FirstOrDefault(x => x.AttributeClass.ToDisplayString() == $"{_httpAttrBase}.PostAttribute");
            if (httpPostAttr != null)
            {
                var restAttr = Map2Type<PostAttribute>(httpPostAttr);
                return restAttr;
            }
            var httpPutAttr = attrs.FirstOrDefault(x => x.AttributeClass.ToDisplayString() == $"{_httpAttrBase}.PutAttribute");
            if (httpPutAttr != null)
            {
                var restAttr = Map2Type<PutAttribute>(httpPutAttr);
                return restAttr;
            }
            var httpDeleteAttr = attrs.FirstOrDefault(x => x.AttributeClass.ToDisplayString() == $"{_httpAttrBase}.DeleteAttribute");
            if (httpDeleteAttr != null)
            {
                var restAttr = Map2Type<DeleteAttribute>(httpDeleteAttr);
                return restAttr;
            }
            return null;
        }
        private void ForEachMethod(StringBuilder builder,GeneratorExecutionContext context, InterfaceDeclarationSyntax interfaceDeclaration, string attrName, HttpClientAttribute httpClientAttr)
        {
            var methodSyntaxs = interfaceDeclaration.Members.OfType<MethodDeclarationSyntax>();
            foreach(var methodSyntax in methodSyntaxs)
            {
                var methodSymbol = context.Compilation.GetSemanticModel(methodSyntax.SyntaxTree).GetDeclaredSymbol(methodSyntax) as IMethodSymbol;
                if(methodSymbol == null)
                {
                    continue;
                }
                var httpAttr = GetHttpMethod(methodSymbol);
                if(httpAttr == null)
                {
                    continue;
                }
                GenerateClassMethod(builder,methodSymbol, httpAttr, httpClientAttr);
            }
        }
        private void GenerateClassHeader(StringBuilder builder,string namespaceStr,string interfaceName)
        {
            builder.AppendLine("using System;");
            builder.AppendLine("using System.Threading;");
            builder.AppendLine("using Newtonsoft.Json;");
            builder.AppendLine("using System.Net.Http;");
            builder.AppendLine("using System.Dynamic;");
            builder.AppendLine("using System.Text;");
            builder.AppendLine($"namespace {namespaceStr}");
            builder.AppendLine("{");
            builder.AppendLine($"   public class {interfaceName}Impl : {interfaceName}");
            builder.AppendLine("    {");
        }
        private void GenerateClassFooter(StringBuilder builder)
        {
            builder.AppendLine("    }");
            builder.AppendLine("}");
        }
        private Dictionary<string, string> ForEachInterface(GeneratorExecutionContext context, IEnumerable<InterfaceDeclarationSyntax> interfaceSyntaxs,string attrName)
        {
            Dictionary<string, string> interfaceDict = new Dictionary<string, string>();
            foreach (var interfaceDeclaration in interfaceSyntaxs)
            {
                var interfaceName = interfaceDeclaration.Identifier.ValueText;
                var httpClientAttrSyntax = interfaceDeclaration.AttributeLists.SelectMany(x => x.Attributes).FirstOrDefault(attr=>$"{attr.Name.ToFullString()}Attribute" == attrName);
                if(httpClientAttrSyntax == null)
                {
                    continue;
                }
                var interfaceSymbol = context.Compilation.GetSemanticModel(interfaceDeclaration.SyntaxTree).GetDeclaredSymbol(interfaceDeclaration);
                var namespaceStr = interfaceSymbol.ContainingNamespace.ToDisplayString();
                var attrDatas = interfaceSymbol.GetAttributes();
                var attrData = attrDatas.FirstOrDefault(x => x.AttributeClass.Name == attrName);
                var httpClientAttr = Map2Type<HttpClientAttribute>(attrData);
                StringBuilder builder = new StringBuilder();
                GenerateClassHeader(builder, namespaceStr, interfaceName);
                ForEachMethod(builder,context, interfaceDeclaration, attrName, httpClientAttr);
                GenerateClassFooter(builder);
                context.AddSource($"{interfaceName}{CLASS_SUFFIX}.g.cs", builder.ToString());
                interfaceDict.Add(interfaceName, namespaceStr);
            }
            return interfaceDict;
        }

        private void GenerateInjectionExtensions(GeneratorExecutionContext context, Dictionary<string, string> interfaceDict)
        {
            string className = "EasyHttpClientInjectionExtensions";
            StringBuilder fileBuilder = new StringBuilder();
            StringBuilder injectBuilder = new StringBuilder();
            List<string> injectNamespaces = new List<string>();
            fileBuilder.AppendLine("using Microsoft.Extensions.DependencyInjection;");
            fileBuilder.AppendLine("using System;");
            foreach (var kv in interfaceDict)
            {
                string namespaceStr = kv.Value;
                string interfaceStr = kv.Key;
                injectNamespaces.Add(namespaceStr);
                injectBuilder.AppendLine($"                services.AddScoped<{interfaceStr},{interfaceStr}{CLASS_SUFFIX}>();");
            }
            //去重命名空间
            var appendNamespaces = injectNamespaces.Distinct().ToList();
            foreach (var item in appendNamespaces)
            {
                fileBuilder.AppendLine($"using {item};");
            }
            fileBuilder.AppendLine($"namespace Easy.HttpClient.Extensions");
            fileBuilder.AppendLine("{");
            fileBuilder.AppendLine($"    public static class {className}");
            fileBuilder.AppendLine($"    {{");
            fileBuilder.AppendLine($"        public static void AddEasyHttpClient(this IServiceCollection services)");
            fileBuilder.AppendLine($"        {{");
            fileBuilder.Append(injectBuilder.ToString());
            fileBuilder.AppendLine($"        }}");
            fileBuilder.AppendLine($"    }}");
            fileBuilder.AppendLine("}");
            context.AddSource($"{className}.g.cs", fileBuilder.ToString());
            //Console.WriteLine($"{className}.g.cs");
        }
        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxContextReceiver is HttpClientSyntaxReceiver receiver))
            {
                return;
            }
            var httpClentName = typeof(HttpClientAttribute).Name;
            //从语法树中查找出含有HttpClientAttribute的接口
            var interfaceSyntaxs = context.Compilation.SyntaxTrees.SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<InterfaceDeclarationSyntax>());
            interfaceSyntaxs = interfaceSyntaxs.Where(interfaceeclaration => {
                return interfaceeclaration.AttributeLists.Any(list =>
                    list.Attributes.Any(attr =>
                        $"{attr.Name.ToFullString()}Attribute" == httpClentName
                    )
                );
            });
            var interfaceDict = ForEachInterface(context,interfaceSyntaxs, httpClentName);
            GenerateInjectionExtensions(context,interfaceDict);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new HttpClientSyntaxReceiver());
            //Debugger.Launch();
        }
    }
}
