using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using romaklayt.DynamicFilter.Binder.NetFramework.WebApi.Models;
using romaklayt.DynamicFilter.Common;
using romaklayt.DynamicFilter.Extensions.EntityFramework;
using romaklayt.DynamicFilter.Test.Api.NetFramework.Models;

namespace romaklayt.DynamicFilter.Test.Api.NetFramework.Controllers.Api
{
    public class UserController : ApiController
    {
        [HttpGet]
        [ActionName("List")]
        public IEnumerable<User> GetList(DynamicComplexModel complexModelModel) => Data.Users.Apply(complexModelModel);

        [HttpPost]
        [ActionName("List")]
        public IEnumerable<User> GetPostList(DynamicComplexModel complexModelModel) =>
            Data.Users.Apply(complexModelModel);

        [HttpGet]
        [ActionName("Page")]
        public async Task<PageModel<User>> GetPage(DynamicComplexModel complexModel)
        {
            var filteredUsers = Data.Users.Apply(complexModel);
            return await filteredUsers.ToPageModel(complexModel);
        }

        [HttpGet]
        [ActionName("Pageflat")]
        public async Task<PageFlatModel<User>> GetFlatPage(DynamicComplexModel complexModel)
        {
            var filteredUsers = Data.Users.Apply(complexModel);
            return await filteredUsers.ToPageFlatModel(complexModel);
        }
    }
}