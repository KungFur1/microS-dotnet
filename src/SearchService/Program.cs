using MassTransit;
using MongoDB.Driver;
using MongoDB.Entities;
using Polly;
using Polly.Extensions.Http;
using SearchService;

var builder = WebApplication.CreateBuilder(args);

{
    builder.Services.AddControllers();
    builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetPolicy());
    builder.Services.AddMassTransit(x => 
    {
        x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
        x.AddConsumersFromNamespaceContaining<AuctionUpdatedConsumer>();
        x.AddConsumersFromNamespaceContaining<AuctionDeletedConsumer>();

        x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));

        x.UsingRabbitMq((context, configuration) => 
        {
            configuration.Host(builder.Configuration["RabbitMq:Host"], "/", host => 
            {
                host.Username(builder.Configuration.GetValue("RabbitMq:Username", "guest"));
                host.Password(builder.Configuration.GetValue("RabbitMq:Password", "guest"));
            });

            configuration.ReceiveEndpoint("search-auction-created", e => 
            {
                e.UseMessageRetry(r => r.Interval(5, 5));
                e.ConfigureConsumer<AuctionCreatedConsumer>(context);
            });
            configuration.ReceiveEndpoint("search-auction-updated", e => 
            {
                e.UseMessageRetry(r => r.Interval(5, 5));
                e.ConfigureConsumer<AuctionUpdatedConsumer>(context);
            });
            configuration.ReceiveEndpoint("search-auction-deleted", e => 
            {
                e.UseMessageRetry(r => r.Interval(5, 5));
                e.ConfigureConsumer<AuctionDeletedConsumer>(context);
            });

            configuration.ConfigureEndpoints(context);
        });
    });
    builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
}

var app = builder.Build();

{
    app.UseAuthorization();
    app.MapControllers();
}

app.Lifetime.ApplicationStarted.Register(async () => 
{
    try
    {
        await DbInitializer.InitDb(app);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
});

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetPolicy() 
    => HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));