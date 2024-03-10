using AuctionService.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data;


public class AuctionDbContext : DbContext
{
    public AuctionDbContext(DbContextOptions options) : base(options)
    {
        
    }

    public DbSet<Auction> Auctions {get; set;} = null!;
    public DbSet<Item> Items {get; set;} = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}