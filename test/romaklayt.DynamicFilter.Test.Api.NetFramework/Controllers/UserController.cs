using System.Threading.Tasks;
using System.Web.Mvc;
using romaklayt.DynamicFilter.Binder.NetFramework.Mvc.Models;
using romaklayt.DynamicFilter.Extensions.EntityFramework;

namespace romaklayt.DynamicFilter.Test.Api.NetFramework.Controllers
{
    public class UserController : Controller
    {
        [HttpGet]
        [ActionName("List")]
        public JsonResult GetList(DynamicComplexModel complexModelModel) =>
            Json(Data.Users.Apply(complexModelModel), JsonRequestBehavior.AllowGet);

        [HttpPost]
        [ActionName("List")]
        public JsonResult GetPostList(DynamicComplexModel complexModelModel) =>
            Json(Data.Users.Apply(complexModelModel));

        [HttpGet]
        [ActionName("Page")]
        public async Task<JsonResult> GetPage(DynamicComplexModel complexModel)
        {
            var filteredUsers = Data.Users.Apply(complexModel);
            return Json(await filteredUsers.ToPageModel(complexModel), JsonRequestBehavior.AllowGet);
        }
    }
}