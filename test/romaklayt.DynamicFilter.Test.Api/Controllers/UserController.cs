using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using romaklayt.DynamicFilter.Binder.Net.Models;
using romaklayt.DynamicFilter.Extensions;
using romaklayt.DynamicFilter.Test.Api.Models;

namespace romaklayt.DynamicFilter.Test.Api.Controllers;

[Route("[controller]")]
[Controller]
public class UserController : Controller
{
    [HttpGet]
    public IEnumerable<User> GetList([FromQuery] DynamicComplexModel complexModelModel) =>
        Data.Users.Apply(complexModelModel);

    [HttpPost]
    public IEnumerable<User> GetPostList(DynamicComplexModel complexModelModel) => Data.Users.Apply(complexModelModel);
}