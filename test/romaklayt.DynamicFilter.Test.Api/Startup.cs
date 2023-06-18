using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using romaklayt.DynamicFilter.Binder.Net.Factories;
using romaklayt.DynamicFilter.Binder.Net.Filters;
using romaklayt.DynamicFilter.Parser;

namespace romaklayt.DynamicFilter.Test.Api;

public class Startup
{
    private readonly string _appVersion = typeof(DynamicComplexParser).Assembly
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
        ?.InformationalVersion.Split('+').First();

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<MyContext>(opt => opt.UseInMemoryDatabase("Test").EnableSensitiveDataLogging());
        services.AddScoped<MyContext>();
        services.AddAutoMapper(typeof(UserMap));
        // Register the Swagger generator, defining 1 or more Swagger documents
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = _appVersion,
                Title = "Test Manager",
                Contact = new OpenApiContact
                {
                    Name = "Roman Kolyago",
                    Email = "romakolyago18@gmail.com",
                    Url = new Uri("https://github.com/romaklayt")
                }
            });
        });
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
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            c.RoutePrefix = "";
        });
        app.UseRouting();

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }

    private void AddTestData(MyContext context)
    {
        context.Addresses.AddRange(Data.Addresses);
        context.Roles.AddRange(Data.Users.SelectMany(user => user.Roles));
        context.Users.AddRange(Data.Users);
        context.SaveChanges();
    }
}