using System;
using System.Collections.Generic;

namespace romaklayt.DynamicFilter.Test.Api.Models
{
    public class User
    {
        public User()
        {
        }

        public User(string name, int age, Address address)
        {
            Name = name;
            Age = age;
            Address = address;
        }

        public Guid Id { get; set; } = new();
        public string Name { get; set; }
        public int Age { get; set; }
        public Address Address { get; set; }
        public List<Role> Roles { get; set; }

        public DateTime BirthDate { get; set; } = DateTime.Now.AddMonths(-new Random().Next(0, 20));
    }

    public class UserViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public Address Address { get; set; }
        public List<Role> Roles { get; set; }

        public DateTime BirthDate { get; set; }
        public string Address_Zip_Country { get; set; }
    }
}