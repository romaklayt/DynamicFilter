using System.Web.Mvc;
using romaklayt.DynamicFilter.Binder.NetFramework.Mvc.Models;
using romaklayt.DynamicFilter.Extensions;

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
    }
}