namespace Yam.Generator;

internal class Mapping
{
    public Mapping(YamClass source, YamClass target)
    {
        Source = source;
        Target = target;
        Properties = new List<MappingProperty>();
    }

    public readonly YamClass Source;

    public readonly YamClass Target;

    public readonly List<MappingProperty> Properties;
}
