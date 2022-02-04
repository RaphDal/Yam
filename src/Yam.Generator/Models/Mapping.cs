using Yam.Generator.MappingProperties;

namespace Yam.Generator.Models;

internal class Mapping
{
    public Mapping(YamClass source, YamClass target)
    {
        Source = source;
        Target = target;
        Properties = new List<IMappingProperty>();
    }

    public readonly YamClass Source;

    public readonly YamClass Target;

    public readonly List<IMappingProperty> Properties;
}
