using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yam.Generator.Models;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yam.Generator.Core;

internal static class SourceGenerator
{
    private static readonly string ClassName = "Mappings";
    private static readonly IdentifierNameSyntax NamespaceIdentifierName = IdentifierName("Yam");

    /// <summary>
    /// Generate the CompilationUnitSyntax from the list of mapping.
    /// </summary>
    internal static CompilationUnitSyntax GenerateSource(IEnumerable<Mapping> mappings)
    {

        var methods = mappings.Select(mapping => GenerateMappingSource(mapping)).ToList();

        var compilationUnit = CompilationUnit()
        .AddMembers(
            FileScopedNamespaceDeclaration(NamespaceIdentifierName)
            .WithMembers(
                SingletonList<MemberDeclarationSyntax>(
                    ClassDeclaration(ClassName)
                    .WithModifiers(
                        TokenList(new[]
                        {
                            Token(SyntaxKind.PublicKeyword),
                            Token(SyntaxKind.StaticKeyword),
                        })
                    )
                    .WithMembers(List(methods))
                )
            )
        );

        return compilationUnit;
    }

    private static readonly IdentifierNameSyntax ValueSyntaxToken = IdentifierName("value");
    public static string GenerateMethodName(string targetName) => $"To{targetName}";

    /// <summary>
    /// Create a new MethodDeclarationSyntax from the mapping.
    /// </summary>
    static MemberDeclarationSyntax GenerateMappingSource(Mapping map)
    {
        var propertiesAssignements = new SeparatedSyntaxList<ExpressionSyntax>();
        foreach (var property in map.Properties)
        {
            var access = MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                ValueSyntaxToken,
                IdentifierName(property.Source)
            );

            propertiesAssignements = propertiesAssignements.Add(AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName(property.Target),
                    property.ToExpressionSyntax(access)
                )
            );
        } 

        var methodSyntax = MethodDeclaration(map.Target.FullNameSyntax, GenerateMethodName(map.Target.Name))
        .WithParameterList(
            ParameterList(
                SingletonSeparatedList(
                    Parameter(ValueSyntaxToken.Identifier)
                    .WithType(map.Source.FullNameSyntax)
                    .WithModifiers(
                        TokenList(Token(SyntaxKind.ThisKeyword))
                    )
                )
            )
        )
        .WithModifiers(
            TokenList(new[]
            {
                Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.StaticKeyword),
            })
        )
        .WithBody(
            Block(
                ReturnStatement(
                    ObjectCreationExpression(map.Target.FullNameSyntax)
                    .WithInitializer(
                        InitializerExpression(
                            SyntaxKind.ObjectInitializerExpression,
                            propertiesAssignements
                        )
                    )
                )
            )
        );

        return methodSyntax;
    }
}
