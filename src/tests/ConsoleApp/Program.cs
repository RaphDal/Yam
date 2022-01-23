using Yam;
using ConsoleApp.Entities;

var company = new Company()
{
    Id = 1,
    Name = "name"
};

var dto = company.ToCompanyDto();

Console.WriteLine($"Company id: {dto.Id}");
Console.WriteLine($"Company name: {dto.Name}");
