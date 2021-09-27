using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using romaklayt.DynamicFilter.Binder.Net.Models;
using romaklayt.DynamicFilter.Common;
using romaklayt.DynamicFilter.Extensions;
using romaklayt.DynamicFilter.Parser;
using romaklayt.DynamicFilter.Test.Api.Models;

namespace romaklayt.DynamicFilter.Test.Api.Controllers.Api
{
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
        public async Task<IEnumerable<User>> GetList(DynamicComplexModel complexModelModel)
        {
            return await Data.Users.UseFilter(complexModelModel);
        }

        [HttpPost]
        public async Task<IEnumerable<User>> GetPostList(DynamicComplexModel complexModelModel)
        {
            return await Data.Users.UseFilter(complexModelModel);
        }

        [HttpGet("page")]
        public async Task<PageModel<User>> GetPage(DynamicComplexModel complexModel)
        {
            return await Data.Users.ToPagedList(complexModel);
        }

        [HttpGet("context")]
        [ProducesResponseType(typeof(UserViewModel), 200)]
        public object GetContextList(DynamicComplexModel complexModelModel)
        {
            return _myContext.Users.UseFilter(complexModelModel).Result;
        }

        [HttpGet("contextrender")]
        [ProducesResponseType(typeof(UserViewModel), 200)]
        public object GetContextListRender(DynamicComplexModel complexModelModel)
        {
            return _myContext.Users.UseFilter(complexModelModel).Result.RenderOnlySelectedProperties(complexModelModel);
        }

        [HttpGet("count")]
        [ProducesResponseType(typeof(int), 200)]
        public object Count(DynamicFilterModel filterModelModel)
        {
            return _myContext.Users.UseFilter(filterModelModel).Result;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(User), 200)]
        public object GetById(DynamicSelectModel dynamicSelectModel, Guid id)
        {
            return _myContext.Users.UseSelect(dynamicSelectModel).FirstOrDefault(user => user.Id == id);
        }

        [HttpGet("render/{id}")]
        [ProducesResponseType(typeof(User), 200)]
        public async Task<object> RenderGetById(DynamicSelectModel dynamicSelectModel, Guid id)
        {
            var user = await _myContext.Users.UseSelect(dynamicSelectModel).FirstOrDefaultAsync(user => user.Id == id);
            return user.RenderOnlySelectedProperties(dynamicSelectModel);
        }
    }
}