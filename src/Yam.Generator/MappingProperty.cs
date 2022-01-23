namespace Yam.Generator;

internal class MappingProperty
{
    public MappingProperty(string source, string target, string? sourcePrefix = null, string? sourceSuffix = null)
    {
        Source = source;
        Target = target;
        SourcePrefix = sourcePrefix;
        SourceSuffix = sourceSuffix;
    }

    public string Source { get; }

    public string Target { get; }

    public string? SourcePrefix { get; }

    public string? SourceSuffix { get; }
}
