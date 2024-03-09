When you use EF tools, it will build & run the application and look what kind of services you have configured and based on those services ef tools will then apply migration to the database.

```c#

builder.Services.AddDbContext<AuctionDbContext>(options => 
{
    string connection_string = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connection_string);
});
```

Allows the ASP.NET framework to redirect the requests to the specific API controllers: `app.MapControllers();`.

`public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions()` - ActionResult lets us return an HTTP response like 200 OK.

`return _mapper.Map<AuctionDto>(auction);` - When you return a DTO the ASP.NET automatically converts your object to `ActionResult<T>`

`Boolean succesful = await _context.SaveChangesAsync() > 0;` - Returns an integer, for the count of changes that were made in the database.

`_mapper.Map<AuctionDto>(auction)` - mapper will create the specified Object type out of any object, but that object type needs to have a mapping defined in the MappingProfiles.

`auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;` - ?? the null conditional operator, if the left is null return right instead.

**Indexing**: an index is a separate data in the database, there are many types of indexes, but the general idea is that you take a key (such as the ID or Surname or Description) and you map these things to the locations of the actual elements, that way you can find the elements quickly based on the key that you indexed.

Route parameter: `[HttpGet("{id}")]`.

Query parameter: `GET /api/search?searchTerm=laptop` - In ASP.NET Core Web API, you don't need to do anything special to accept query parameters; just having a parameter on your action method with the same name as the query parameter is enough.

You can have custom HTTP headers, they should be marked with `X-` in the start to specify that this is an extension. But it is recommended to only use the **standard headers**.

Hot Reload can be bugy, sometimes its better to just restart.

This anonymous object gets automatically turned into a json body.
```C#
    return Ok(new 
    {
        results = result.Results,
        pageCount = result.PageCount,
        totalCount = result.TotalCount
    });
```

Synchronous communication - Service A sends an HTTP or gRPC to Service B and while Service A is waiting for the response it stops all work.

??? What is up with all the dependenncy injection methods, how do they work????