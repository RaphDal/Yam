using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yam.Generator.MappingProperties;

internal class DefaultMappingProperty : IMappingProperty
{
    public DefaultMappingProperty(string source, string target)
    {
        Source = source;
        Target = target;
    }

    public string Source { get; }

    public string Target { get; }

    public ExpressionSyntax ToExpressionSyntax(MemberAccessExpressionSyntax memberAccessExpressionSyntax)
        => memberAccessExpressionSyntax;
}
