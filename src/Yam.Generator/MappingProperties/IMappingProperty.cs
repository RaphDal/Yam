using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yam.Generator.MappingProperties;

internal interface IMappingProperty
{
    string Source { get; }

    string Target { get; }

    ExpressionSyntax ToExpressionSyntax(MemberAccessExpressionSyntax memberAccessExpressionSyntax);
}
