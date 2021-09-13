using System;
using System.Collections.Generic;

namespace romaklayt.DynamicFilter.Test.Api.Models
{
    public class User
    {
        private DateTime _birthDate;

        public User()
        {
        }

        public User(string name, int age, Address address)
        {
            Name = name;
            Age = age;
            Address = address;
            Date = DateTime.Now;
        }

        public Guid Id { get; set; } = new();
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