using Microsoft.CodeAnalysis;

namespace Yam.Generator;

internal class YamClass
{
    public YamClass(string name, string fullName, ITypeSymbol symbol)
    {
        Name = name;
        FullName = fullName;
        Symbol = symbol;
        Properties = new Dictionary<string, YamProperty>();
        Targets = new HashSet<string>();
        Sources = new HashSet<string>();
    }

    public string Name { get; }

    public string FullName { get; }

    public ITypeSymbol Symbol { get; }

    public IDictionary<string, YamProperty> Properties { get; }

    public HashSet<string> Targets { get; }

    public HashSet<string> Sources { get; }
}
