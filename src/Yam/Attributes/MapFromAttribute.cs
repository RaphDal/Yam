namespace Yam.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class MapFromAttribute : Attribute
{
    public MapFromAttribute(params Type[] types)
    {
        Types = types;
    }

    public readonly Type[] Types;
}
