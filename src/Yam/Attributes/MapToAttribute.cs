namespace Yam.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class MapToAttribute : Attribute
{
    public MapToAttribute(params Type[] types)
    {
        Types = types;
    }

    public readonly Type[] Types;
}
