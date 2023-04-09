using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using romaklayt.DynamicFilter.Common;
using romaklayt.DynamicFilter.Test.Api.Models;

namespace romaklayt.DynamicFilter.Test.Api;

public class UserMap : Profile
{
    public UserMap()
    {
        CreateMap<User, UserViewModel>();
        CreateMap(typeof(PageModel<>), typeof(PageModel<>)).ConvertUsing(typeof(Converter<,>));
    }
}

internal class Converter<TSource, TDest> : ITypeConverter<PageModel<TSource>, PageModel<TDest>>
{
    public PageModel<TDest>
        Convert(PageModel<TSource> source, PageModel<TDest> destination, ResolutionContext context) =>
        new(context.Mapper.Map<List<TDest>>(source.ToList()), source.TotalCount,
            source.CurrentPage, source.PageSize);
}