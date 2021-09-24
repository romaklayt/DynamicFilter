using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using romaklayt.DynamicFilter.Binder.Net;
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
        public async Task<IEnumerable<User>> GetList(DynamicFilterModel filterModelModel)
        {
            return await Data.Users.UseFilter(filterModelModel);
        }

        [HttpPost]
        public async Task<IEnumerable<User>> GetPostList(DynamicFilterModel filterModelModel)
        {
            return await Data.Users.UseFilter(filterModelModel);
        }

        [HttpGet("page")]
        public async Task<PageModel<User>> GetPage(DynamicFilterModel filterModel)
        {
            return await Data.Users.ToPagedList(filterModel);
        }

        [HttpGet("context")]
        [ProducesResponseType(typeof(UserViewModel), 200)]
        public object GetContextList(DynamicFilterModel filterModelModel)
        {
            return _myContext.Users.UseFilter(filterModelModel).Result;
        }
        
        [HttpGet("contextrender")]
        [ProducesResponseType(typeof(UserViewModel), 200)]
        public object GetContextListRender(DynamicFilterModel filterModelModel)
        {
            return _myContext.Users.UseFilter(filterModelModel).Result.RenderOnlySelectedProperties(filterModelModel);
        }

        [HttpGet("count")]
        [ProducesResponseType(typeof(int), 200)]
        public object Count(DynamicCountFilterModel filterModelModel)
        {
            return _myContext.Users.UseFilter(filterModelModel).Result;
        }
    }
}