using System;
using System.Collections.Generic;

namespace romaklayt.DynamicFilter.Test.Api.Models;

public class Role : BaseEntityAudit<Guid>
{
    public Role()
    {
    }

    public Role(string name, Address address)
    {
        Name = name;
        Addresses.Add(address);
    }

    public string Name { get; set; }
    public List<Address> Addresses { get; set; } = [];
}