using System.Collections.Generic;
using System.Web.Http;
using romaklayt.DynamicFilter.Binder.NetFramework.WebApi.Models;
using romaklayt.DynamicFilter.Extensions;
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
    }
}