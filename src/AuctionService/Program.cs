using AuctionService;
using AuctionService.Data;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
    builder.Services.AddMassTransit(x => 
    {
        x.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();

        x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));

        x.AddEntityFrameworkOutbox<AuctionDbContext>(opt =>
        {
            opt.QueryDelay = TimeSpan.FromSeconds(10);
            opt.UsePostgres();
            opt.UseBusOutbox();
        });

        x.UsingRabbitMq((context, configuration) => 
        {
            configuration.ConfigureEndpoints(context);
        });
    });
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme) // This configures to call Identity service to verify the token everytime
        .AddJwtBearer(options =>
        {
            options.Authority = builder.Configuration["IdentityServiceUrl"];
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters.ValidateAudience = false;
            options.TokenValidationParameters.NameClaimType = "username";
        });
}

var app = builder.Build();

{
    app.UseAuthentication();
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