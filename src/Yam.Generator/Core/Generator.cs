using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;
using Yam.Generator.Constants;
using Yam.Generator.Models;

namespace Yam.Generator.Core;

[Generator(LanguageNames.CSharp)]
public class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<YamClass> classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null)!;

        IncrementalValueProvider<(Compilation, ImmutableArray<YamClass>)> compilationAndClasses
            = context.CompilationProvider.Combine(classDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndClasses, static (spc, source) => Execute(source.Item1, source.Item2, spc));
    }

    static bool IsSyntaxTargetForGeneration(SyntaxNode node)
        => node is ClassDeclarationSyntax m && m.AttributeLists.Count > 0;

    static YamClass? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

        foreach (AttributeListSyntax attributeLists in classDeclarationSyntax.AttributeLists)
        {
            foreach (AttributeSyntax attribute in attributeLists.Attributes)
            {
                if (context.SemanticModel.GetSymbolInfo(attribute).Symbol is not IMethodSymbol attributeSymbol)
                {
                    continue;
                }

                INamedTypeSymbol typeSymbol = attributeSymbol.ContainingType;
                string fullName = typeSymbol.ToDisplayString();

                if (fullName == AttributeNames.MapFrom || fullName == AttributeNames.MapTo ||
                    fullName == AttributeNames.Map)
                {
                    return YamGenerator.GenerateYamClass(context, classDeclarationSyntax);
                }
            }
        }

        return null;
    }

    static void Execute(Compilation _, ImmutableArray<YamClass> classes, SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty)
        {
            return;
        }

        var entities = classes.ToDictionary(c => c.FullName);

        var mappings = MapperGenerator.GenerateMappings(entities);

        var source = SourceGenerator.GenerateSource(mappings);
        var strSource = source.NormalizeWhitespace().ToFullString();

        context.AddSource("Mappings.g.cs", SourceText.From(strSource, Encoding.UTF8));
    }
}
