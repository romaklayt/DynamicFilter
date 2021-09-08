using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using romaklayt.DynamicFilter.Binder;
using romaklayt.DynamicFilter.Parser;

namespace romaklayt.DynamicFilter.Test.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        List<User> users = new List<User>()
        {
            new User("Bruno", 27, new Address("street 1", 23, new Zip(123456, "USA")))
            {
                Roles = new List<Role>() { new Role(1,"Admin") }
            },
            new User("Fred", 33, new Address("street 2", null, new Zip(1234567, "BR")))
            {
                Roles = new List<Role>() { new Role(2, "Admin") }
            },
            new User("Albert", 37, new Address("street 3", 43, new Zip(54375445, "BR")))
            {
                Roles = new List<Role>() { new Role(null,"Read"), new Role(3,"Write") }
            },
            new User("Lucao", 23, new Address("street 4", 53, new Zip(76878979, "PT")))
            {
                Roles = new List<Role>() { new Role(4,"Read"), new Role(5,"Write") }
            },
            new User("Luide", 28, new Address("street 5", 63, new Zip(65756443, "PT")))
            {
                Roles = new List<Role>() { new Role(6,"Read"), new Role(7,"Write") }
            }
        };
        
        [HttpGet]
        public Task<List<User>> Get(DynamicFilterModel filterModelModel)
        {
            var filter = filterModelModel.BindFilterExpressions<User>();
            IQueryable<User> result;
            if (filter.Filter != null)
                result = users.Where(filter.Filter.Compile()).AsQueryable();
            else
                result = users.AsQueryable();

            if (filter.Select != null)
                result = result.Select(filter.Select);

            if (filter.Order != null)
            {
                if (filter.OrderType == OrderType.Asc)
                    result = result.OrderBy(filter.Order).AsQueryable();
                else
                    result = result.OrderByDescending(filter.Order).AsQueryable();
            }


            return Task.FromResult(result.ToList());
        }
        [HttpGet("page")]
        public Task<PageModel<User>> GetPage(DynamicFilterModel filterModelModel)
        {
            var filter = filterModelModel.BindFilterExpressions<User>();
            IQueryable<User> result;
            if (filter.Filter != null)
                result = users.Where(filter.Filter.Compile()).AsQueryable();
            else
                result = users.AsQueryable();

            if (filter.Select != null)
                result = result.Select(filter.Select);

            if (filter.Order != null)
            {
                if (filter.OrderType == OrderType.Asc)
                    result = result.OrderBy(filter.Order).AsQueryable();
                else
                    result = result.OrderByDescending(filter.Order).AsQueryable();
            }


            return Task.FromResult(result.ToPagedList(filter.Page, filter.PageSize));
        }
    }

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
            Date = DateTime.Now;
        }

        public string Name { get; set; }
        public int Age { get; set; }
        public Address Address { get; set; }
        public List<Role> Roles { get; set; }

        private DateTime _birthDate;
        private DateTimeOffset Date { get; set; }

        public DateTime BirthDate
        {
            get
            {
                return DateTime.Now.AddYears(-Age);
            }
            set
            {
                _birthDate = value;
            }
        }
    }

    public class Role
    {
        public Role(int? id, string name)
        {
            Id = id;
            Name = name;
        }

        public int? Id { get; set; }
        public string Name { get; set; }
    }

    public class Address
    {
        public Address(string street, int? number, Zip zip)
        {
            Street = street;
            Number = number;
            Zip = zip;
        }      

        public string Street { get; set; }
        public int? Number { get; set; }
        public Zip Zip { get; private set; }
    }

    public class Zip
    {
        public Zip(int number, string country)
        {
            Number = number;
            Country = country;
        }
        public string Country { get; set; }
        public int Number { get; set; }
    }
}