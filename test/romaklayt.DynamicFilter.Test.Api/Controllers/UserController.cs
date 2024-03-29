﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using romaklayt.DynamicFilter.Common;
using romaklayt.DynamicFilter.Common.Models;
using romaklayt.DynamicFilter.Extensions.EntityFrameworkCore;
using romaklayt.DynamicFilter.Test.Api.Models;

namespace romaklayt.DynamicFilter.Test.Api.Controllers;

[Route("[controller]")]
[Controller]
public class UserController : Controller
{
    [HttpGet]
    public IEnumerable<User> GetList([FromQuery] DynamicComplexModel complexModelModel) => Data.Users.Apply(complexModelModel);

    [HttpPost]
    public IEnumerable<User> GetPostList(DynamicComplexModel complexModelModel) => Data.Users.Apply(complexModelModel);

    [HttpGet("page")]
    public async Task<PageFlatModel<User>> GetPage([FromQuery] DynamicComplexModel complexModel)
    {
        var filteredUsers = Data.Users.Apply(complexModel);
        return await filteredUsers.ToPageFlatModel(complexModel);
    }
}