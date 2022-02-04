using Yam.Attributes;

namespace ConsoleApp.Entities;

[Map]
public class Address
{
    public int Id { get; set; }

    public string Street { get; set; }

    public string City { get; set; }

    public int PostalCode { get; set; }

    public string Country { get; set; }
}
