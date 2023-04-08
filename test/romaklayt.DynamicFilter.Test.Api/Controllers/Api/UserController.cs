﻿using System;
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
    public IEnumerable<User> GetList([FromQuery] DynamicComplexModel complexModelModel) =>
        Data.Users.Apply(complexModelModel);

    [HttpPost]
    public IEnumerable<User> GetPostList(DynamicComplexModel complexModelModel) => Data.Users.Apply(complexModelModel);

    [HttpGet("context")]
    [ProducesResponseType(typeof(UserViewModel), 200)]
    public object GetContextList([FromQuery] DynamicComplexModel complexModelModel) =>
        _myContext.Users.Apply(complexModelModel);

    [HttpGet("count")]
    [ProducesResponseType(typeof(int), 200)]
    public object Count([FromQuery] DynamicFilterModel filterModelModel) =>
        _myContext.Users.ApplyFilter(filterModelModel);

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(User), 200)]
    public async Task<object> GetById([FromQuery] DynamicSelectModel dynamicSelectModel, Guid id) =>
        await _myContext.Users.ApplySelect(dynamicSelectModel).DynamicFirstOfDefault("Id", id);
}