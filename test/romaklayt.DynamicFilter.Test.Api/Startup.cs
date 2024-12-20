using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using romaklayt.DynamicFilter.Binder.Net.Factories;
using romaklayt.DynamicFilter.Binder.Net.Filters;

namespace romaklayt.DynamicFilter.Test.Api;

public class Startup
{
    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<MyContext>(opt => opt.UseInMemoryDatabase("Test").EnableSensitiveDataLogging());
        services.AddScoped<MyContext>();
        services.AddAutoMapper(typeof(UserMap));
        // Register the Swagger generator, defining 1 or more Swagger documents
        services.AddOpenApi();
        services.AddControllers(options =>
        {
            options.Filters.Add(new PageInfoWriter());
            options.ValueProviderFactories.Add(new JsonBodyValueProviderFactory());
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, MyContext context)
    {
        if (env.IsDevelopment()) app.UseDeveloperExceptionPage();
        AddTestData(context);
        app.UseRouting();
        app.UseSwaggerUI(options => { options.SwaggerEndpoint("/openapi/v1.json", "v1"); });
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapOpenApi();
        });
    }

    private void AddTestData(MyContext context)
    {
        context.Addresses.AddRange(Data.Addresses);
        context.Roles.AddRange(Data.Users.SelectMany(user => user.Roles));
        context.Users.AddRange(Data.Users);
        context.SaveChanges();
    }
}