using System;

namespace romaklayt.DynamicFilter.Test.Api.Models;

public class Role
{
    public Role()
    {
    }

    public Role(string name, Address address)
    {
        Name = name;
        Address = address;
    }

    public Guid Id { get; set; } = new();
    public string Name { get; set; }
    public Address Address { get; set; }
    public DateTime? Valid { get; set; } = DateTime.Now;
}