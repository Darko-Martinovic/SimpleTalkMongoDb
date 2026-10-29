using System.Text.RegularExpressions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.EntityFrameworkCore.Diagnostics;
using MongoDB.EntityFrameworkCore.Extensions;

using SimpleTalkMongoDb.Configuration;

namespace SimpleTalkMongoDb.Modern
{
    /// <summary>
    /// The MongoDB provider for Entity Framework Core (generally available since 2024): the CRUD of
    /// CrudDemo through a DbContext, with LINQ queries, change tracking and SaveChanges, which
    /// sends only the fields that changed. The provider logs the MQL it runs.
    /// </summary>
    internal static class EfCoreProvider
    {
        private const string CollectionName = "efOffers";

        public static async Task RunAsync()
        {
            // A copy of the special offers to work on
            var db = SampleConfig.Db;
            await db.DropCollectionAsync(CollectionName);
            await db.GetCollection<BsonDocument>(CollectionName).InsertManyAsync(
                await db.GetCollection<BsonDocument>(SampleConfig.CollSpecialOffer.CollectionNamespace.CollectionName)
                    .Find(FilterDefinition<BsonDocument>.Empty).ToListAsync());

            ConsoleEx.WriteLine("Entity Framework Core with the MongoDB provider", ConsoleColor.Magenta);
            await using var context = new OffersContext();

            // Read: LINQ, translated to MQL
            var resellers = await context.Offers
                .Where(o => o.Category == "Reseller" && o.Type == "Volume Discount")
                .OrderBy(o => o.Id)
                .ToListAsync();
            ConsoleEx.WriteLine($"Reseller volume discounts: {string.Join(", ", resellers.Select(o => $"#{o.Id} {o.Description}"))}", ConsoleColor.Yellow);

            // Create
            context.Offers.Add(new Offer { Id = 50, Description = "EF Core launch offer", Type = "New Product", Category = "Customer" });
            await context.SaveChangesAsync();
            ConsoleEx.WriteLine("Added offer #50", ConsoleColor.Yellow);

            // Update: change a tracked entity; SaveChanges sets only the changed field
            var offer = await context.Offers.SingleAsync(o => o.Id == 50);
            offer.Description = "EF Core launch offer, extended";
            await context.SaveChangesAsync();
            ConsoleEx.WriteLine($"Updated #50: {(await db.GetCollection<BsonDocument>(CollectionName).Find(new BsonDocument("_id", 50)).FirstAsync())["description"]}", ConsoleColor.Yellow);

            // Delete
            context.Offers.Remove(offer);
            await context.SaveChangesAsync();
            ConsoleEx.WriteLine($"Removed #50; offers left: {await context.Offers.CountAsync()}", ConsoleColor.Yellow);
        }

        public sealed class Offer
        {
            public int Id { get; set; }
            public string Description { get; set; } = "";
            public string Type { get; set; } = "";
            public string Category { get; set; } = "";
        }

        private sealed class OffersContext : DbContext
        {
            public DbSet<Offer> Offers => Set<Offer>();

            protected override void OnConfiguring(DbContextOptionsBuilder options) =>
                options
                    .UseMongoDB(SampleConfig.Client, SampleConfig.Db.DatabaseNamespace.DatabaseName)
                    // Show the MQL the provider runs for each query, with its values (a demo can log them)
                    .LogTo(
                        // The message's first line is the log header; the rest is what was run
                        message => ConsoleEx.WriteLine("  " + Regex.Replace(message.Split('\n', 2)[^1], @"\s+", " ").Trim(), ConsoleColor.DarkGray),
                        [MongoEventId.ExecutedMqlQuery, MongoEventId.ExecutedBulkWrite],
                        LogLevel.Information)
                    .EnableSensitiveDataLogging();

            protected override void OnModelCreating(ModelBuilder model)
            {
                // The same document shape as the special offers: the element names differ from the properties
                model.Entity<Offer>(offer =>
                {
                    offer.ToCollection(CollectionName);
                    offer.Property(o => o.Description).HasElementName("description");
                    offer.Property(o => o.Type).HasElementName("offerType");
                    offer.Property(o => o.Category).HasElementName("category");
                });
            }
        }
    }
}
