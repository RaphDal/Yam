using Yam.Attributes;
using ConsoleApp.Entities;

namespace ConsoleApp.Dtos;

[MapTo(typeof(Company))]
[MapFrom(typeof(Company))]
public class CompanyDto
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
}
