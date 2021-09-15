using System;
using System.Collections.Generic;

namespace romaklayt.DynamicFilter.Test.Api.NetFramework.Models
{
    public class User
    {
        private User()
        {
        }

        public User(string name, int age, Address address)
        {
            Name = name;
            Age = age;
            Address = address;
        }

        public string Name { get; set; }
        public int Age { get; set; }
        public Address Address { get; set; }
        public List<Role> Roles { get; set; }

        public DateTime BirthDate { get; set; } = DateTime.Now.AddMonths(-new Random().Next(0, 20));
    }
}