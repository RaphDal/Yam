using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yam.Attributes;
using Yam.Generator.Extensions;
using Yam.Generator.Helpers;
using Yam.Generator.Models;

namespace Yam.Generator.Core;

internal static class YamGenerator
{
    internal static YamClass? GenerateYamClass(GeneratorSyntaxContext context, ClassDeclarationSyntax declarationSyntax)
    {
        if (context.SemanticModel.GetDeclaredSymbol(declarationSyntax) is not ITypeSymbol symbol)
        {
            return null;
        }

        var name = symbol.Name;
        var (fullNameSyntax, fullName) = SymbolHelper.GetFullNameSyntax(symbol);

        var yam = new YamClass(name, fullName, fullNameSyntax, symbol);

        foreach (var attribute in symbol.GetAttributes())
        {
            if (attribute is { AttributeClass.Name: nameof(MapToAttribute) })
            {
                yam.Targets.UnionWith(AttributeHelper.ExtractAttributeTypes(attribute));
            }

            if (attribute is { AttributeClass.Name: nameof(MapFromAttribute) })
            {
                yam.Sources.UnionWith(AttributeHelper.ExtractAttributeTypes(attribute));
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

    internal static YamProperty? GenerateYamProperty(GeneratorSyntaxContext context, MemberDeclarationSyntax member)
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
}
