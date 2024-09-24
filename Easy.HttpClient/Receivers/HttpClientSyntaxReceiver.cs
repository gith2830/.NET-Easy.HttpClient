using Easy.HttpClient.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Easy.HttpClient.Receivers
{
    internal class HttpClientSyntaxReceiver : ISyntaxContextReceiver
    {
        public List<INamedTypeSymbol> TypeSymbols { get; set; } = new List<INamedTypeSymbol>();
        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is InterfaceDeclarationSyntax ids && ids.AttributeLists.Count > 0)
            {
                var typeSymbol = ModelExtensions.GetDeclaredSymbol(context.SemanticModel, ids) as INamedTypeSymbol;
                var namespaceStr = typeof(HttpClientAttribute).Namespace;
                if (typeSymbol.GetAttributes().Any(x =>
                        x.AttributeClass.ToDisplayString() ==
                        namespaceStr))
                {
                    TypeSymbols.Add(typeSymbol);
                }
            }
        }
    }
}
