using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace SearchService;

public class AuctionUpdatedConsumer : IConsumer<AuctionUpdated>
{
    private readonly IMapper _mapper;

    public AuctionUpdatedConsumer(IMapper mapper)
    {
        _mapper = mapper;
    }
    public async Task Consume(ConsumeContext<AuctionUpdated> context)
    {
        Console.WriteLine("--> Consuming Auction Updated: " + context.Message.Id);
        System.Console.WriteLine("Info: " + context.Message.Model);

        Item item = _mapper.Map<Item>(context.Message);
        System.Console.WriteLine("Info: " + item.Model);
        var result = await DB.Update<Item>()
            .MatchID(context.Message.Id)
            .ModifyOnly(x => new
            {
                x.Color,
                x.Make,
                x.Model,
                x.Year,
                x.Mileage
            }, item)
            .ExecuteAsync();
        if (!result.IsAcknowledged)
            throw new MessageException(typeof(AuctionUpdated), "Problem updating mongodb");
    }
}
