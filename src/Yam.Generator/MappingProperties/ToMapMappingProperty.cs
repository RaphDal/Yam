using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yam.Generator.Core;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yam.Generator.MappingProperties;

internal class ToMapMappingProperty : IMappingProperty
{
    public ToMapMappingProperty(string source, string target, string type)
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
                memberAccessExpressionSyntax,
                IdentifierName(SourceGenerator.GenerateMethodName(Type))
            )
        );
    }
}
