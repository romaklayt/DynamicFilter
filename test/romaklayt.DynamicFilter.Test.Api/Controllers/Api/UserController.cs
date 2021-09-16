using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using romaklayt.DynamicFilter.Binder.Net;
using romaklayt.DynamicFilter.Common;
using romaklayt.DynamicFilter.Extensions;
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
            var filteredUsers = await Data.Users.UseFilter(filterModel);
            return await filteredUsers.ToPagedList(filterModel);
        }

        [HttpGet("context")]
        [ProducesResponseType(typeof(UserViewModel), 200)]
        public async Task<object> GetContextList(DynamicFilterModel filterModelModel)
        {
            return await _myContext.Users.UseFilter<User, UserViewModel>(filterModelModel);
        }
    }
}