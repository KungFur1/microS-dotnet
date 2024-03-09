using AuctionService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data;


public class AuctionDbContext : DbContext
{
    public AuctionDbContext(DbContextOptions options) : base(options)
    {
        
    }

    public DbSet<Auction> Auctions {get; set;} = null!;
    public DbSet<Item> Items {get; set;} = null!;
}