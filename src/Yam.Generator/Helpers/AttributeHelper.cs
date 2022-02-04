using Microsoft.CodeAnalysis;

namespace Yam.Generator.Helpers;

public static class AttributeHelper
{
    public static HashSet<string> ExtractAttributeTypes(AttributeData attribute)
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
}
