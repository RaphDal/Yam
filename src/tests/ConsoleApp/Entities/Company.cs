using Yam.Attributes;

namespace ConsoleApp.Entities;

[Map]
public class Company
{
    public int Id { get; set; }

    public string Name { get; set; }

    public Address Address { get; set; }
}
