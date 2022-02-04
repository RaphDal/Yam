using Yam.Generator.MappingProperties;
using Yam.Generator.Models;

namespace Yam.Generator.Core;

internal static class MapperGenerator
{
    internal static IEnumerable<(string, string)> GetMappingList(IDictionary<string, YamClass> entities)
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

    internal static List<Mapping> GenerateMappings(IDictionary<string, YamClass> entities)
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

    internal static Mapping GetMapping(IDictionary<string, YamClass> entities, YamClass source, YamClass target)
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

    internal static IMappingProperty? GetMappingProperty(IDictionary<string, YamClass> entities, YamProperty source, YamProperty target)
    {
        if (source.Type == target.Type)
        {
            return new DefaultMappingProperty(source.Name, target.Name);
        }

        if (target.Type == StringFullName)
        {
            return new ToStringMappingProperty(source.Name, target.Name);
        }

        if (target.IsNative && source.Type == StringFullName)
        {
            return new ParseMappingProperty(source.Name, target.Name, target.Type);
        }

        if (entities.TryGetValue(source.Type, out var sourceEntity) && sourceEntity.Targets.Contains(target.Type) &&
            entities.TryGetValue(target.Type, out var targetEntity))
        {
            return new ToMapMappingProperty(source.Name, target.Name, targetEntity.Name);
        }

        if (entities.TryGetValue(target.Type, out var targetEntity2) && targetEntity2.Sources.Contains(source.Type) &&
            entities.TryGetValue(source.Type, out var _))
        {
            return new ToMapMappingProperty(source.Name, target.Name, targetEntity2.Name);
        }

        return null;
    }
}
