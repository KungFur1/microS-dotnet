using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace SearchService;

public class AuctionDeletedConsumer : IConsumer<AuctionDeleted>
{
    public async Task Consume(ConsumeContext<AuctionDeleted> context)
    {
        Console.WriteLine("--> Consuming Auction Deleted: " + context.Message.Id);
        
        string id = context.Message.Id;
        var result = await DB.DeleteAsync<Item>(id);
        if (!result.IsAcknowledged)
            throw new MessageException(typeof(AuctionUpdated), "Problem deleting item");
    }
}
