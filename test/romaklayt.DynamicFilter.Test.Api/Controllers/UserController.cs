using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using romaklayt.DynamicFilter.Binder.Net.Models;
using romaklayt.DynamicFilter.Common;
using romaklayt.DynamicFilter.Extensions;
using romaklayt.DynamicFilter.Test.Api.Models;

namespace romaklayt.DynamicFilter.Test.Api.Controllers;

[Route("[controller]")]
[Controller]
public class UserController : Controller
{
    [HttpGet]
    public IEnumerable<User> GetList([FromQuery] DynamicComplexModel complexModelModel)
    {
        return Data.Users.Apply(complexModelModel);
    }

    [HttpPost]
    public IEnumerable<User> GetPostList(DynamicComplexModel complexModelModel)
    {
        return Data.Users.Apply(complexModelModel);
    }

    [HttpGet("page")]
    public async Task<PageModel<User>> GetPage([FromQuery] DynamicComplexModel complexModel)
    {
        var filteredUsers = Data.Users.Apply(complexModel);
        return await filteredUsers.ToPageModel(complexModel);
    }
}