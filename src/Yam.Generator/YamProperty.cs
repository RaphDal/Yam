namespace Yam.Generator;

internal class YamProperty
{
    public YamProperty(string name, string type, bool isNative, bool get, bool set)
    {
        Name = name;
        Type = type;
        IsNative = isNative;
        Get = get;
        Set = set;
    }

    public string Name { get; }

    public string Type { get; }

    public bool IsNative { get; }

    public bool Get { get; }

    public bool Set { get; }
}