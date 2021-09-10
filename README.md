# romaklayt.DynamicFilter
Simple filter for .net WebApi that enables your endpoint to query your requests by url.

It provides ways to query, order and page your webapi and mvc.

# .Net Core and .Net

First, download the packages into your project from nuget
```batch
nuget install romaklayt.DynamicFilter.Extensions
nuget install romaklayt.DynamicFilter.Binder.Net
```

# .Net Framework (>=4.6.1)

First, download the packages into your project from nuget
```batch
nuget install romaklayt.DynamicFilter.Extensions
```

If you need to add a filter to the **web api** controller use the package
```batch
nuget install romaklayt.DynamicFilter.Binder.NetFramework.WebApi
```

and add to your **WebApiConfig** class for register value providers (form-data, x-www-form-urlencoded and JSON) for body POST request
```C#
config.AddDynamicFilterProviders();
```

If you need to add a filter to the **MVC** controller use the package
```batch
nuget install romaklayt.DynamicFilter.Binder.NetFramework.Mvc
```

and add to your *Application_Start* **Global.asax.cs** class for register value providers (form-data, x-www-form-urlencoded and JSON) for body POST request
```C#
DynamicFilterProviders.AddProviders();
```

If you need to use DynamicFilter with IAsyncEnumerable or IAsyncQueryable use the package
```batch
nuget install romaklayt.DynamicFilter.Extensions.Async
```

After downloaded, go to your webapi and create an *get* or *post* endpoint receiving ``` DynamicFilter ``` as parameter.

```C#
[HttpGet]
public Task<List<User>> Get(DynamicFilter filter)
```

Now you can query your endpoint with the DynamicFilter properties. Check *"tests"* folder on the Api projects for examples.

```C#
[HttpGet]
public Task<List<User>> Get(DynamicFilter filter)
{
    var result = this.users.UseFilter(filter);

    return Task.FromResult(result.ToList());
}
```

DynamicFilter.Parser will transform your URI Queries into .Net Expressions. That way, you can use these expressions to filter your values into your database repository.


## Simple Query

Example Uri:

```http
GET http://url?query=name=Bruno
```
Expression generated by Uri:

```C#
x => x.Name == "Bruno"
```

## Complex Query

Example Uri:

```http
GET http://url?query=name=Bruno,lastname%r,age>=27
```
Expression generated by Uri:

```C#
x => x.Name == "Bruno" 
  && x.LastName.ToLower().Contains("r") 
  && x.Age >= 27
```

You can add conditions to your query sorting your filters with a comma (,), as shown above.

## Nested Query

Example Uri:

```http
GET http://url?query=address.number=23
```

Expression generated by Uri:
```C#
x => x.Address.Number == 23
```

# Ordering

You can also order your queries via DynamicFilter. You simply need to add an order parameter on your query, where you specify the property you'll use for order.

```http
GET http://url?query=name=Bruno&order=name
```

Default order type is Ascending. You can specify the order type with an equals (=) *Asc* or *Desc* after the property.

```http
GET http://url?query=name=Bruno&order=name=Asc

GET http://url?query=name=Bruno&order=name=Desc
```

On your DynamicFilter object received on the endpoint, you'll get the orderType as an Enum, this way you can order by the type specified on enum.

```C#
public Task<List<User>> Get(DynamicFilter filter)
{
    var result = users.UseFilter(filter);

    return Task.FromResult(result.ToList());
}
```

# Pagging


```http
GET http://url?query=name%b&order=name&page=1&pagesize=10
```

In romaklayt.DynamicFilter.Extensions and romaklayt.DynamicFilter.Extensions.Async there is a method of expanding the **ToPagedList** which returns *PageModel* with *info about page* and your *filtered data*. 

```C#
var page = users.ToPagedList(filterModel); #only paging
```

or

```C#
var filteredUsers = await users.UseFilter(filterModel); #apply filter
return await filteredUsers.ToPagedList(filterModel); #return page model
```

If you no need page info, you simply needs to add the parameters page and pagesize on your get request.

```C#
result = result.Skip(filter.Page).Take(filter.PageSize);
```

Example:

```C#
[HttpGet("page")]
public async Task<PageModel<User>> GetPage(DynamicFilterModel filterModel)
{
    var filteredUsers = await Data.Users.UseFilter(filterModel);
    return await filteredUsers.ToPagedList(filterModel);
}
```

# Select

To select, you simply needs to add the parameter select with the properties you want to select from. It will render either an linq select and a plain string select.

```http
GET http://url?select=name,age
```

# Filter operator support

- Equals (=)

```http
GET http://url?query=name=Bruno
```

- Contains (%)

```http
GET http://url?query=name%b
```

- Contains Case Sensitive (%%)

```http
GET http://url?query=name%%B
```

- GreaterThan (>)

```http
GET http://url?query=age>15
```

- GreaterOrEqual (>=)

```http
GET http://url?query=age>=15
```

- LessThan (<)

```http
GET http://url?query=age<15
```

- LessOrEqual (<=)

```http
GET http://url?query=age<=15
```

- NotEquals (!=)

```http
GET http://url?query=age!=15
```