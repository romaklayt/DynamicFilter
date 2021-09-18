using System.Collections.Generic;
using System.Linq;
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
            return await Data.Users.ToPagedList(filterModel);
        }

        [HttpGet("context")]
        [ProducesResponseType(typeof(UserViewModel), 200)]
        public object GetContextList(DynamicFilterModel filterModelModel)
        {
            return Data.Users.ToList().UseFilter(filterModelModel).Result.GetOnlySelectedProperties(filterModelModel);
        }
    }
}