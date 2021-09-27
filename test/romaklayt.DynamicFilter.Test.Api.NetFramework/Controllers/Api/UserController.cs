using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using romaklayt.DynamicFilter.Binder.NetFramework.WebApi.Models;
using romaklayt.DynamicFilter.Common;
using romaklayt.DynamicFilter.Extensions;
using romaklayt.DynamicFilter.Test.Api.NetFramework.Models;

namespace romaklayt.DynamicFilter.Test.Api.NetFramework.Controllers.Api
{
    public class UserController : ApiController
    {
        [HttpGet]
        [ActionName("List")]
        public IEnumerable<User> GetList(DynamicComplexModel complexModelModel)
        {
            return Data.Users.UseFilter(complexModelModel).Result;
        }

        [HttpPost]
        [ActionName("List")]
        public async Task<IEnumerable<User>> GetPostList(DynamicComplexModel complexModelModel)
        {
            return await Data.Users.UseFilter(complexModelModel);
        }

        [HttpGet]
        [ActionName("Page")]
        public async Task<PageModel<User>> GetPage(DynamicComplexModel complexModel)
        {
            var filteredUsers = await Data.Users.UseFilter(complexModel);
            return await filteredUsers.ToPagedList(complexModel);
        }
    }
}