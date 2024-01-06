using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using romaklayt.DynamicFilter.Common;
using romaklayt.DynamicFilter.Common.Interfaces;

namespace romaklayt.DynamicFilter.Binder.Net.Filters;

public class PageInfoWriter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext _, ActionExecutionDelegate next)
    {
        var context = await next();
        if (context.Result is not ObjectResult { Value: IPageInfo pageInfo }) return;
        context.HttpContext.Response.Headers.Append(PageInfoHeaders.PageSize, pageInfo.PageSize.ToString());
        context.HttpContext.Response.Headers.Append(PageInfoHeaders.CurrentPage, pageInfo.CurrentPage.ToString());
        context.HttpContext.Response.Headers.Append(PageInfoHeaders.TotalPages, pageInfo.TotalPages.ToString());
        context.HttpContext.Response.Headers.Append(PageInfoHeaders.HasNext, pageInfo.HasNext.ToString());
        context.HttpContext.Response.Headers.Append(PageInfoHeaders.HasPrevious, pageInfo.HasPrevious.ToString());
        context.HttpContext.Response.Headers.Append(PageInfoHeaders.TotalCount, pageInfo.TotalCount.ToString());
    }
}