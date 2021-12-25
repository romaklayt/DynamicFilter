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
    public async Task<IEnumerable<User>> GetList([FromQuery] DynamicComplexModel complexModelModel)
    {
        return await Data.Users.UseFilter(complexModelModel);
    }

    [HttpPost]
    public async Task<IEnumerable<User>> GetPostList(DynamicComplexModel complexModelModel)
    {
        return await Data.Users.UseFilter(complexModelModel);
    }

    [HttpGet("page")]
    public async Task<PageModel<User>> GetPage([FromQuery] DynamicComplexModel complexModel)
    {
        var filteredUsers = await Data.Users.UseFilter(complexModel);
        return await filteredUsers.ToPagedList(complexModel);
    }
}