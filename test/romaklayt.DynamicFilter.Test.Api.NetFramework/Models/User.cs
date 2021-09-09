using System;
using System.Collections.Generic;

namespace romaklayt.DynamicFilter.Test.Api.NetFramework.Models
{
    public class User
    {
        private DateTime _birthDate;

        private User()
        {
        }

        public User(string name, int age, Address address)
        {
            Name = name;
            Age = age;
            Address = address;
            Date = DateTime.Now;
        }

        public string Name { get; set; }
        public int Age { get; set; }
        public Address Address { get; set; }
        public List<Role> Roles { get; set; }
        private DateTimeOffset Date { get; }

        public DateTime BirthDate
        {
            get => DateTime.Now.AddYears(-Age);
            set => _birthDate = value;
        }
    }
}