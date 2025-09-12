using api_raspi_web.Models;
using Microsoft.EntityFrameworkCore;

namespace api_raspi_web.Contexts
{
    public class RaspidbContext : DbContext
    {
        public RaspidbContext(DbContextOptions<RaspidbContext> options) : base(options)
        {
        }
        public DbSet<Balance> Balance { get; set; }
        public DbSet<Item> Item { get; set; }
        public DbSet<CanBalance> CanBalance { get; set; }
        public DbSet<CanItem> CanItem { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Balance>(entity =>
            {
                entity.HasKey(e => e.BalanceId);
                entity.HasData(
                    new Balance { BalanceId = 1, Total = 15941.40m },
                    new Balance { BalanceId = 2, Total = 3527.96m }, 
                    new Balance { BalanceId = 3, Total = 2807.96m }
                );
            });

            modelBuilder.Entity<CanBalance>(entity =>
            {
                entity.HasKey(e => e.CanBalanceId);
                entity.HasData(
                    new CanBalance { CanBalanceId = 1, Total = 500m },
                    new CanBalance { CanBalanceId = 2, Total = 499.99m }

                );
            });

            modelBuilder.Entity<Item>(entity =>
            {
                entity.HasKey(e => e.ItemId);
                entity.Property(e => e.Description).HasMaxLength(500);

                entity.HasData(
                    new Item { ItemId = 1, Price = 12413.44m, Description = "Beemden" },
                    new Item { ItemId = 2, Price = 720.00m, Description = "Photographer Ksenia" }
                );
            });
            modelBuilder.Entity<CanItem>(entity =>
            {
                entity.HasKey(e => e.CanItemId);
                entity.Property(e => e.Description).HasMaxLength(500);

                entity.HasData(
                    new CanItem { CanItemId = 1, Price = 0.01m, Description = "Test" }
                    
                );
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.TransactionId);
                entity.HasOne(t => t.Item)
                      .WithMany()
                      .HasForeignKey(t => t.ItemId);

                entity.HasOne(t => t.Balance)
                      .WithMany()
                      .HasForeignKey(t => t.BalanceId);

                entity.HasData(
                    new Transaction
                    {
                        TransactionId = 1,
                        ItemId = 1,
                        BalanceId = 2,  // after item 1 was applied
                        TransactionDate = new DateTime(2025, 05, 17, 0, 0, 0, DateTimeKind.Utc)
                    },
                    new Transaction
                    {
                        TransactionId = 2,
                        ItemId = 2,
                        BalanceId = 3,  // after item 2 was applied
                        TransactionDate = new DateTime(2025, 05, 13, 0, 0, 0, DateTimeKind.Utc)
                    }
                );
            });

            modelBuilder.Entity<CanTransaction>(entity =>
            {
                entity.HasKey(e => e.CanTransactionId);
                entity.HasOne(t => t.CanItem)
                      .WithMany()
                      .HasForeignKey(t => t.CanItemId);

                entity.HasOne(t => t.CanBalance)
                      .WithMany()
                      .HasForeignKey(t => t.CanBalanceId);

                entity.HasData(
                    new CanTransaction
                    {
                        CanTransactionId = 1,
                        CanItemId = 1,
                        CanBalanceId = 2,  // after item 1 was applied
                        TransactionDate = new DateTime(2025, 05, 17, 0, 0, 0, DateTimeKind.Utc)
                    }
                    
                );

            });
        }
    }
}
