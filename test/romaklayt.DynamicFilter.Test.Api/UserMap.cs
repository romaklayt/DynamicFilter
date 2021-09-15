using AutoMapper;
using romaklayt.DynamicFilter.Test.Api.Models;

namespace romaklayt.DynamicFilter.Test.Api
{
    public class UserMap : Profile
    {
        public UserMap()
        {
            CreateMap<User, UserViewModel>().ForMember(model => model.Address_Zip_Country,
                expression => expression.MapFrom(user => user.Address.Zip.Country));
        }
    }
}