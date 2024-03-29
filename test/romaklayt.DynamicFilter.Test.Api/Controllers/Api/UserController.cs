﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using romaklayt.DynamicFilter.Common;
using romaklayt.DynamicFilter.Common.Models;
using romaklayt.DynamicFilter.Extensions.EntityFrameworkCore;
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
    public IEnumerable<User> GetList([FromQuery] DynamicComplexModel complexModelModel) => Data.Users.Apply(complexModelModel);

    [HttpPost]
    public IEnumerable<User> GetPostList(DynamicComplexModel complexModelModel) => Data.Users.Apply(complexModelModel);

    [HttpGet("page")]
    public async Task<object> GetPage([FromQuery] DynamicComplexModel complexModel)
    {
        var data = await _myContext.Users.ToPageModel(complexModel);
        return _mapper.Map<PageModel<UserViewModel>>(data);
    }

    [HttpGet("page/filter")]
    public async Task<object> GetPageWithoutSelect([FromQuery] DynamicPagingModel dynamicPagingModel)
    {
        var data = await _myContext.Users.ToPageModel(dynamicPagingModel);
        return _mapper.Map<PageModel<UserViewModel>>(data);
    }

    [HttpGet("pageflat")]
    [ProducesResponseType(typeof(List<UserViewModel>), 200)]
    public async Task<object> GetFlatPage([FromQuery] DynamicComplexModel complexModel)
    {
        var data = await _myContext.Users.ToPageFlatModel(complexModel);
        return _mapper.Map<PageFlatModel<UserViewModel>>(data);
    }

    [HttpGet("context")]
    [ProducesResponseType(typeof(UserViewModel), 200)]
    public object GetContextList([FromQuery] DynamicComplexModel complexModelModel) => _myContext.Users.Apply(complexModelModel);

    [HttpGet("count")]
    [ProducesResponseType(typeof(int), 200)]
    public async Task<object> Count([FromQuery] DynamicFilterModel filterModelModel) => await _myContext.Users.CountAsync(filterModelModel);

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(User), 200)]
    public async Task<object> GetById([FromQuery] DynamicSelectModel dynamicSelectModel, Guid id) =>
        await _myContext.Users.ApplySelect(dynamicSelectModel).DynamicFirstOfDefault("Id", id);
}