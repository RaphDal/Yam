using ConsoleApp.Entities;
using Yam.Attributes;

namespace ConsoleApp.Dtos;

[MapTo(typeof(Address))]
[MapFrom(typeof(Address))]
public class AddressDto
{
    public string Street { get; set; }

    public string City { get; set; }

    public int PostalCode { get; set; }

    public string Country { get; set; }
}
