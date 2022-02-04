using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yam.Generator.Models;

internal class YamClass
{
    public YamClass(string name, string fullName, NameSyntax fullNameSyntax, ITypeSymbol symbol)
    {
        Name = name;
        FullName = fullName;
        FullNameSyntax = fullNameSyntax;
        Symbol = symbol;
        Properties = new Dictionary<string, YamProperty>();
        Targets = new HashSet<string>();
        Sources = new HashSet<string>();
    }

    public string Name { get; }

    public string FullName { get; }

    public NameSyntax FullNameSyntax { get; }

    public ITypeSymbol Symbol { get; }

    public IDictionary<string, YamProperty> Properties { get; }

    public HashSet<string> Targets { get; }

    public HashSet<string> Sources { get; }
}
