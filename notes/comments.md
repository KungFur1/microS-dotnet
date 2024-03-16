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

**??? What is up with all the dependenncy injection methods, how do they work????**



## RabbitMQ

RabbitMQ - message broker, implements **AMQP** - accepts and forwards messages. Allows for asynchronous micro-service communication. Can use persistent storage. Should be used with some package for the client (micro-service). In .NET you can use the **rabbitMQ client**, but it is better to use **Mass Transit**, because it is easily interchangable with other message brokers that implement Advanced Message Queuing Protocol (**AMQP**), like **Azure Service Bus**.

```txt
                             --B KEY--> Queue -> ConsumerA
Producer --R KEY--> Exchange --B KEY--> Queue -> ConsumerB
                             --B KEY--> Queue -> ConsumerC
```

### Core Concepts
- **Producer** - emits messages to the **Exchange**.
- **Consumer** - receives messages from **Queue**.
- **Binding** - connects an exchange with a queue using **Binding Key**.
- Exchange compares **Routing Key** with the **Binding Key**.
- The **Producer** is responsible for setting up the **Exchange** and its type.
- The **Consumer** is responsible for setting up the **Queue** and binding it to the **Exchange** and also providing a **Binding Key**.

### Exchange Types
- **Fanout Exchange** - ignores the routing key and just sends the message to all Queues.
- **Direct Exchange** - sends the messages to where the routing key is exactly the same as binding key.
- **Topic Exchange** - allows partial matches of keys: "red.*" -> "red.green".
- **Header Exchange** - uses message header instead of the routing key.
- **Default (nameless) Exchang** (special to RabbitMQ) - compares the routing key to the queue name, but not the binding key.

### Mass Transit
- MassTransit requires that both the producer and consumer have their contract class in the same namespace.
- Events are something that already happened, so we name the contract classes as past: `AuctionCreated`.
- Mass transit is convention based and it will expect our consumer classes to have consumer at the end: `AuctionCreatedConsumer`.
- In case `RabbitMQ` container crashes or fails to receive message you can settup the **Outbox**, it will save the failed to send messages in your database and try to resend them periodically, this requires additional setup in your `DbContext` class.
- In case your Consumer suffers from **transient issues** (issues that you hope after some retries will succeed (a database fails and you hope it restarts)), you can add a retry policy in your consumer class.


#### Producer Example
`Program.cs`
```c#
builder.Services.AddMassTransit(x => 
{
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
```

`AuctionDbContext.cs` (optional - only for outbox)
```c#
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.AddInboxStateEntity();
    modelBuilder.AddOutboxMessageEntity();
    modelBuilder.AddOutboxStateEntity();
}
```

#### Consumer Example

`Program.cs`
```c#
builder.Services.AddMassTransit(x => 
{
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
    x.AddConsumersFromNamespaceContaining<AuctionUpdatedConsumer>();
    x.AddConsumersFromNamespaceContaining<AuctionDeletedConsumer>();

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));

    x.UsingRabbitMq((context, configuration) => 
    {
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
```

`AuctionCreatedConsumer.cs`
```c#
public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
    private readonly IMapper _mapper;
    public AuctionCreatedConsumer(IMapper mapper) {
        _mapper = mapper;
    }

    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        var item = _mapper.Map<Item>(context.Message);
        await item.SaveAsync();
    }
}
```

#### Contracts

You will also need a contract class, that will be used for sending and receiving messages. Its best to create a class library project and include it in your other projects.

`AuctionDeleted.cs`
```c#
namespace Contracts;

public class AuctionDeleted
{
    public string Id {get; set;}
}
```

### Fault Queues
Fault Queues are created automatically by Mass Transit and they contain the information about failed failed to consume messages, which throwed an exception in the middle of handling the message. Fault queues can also be consumed and they contain the messages that were failed to send, so you can change them and resend them or use them for logs or analysis.


## Identity Server
Authentication server, implements OpenID connect (OIDC) and OAuth 2.0

Requires a license.

Single Sign On solution - one login for all applications.

### Terminology
- Resource owner - user that uses your services and has access to his own data.
- Client - the application (frontend), that wants to perform actions on behalf of resource owner.
- Authorization Server - the application that knows the resource owner.
- Resouce Server - has data that the client wants to access. Resource Server trusts Authorization Server.
- Redirect URI - the link that Authorization Server will redirect to after loging in.
- Response Type - Code.
- Scope - the operations that the client wants access to, like: Read Auctions, Create Auction, Delete Auction, Read Profile...
- Consent Form - Do you want to allow microS-dotnet to access your profile, email etc?
- Client ID - identify the client with the authorization server (we will have two cleints: NextJsApp, Postman).
- Client Secret - is provided by the Authorization Server to the Client and is saved by both. (Client ID, Client Secret)
- Authorization Code - a short lived code that is sent to the client, which is then sent to the Authorization Server along with the Client Secret in exchange for the Access Token.
- Access Token - is used to access the Resource Server.

### OAuth 2.0 Flow
1. User opens the Client and goes to Login.
2. Client sends: Client ID, Redirect URI, Response Type, Scope to the Authorization Server.
3. The Authorization Server then verifies who you are and if necessary prompts for a login. (Maybe you already have a session on the identity server)
4. (Optional) Consent form, if your identity server is used for multiple applications, then maybe you need to verify that the user is ok with sharing his profile to other applications.
5. Temporary Authorization code is passed to the users browser, which then calls using the redirect URI to the client app server.
6. The client app then directly calls the Authorization Server and sends Client ID and Client Secret.
7. The Authorization Server then responds with the access token.
8. Finally, everytime when Client app is requesting data from the Resource Server it also sends the Access Token.


* Claim - a JWT field

## Gateway Service

Reverse Proxy - handles all incoming requests to the server and redistrubutes them to the appropriate micro-services.

- Very typical in Micro Services.
- Single surface area for requests.
- Client is unaware of any internal services.
- Security.
- SSL termination.
- URL rewriting.
- Load balancing.
- Caching.

### YARP

YARP - yet another reverse proxy. It is imported as a library into a standard .NET web api project. You then need to configure the routes in your configuration file.