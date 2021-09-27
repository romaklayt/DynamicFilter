using System.Threading.Tasks;
using System.Web.Mvc;
using romaklayt.DynamicFilter.Binder.NetFramework.Mvc.Models;
using romaklayt.DynamicFilter.Extensions;

namespace romaklayt.DynamicFilter.Test.Api.NetFramework.Controllers
{
    public class UserController : Controller
    {
        [HttpGet]
        [ActionName("List")]
        public JsonResult GetList(DynamicComplexModel complexModelModel)
        {
            return Json(Data.Users.UseFilter(complexModelModel).Result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ActionName("List")]
        public async Task<JsonResult> GetPostList(DynamicComplexModel complexModelModel)
        {
            return Json(await Data.Users.UseFilter(complexModelModel));
        }

        [HttpGet]
        [ActionName("Page")]
        public async Task<JsonResult> GetPage(DynamicComplexModel complexModel)
        {
            var filteredUsers = await Data.Users.UseFilter(complexModel);
            return Json(await filteredUsers.ToPagedList(complexModel), JsonRequestBehavior.AllowGet);
        }
    }
}