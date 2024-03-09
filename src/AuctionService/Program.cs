using AuctionService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

{
    builder.Services.AddControllers();
    builder.Services.AddDbContext<AuctionDbContext>(options => 
    {
        string connection_string = builder.Configuration.GetConnectionString("DefaultConnection");
        options.UseNpgsql(connection_string);
    });
    builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
}

var app = builder.Build();

{
    app.UseAuthorization();
    app.MapControllers();
}

try
{
    DbInitializer.InitDb(app);
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}

app.Run();