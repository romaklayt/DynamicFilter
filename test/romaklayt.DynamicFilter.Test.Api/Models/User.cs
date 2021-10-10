using System;
using System.Collections.Generic;

namespace romaklayt.DynamicFilter.Test.Api.Models
{
    public class User
    {
        public User()
        {
        }

        public User(string name, string lastName, int age, Address address)
        {
            Name = name;
            Age = age;
            Address = address;
            LastName = lastName;
        }

        public Guid Id { get; set; } = new();
        public string Name { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public Address Address { get; set; }
        public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

        public DateTime BirthDate { get; set; } = DateTime.Now.AddMonths(-new Random().Next(0, 20));
    }

    public class UserViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Lastname { get; set; }
        public int Age { get; set; }
        public Address Address { get; set; }
        public List<Role> Roles { get; set; }

        public DateTime BirthDate { get; set; }
    }
}