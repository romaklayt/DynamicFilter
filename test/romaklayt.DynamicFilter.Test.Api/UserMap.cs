using AutoMapper;
using romaklayt.DynamicFilter.Test.Api.Models;

namespace romaklayt.DynamicFilter.Test.Api
{
    public class UserMap : Profile
    {
        public UserMap()
        {
            CreateMap<User, UserViewModel>();
        }
    }
}