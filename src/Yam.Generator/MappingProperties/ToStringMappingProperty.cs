using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yam.Generator.MappingProperties;

internal class ToStringMappingProperty : IMappingProperty
{
    private readonly static IdentifierNameSyntax ToStringIdentifierName = IdentifierName("ToString");

    public ToStringMappingProperty(string source, string target)
    {
        Source = source;
        Target = target;
    }

    public string Source { get; }

    public string Target { get; }

    public ExpressionSyntax ToExpressionSyntax(MemberAccessExpressionSyntax memberAccessExpressionSyntax)
    {
        return InvocationExpression(
            MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                memberAccessExpressionSyntax,
                ToStringIdentifierName
            )
        );
    }
}
