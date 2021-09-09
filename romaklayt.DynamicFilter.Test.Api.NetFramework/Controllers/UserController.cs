using System.Threading.Tasks;
using System.Web.Mvc;
using romaklayt.DynamicFilter.Binder.NetFramework.Mvc;
using romaklayt.DynamicFilter.Extensions;

namespace romaklayt.DynamicFilter.Test.Api.NetFramework.Controllers
{
    public class UserController : Controller
    {
        [HttpGet]
        [ActionName("List")]
        public JsonResult GetList(DynamicFilterModel filterModelModel)
        {
            return Json(Data.Users.UseFilter(filterModelModel).Result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ActionName("List")]
        public async Task<JsonResult> GetPostList(DynamicFilterModel filterModelModel)
        {
            return Json(await Data.Users.UseFilter(filterModelModel));
        }

        [HttpGet]
        [ActionName("Page")]
        public async Task<JsonResult> GetPage(DynamicFilterModel filterModel)
        {
            var filteredUsers = await Data.Users.UseFilter(filterModel);
            return Json(await filteredUsers.ToPagedList(filterModel), JsonRequestBehavior.AllowGet);
        }
    }
}