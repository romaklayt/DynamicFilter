using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using romaklayt.DynamicFilter.Binder;
using romaklayt.DynamicFilter.Common;
using romaklayt.DynamicFilter.Extensions;
using romaklayt.DynamicFilter.Test.Api.Models;

namespace romaklayt.DynamicFilter.Test.Api.Controllers
{
    [Route("[controller]")]
    [Controller]
    public class UserController : Controller
    {
        [HttpGet]
        public async Task<IEnumerable<User>> GetList(DynamicFilterModel filterModelModel)
        {
            return await Data.Users.UseFilter(filterModelModel);
        }

        [HttpPost]
        public async Task<IEnumerable<User>> GetPostList(DynamicFilterModel filterModelModel)
        {
            return await Data.Users.UseFilter(filterModelModel);
        }

        [HttpGet("page")]
        public async Task<PageModel<User>> GetPage(DynamicFilterModel filterModel)
        {
            var filteredUsers = await Data.Users.UseFilter(filterModel);
            return await filteredUsers.ToPagedList(filterModel);
        }
    }
}