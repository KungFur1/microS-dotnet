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