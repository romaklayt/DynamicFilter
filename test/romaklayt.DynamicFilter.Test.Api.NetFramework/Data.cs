using System.Collections.Generic;
using romaklayt.DynamicFilter.Test.Api.NetFramework.Models;

namespace romaklayt.DynamicFilter.Test.Api.NetFramework
{
    public class Data
    {
        public static readonly List<User> Users = new List<User>
        {
            new User("Bruno", 27, new Address("street 1", 23, new Zip(123456, "USA")))
            {
                Roles = new List<Role> { new Role(1, "Admin") }
            },
            new User("Fred", 33, new Address("street 2", null, new Zip(1234567, "BR")))
            {
                Roles = new List<Role> { new Role(2, "Admin") }
            },
            new User("Albert", 37, new Address("street 3", 43, new Zip(54375445, "BR")))
            {
                Roles = new List<Role> { new Role(null, "Read"), new Role(3, "Write") }
            },
            new User("Lucao", 23, new Address("street 4", 53, new Zip(76878979, "PT")))
            {
                Roles = new List<Role> { new Role(4, "Read"), new Role(5, "Write") }
            },
            new User("Luide", 28, new Address("street 5", 63, new Zip(65756443, "PT")))
            {
                Roles = new List<Role> { new Role(6, "Read"), new Role(7, "Write") }
            }
        };
    }
}