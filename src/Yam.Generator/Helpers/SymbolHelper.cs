using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yam.Generator.Helpers;

internal static class SymbolHelper
{
    public static (NameSyntax, string) GetFullNameSyntax(ITypeSymbol symbol)
    {
        var names = new Stack<string>();

        names.Push(symbol.Name);

        var namespaceSymbol = symbol.ContainingNamespace;

        while (namespaceSymbol != null)
        {
            if (namespaceSymbol.IsGlobalNamespace) break;

            names.Push(namespaceSymbol.Name);

            namespaceSymbol = namespaceSymbol.ContainingNamespace;
        }

        var name = string.Join(".", names);
        NameSyntax nameSyntax = IdentifierName(names.Pop());

        while (names.Count > 0)
        {
            nameSyntax = QualifiedName(nameSyntax, IdentifierName(names.Pop()));
        }

        return (nameSyntax, name);
    }
}
