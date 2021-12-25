using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using romaklayt.DynamicFilter.Binder.Net.Models;
using romaklayt.DynamicFilter.Extensions;
using romaklayt.DynamicFilter.Test.Api.Models;

namespace romaklayt.DynamicFilter.Test.Api.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly MyContext _myContext;

    public UserController(MyContext myContext, IMapper mapper)
    {
        _myContext = myContext;
        _mapper = mapper;
    }

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
    public async Task<object> GetPage([FromQuery] DynamicComplexModel complexModel)
    {
        var data = await Data.Users.ToPagedList(complexModel);
        return data;
    }

    [HttpGet("context")]
    [ProducesResponseType(typeof(UserViewModel), 200)]
    public object GetContextList([FromQuery] DynamicComplexModel complexModelModel)
    {
        return _myContext.Users.UseFilter(complexModelModel).Result;
    }

    [HttpGet("count")]
    [ProducesResponseType(typeof(int), 200)]
    public object Count([FromQuery] DynamicFilterModel filterModelModel)
    {
        return _myContext.Users.UseFilter(filterModelModel).Result;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(User), 200)]
    public async Task<object> GetById([FromQuery] DynamicSelectModel dynamicSelectModel, Guid id)
    {
        return await _myContext.Users.UseFilter(dynamicSelectModel).Result.DynamicFirstOfDefault("Id", id);
    }
}