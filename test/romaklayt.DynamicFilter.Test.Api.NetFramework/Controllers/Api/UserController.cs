using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using romaklayt.DynamicFilter.Binder.NetFramework.WebApi;
using romaklayt.DynamicFilter.Common;
using romaklayt.DynamicFilter.Extensions;
using romaklayt.DynamicFilter.Test.Api.NetFramework.Models;

namespace romaklayt.DynamicFilter.Test.Api.NetFramework.Controllers.Api
{
    public class UserController : ApiController
    {
        [HttpGet]
        [ActionName("List")]
        public IEnumerable<User> GetList(DynamicFilterModel filterModelModel)
        {
            return Data.Users.UseFilter(filterModelModel).Result;
        }

        [HttpPost]
        [ActionName("List")]
        public async Task<IEnumerable<User>> GetPostList(DynamicFilterModel filterModelModel)
        {
            return await Data.Users.UseFilter(filterModelModel);
        }

        [HttpGet]
        [ActionName("Page")]
        public async Task<PageModel<User>> GetPage(DynamicFilterModel filterModel)
        {
            var filteredUsers = await Data.Users.UseFilter(filterModel);
            return await filteredUsers.ToPagedList(filterModel);
        }
    }
}