using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using romaklayt.DynamicFilter.Binder;
using romaklayt.DynamicFilter.Extensions;
using romaklayt.DynamicFilter.Test.Api.Models;

namespace romaklayt.DynamicFilter.Test.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly List<User> users = new()
        {
            new User("Bruno", 27, new Address("street 1", 23, new Zip(123456, "USA")))
            {
                Roles = new List<Role> { new(1, "Admin") }
            },
            new User("Fred", 33, new Address("street 2", null, new Zip(1234567, "BR")))
            {
                Roles = new List<Role> { new(2, "Admin") }
            },
            new User("Albert", 37, new Address("street 3", 43, new Zip(54375445, "BR")))
            {
                Roles = new List<Role> { new(null, "Read"), new(3, "Write") }
            },
            new User("Lucao", 23, new Address("street 4", 53, new Zip(76878979, "PT")))
            {
                Roles = new List<Role> { new(4, "Read"), new(5, "Write") }
            },
            new User("Luide", 28, new Address("street 5", 63, new Zip(65756443, "PT")))
            {
                Roles = new List<Role> { new(6, "Read"), new(7, "Write") }
            }
        };

        [HttpGet]
        public async Task<IEnumerable<User>> GetList(DynamicFilterModel filterModelModel)
        {
            return await users.UseFilter(filterModelModel);
        }
        
        [HttpPost]
        public async Task<IEnumerable<User>> GetPostList(DynamicFilterModel filterModelModel)
        {
            return await users.UseFilter(filterModelModel);
        }

        [HttpGet("page")]
        public async Task<PageModel<User>> GetPage(DynamicFilterModel filterModel)
        {
            var filteredUsers = await users.UseFilter(filterModel);
            return await filteredUsers.ToPagedList(filterModel);
        }
    }
}