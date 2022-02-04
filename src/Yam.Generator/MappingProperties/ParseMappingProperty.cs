using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yam.Generator.MappingProperties;

internal class ParseMappingProperty : IMappingProperty
{
    private readonly static IdentifierNameSyntax ParseIdentifierName = IdentifierName("Parse");

    public ParseMappingProperty(string source, string target, string type)
    {
        Source = source;
        Target = target;
        Type = type;
    }

    public string Source { get; }

    public string Target { get; }

    private string Type { get; }

    public ExpressionSyntax ToExpressionSyntax(MemberAccessExpressionSyntax memberAccessExpressionSyntax)
    {
        return InvocationExpression(
            MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName(Type),
                ParseIdentifierName
            )
        )
            .WithArgumentList(
                ArgumentList(
                    SingletonSeparatedList(Argument(memberAccessExpressionSyntax))
                )
            );
    }
}
