﻿using System.Collections.Generic;
using romaklayt.DynamicFilter.Test.Api.Models;

namespace romaklayt.DynamicFilter.Test.Api;

public class Data
{
    public static readonly List<Address> Addresses =
    [
        new("street:1", 23, new Zip(123456, "USA")),
        new("street:2", null, new Zip(1234567, "BR")),
        new("street:3", 43, new Zip(54375445, "BR")),
        new("street:4", 53, new Zip(76878979, "PT")),
        new("street:5", 63, new Zip(65756443, "PT"))
    ];

    public static readonly List<User> Users =
    [
        new("Albert", "Mars", 27, Addresses[4]) { Roles = new List<Role> { new("Admin", Addresses[1]) } },
        new("Albert", "Ai", 37, Addresses[0]) { Roles = new List<Role> { new("Read", Addresses[2]), new("Write", Addresses[1]) } },
        new("Lucao", "Ya", 23, Addresses[4]) { Roles = new List<Role> { new("Read", Addresses[3]), new("Write", Addresses[2]) } },
        new("Luide", "Op", 28, Addresses[1]) { Roles = new List<Role> { new("Read", Addresses[4]), new("Write", Addresses[3]) } }
    ];
}