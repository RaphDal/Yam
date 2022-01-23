using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;
using Yam.Attributes;
using Yam.Generator.Extensions;

namespace Yam.Generator;

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
                    return GenerateYamClass(context, classDeclarationSyntax);
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

        var entities = new Dictionary<string, YamClass>();
        foreach (var clazz in classes)
        {
            if (!entities.ContainsKey(clazz.FullName))
            {
                entities.Add(clazz.FullName, clazz);
            }
        }

        var mappings = GenerateMappings(entities);
        var source = GenerateSource(mappings);

        context.AddSource("Mappings.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    static YamClass? GenerateYamClass(GeneratorSyntaxContext context, ClassDeclarationSyntax declarationSyntax)
    {
        if (context.SemanticModel.GetDeclaredSymbol(declarationSyntax) is not ITypeSymbol symbol)
        {
            return null;
        }

        var name = symbol.Name;
        var fullName = symbol.ToDisplayString();

        var yam = new YamClass(name, fullName, symbol);

        foreach (var attribute in symbol.GetAttributes())
        {
            if (attribute is { AttributeClass.Name: nameof(MapToAttribute) })
            {
                yam.Targets.UnionWith(ExtractAttributeTypes(attribute));
            }

            if (attribute is { AttributeClass.Name: nameof(MapFromAttribute) })
            {
                yam.Sources.UnionWith(ExtractAttributeTypes(attribute));
            }
        }

        foreach (var member in declarationSyntax.Members)
        {
            var property = GenerateYamProperty(context, member);
            if (property is not null)
            {
                yam.Properties.Add(property.Name, property);
            }
        }

        return yam;
    }

    static HashSet<string> ExtractAttributeTypes(AttributeData attribute)
    {
        var destinations = new HashSet<string>();

        foreach (var argument in attribute.ConstructorArguments)
        {
            foreach (var value in argument.Values)
            {
                if (value is TypedConstant constant)
                {
                    var destination = constant.Value?.ToString();
                    if (destination != null)
                    {
                        destinations.Add(destination);
                    }
                }
            }
        }

        return destinations;
    }

    static YamProperty? GenerateYamProperty(GeneratorSyntaxContext context, MemberDeclarationSyntax member)
    {
        if (member is not PropertyDeclarationSyntax property ||
            context.SemanticModel.GetDeclaredSymbol(member) is not IPropertySymbol symbol)
        {
            return null;
        }

        if (!property.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)) || property.AccessorList == null)
        {
            return null;
        }

        var name = property.Identifier.ValueText;
        var type = symbol.Type.ToDisplayString();
        var get = property.AccessorList.Accessors.Any(accessor => accessor.Keyword.IsKind(SyntaxKind.GetKeyword));
        var set = property.AccessorList.Accessors.Any(accessor => accessor.Keyword.IsKind(SyntaxKind.SetKeyword));

        foreach (var attributeList in property.AttributeLists)
        {
            foreach (var attr in attributeList.Attributes)
            {
                var r = context.SemanticModel.GetSymbolInfo(attr);
            }
        }

        return new YamProperty(name, type, symbol.Type.IsNativeType(), get, set);
    }

    static IEnumerable<(string, string)> GetMappingList(IDictionary<string, YamClass> entities)
    {
        var maps = new HashSet<(string, string)>();

        foreach (var entity in entities.Values)
        {
            foreach (var source in entity.Sources)
            {
                maps.Add((source, entity.FullName));
            }

            foreach (var target in entity.Targets)
            {
                maps.Add((entity.FullName, target));
            }
        }

        return maps;
    }

    static List<Mapping> GenerateMappings(IDictionary<string, YamClass> entities)
    {
        var mappingList = GetMappingList(entities);
        var mappings = new List<Mapping>();

        foreach (var map in mappingList)
        {
            if (!entities.TryGetValue(map.Item1, out var source) ||
                !entities.TryGetValue(map.Item2, out var destination))
            {
                continue;
            }

            mappings.Add(GetMapping(entities, source, destination));
        }

        return mappings;
    }

    static Mapping GetMapping(IDictionary<string, YamClass> entities, YamClass source, YamClass target)
    {
        var mapping = new Mapping(source, target);

        foreach (var targetProperty in target.Properties.Values)
        {
            if (!source.Properties.TryGetValue(targetProperty.Name, out var sourceProperty))
            {
                // We cannot find a type to map this one
                continue;
            }

            var mappingProperty = GetMappingProperty(entities, sourceProperty, targetProperty);
            if (mappingProperty is null)
            {
                continue;
            }

            mapping.Properties.Add(mappingProperty);
        }

        return mapping;
    }

    private const string StringFullName = "System.String";

    static MappingProperty? GetMappingProperty(IDictionary<string, YamClass> entities, YamProperty source, YamProperty target)
    {
        if (source.Type == target.Type)
        {
            return new MappingProperty(source.Name, target.Name);
        }

        if (target.Type == StringFullName)
        {
            return new MappingProperty(source.Name, target.Name, sourceSuffix: ".ToString()");
        }

        if (target.IsNative && source.Type == StringFullName)
        {
            return new MappingProperty(source.Name, target.Name, sourcePrefix: $"{target.Type}.Parse(", sourceSuffix: ")");
        }

        if (entities.TryGetValue(source.Type, out var sourceEntity) && sourceEntity.Targets.Contains(target.Type) &&
            entities.TryGetValue(target.Type, out var targetEntity))
        {
            return new MappingProperty(source.Name, target.Name, sourceSuffix: $".To{targetEntity.Name}()");
        }

        return null;
    }

    static string GenerateSource(IEnumerable<Mapping> mappings)
    {
        var sb = new StringBuilder();

        sb.Append(
@"namespace Yam;

public static class Mappings
{");

        foreach (var mapping in mappings)
        {
            GenerateMappingSource(sb, mapping);
        }

        sb.Append(
@"}
");

        return sb.ToString();
    }

    static void GenerateMappingSource(StringBuilder sb, Mapping map)
    {
        sb.Append(@"
    public static ").Append(map.Target.FullName).Append(" To").Append(map.Target.Name).Append("(this ").Append(map.Source.FullName).Append(@" value)
    {
        return new ").Append(map.Target.FullName).Append(@"
        {");
        foreach (var property in map.Properties)
        {
            sb.Append(@"
            ").Append(property.Target).Append(" = ").Append(property.SourcePrefix)
                .Append("value.").Append(property.Source).Append(property.SourceSuffix).Append(',');
        }
        sb.Append(@"
        };
    }
");
    }
}
