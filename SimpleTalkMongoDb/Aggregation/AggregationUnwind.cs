using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

using SimpleTalkMongoDb.Configuration;
using SimpleTalkMongoDb.Pocos;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleTalkMongoDb.Aggregation
{
    internal static class AggregationUnwind
    {
        public static void Main()
        {
            MainAsync().Wait();
            Console.ReadLine();
        }
        private static async Task MainAsync()
        {
            var collection = SetupClient();

            //Using LINQ
            await UsingLinqChain(collection);

            //Using LINQ with Lookup
            await UsingLinqChainWithLookup(collection);

            //Using MongoDB Aggregation framework
            await UsingMongoAggFrameWork(collection);

            //Using MongoDB Aggregation framework with Lookup
            await UsingMongoAggFrameWorkWithLookUp(collection);

            //Using shell like syntax
            await UsingShellLikeSyntax(collection);
        }

        private static IMongoCollection<SalesHeader> SetupClient()
        {
            return SampleConfig.Collection;
        }

        private static async Task UsingShellLikeSyntax(IMongoCollection<SalesHeader> collection)
        {
            var result = await collection.Aggregate()
                .Unwind(x => x.Details)
                .Group(new BsonDocument
                {
                    {
                        "_id", new BsonDocument
                        {
                            {"SpecialOfferId", "$Details.SpecialOfferId"},
                            {"ProductId", "$Details.ProductId"},
                        }
                    },
                    {"Total", new BsonDocument("$sum", "$Details.LineTotal")}
                })
                .Sort(new BsonDocument("Total", -1))
                .Group(new BsonDocument
                {
                    {
                        "_id", new BsonDocument
                        {
                            {"SpecialOfferId", "$_id.SpecialOfferId"},
                        }
                    },
                    {
                        "Total", new BsonDocument
                        {
                            {"$first", "$Total"},
                        }
                    },
                    {
                        "MaxProduct", new BsonDocument
                        {
                            {"$first", "$_id.ProductId"},
                        }
                    }
                })
                .Sort(new BsonDocument("Total", -1))
                .Match(new BsonDocument("Total", new BsonDocument("$gte", Limit)))
                .Project(new BsonDocument
                {
                    {"_id", 0},
                    {"SpecialOfferId", "$_id.SpecialOfferId"},
                    {"Product", "$MaxProduct"},
                    {"Total", "$Total"},
                }).ToListAsync();


            ConsoleEx.WriteLine("\tUsing MongoDB shell like coding - parsing BSonDocument", ConsoleColor.Magenta);

            Console.WriteLine($"{SpetailOfferId} {MaxProductId} {MaxTotal}");
            foreach (var example in result)
            {
                //Console.WriteLine(d.ToJson(jsonWritter));
                ConsoleEx.WriteLine(
                    $"{example.GetElement("SpecialOfferId").Value.ToString().PadRight(SpetailOfferId.Length)} " +
                    $"{example.GetElement("Product").Value.ToString().PadLeft(MaxProductId.Length)} " +
                    $"{((decimal)example.GetElement("Total").Value):N2}", ConsoleColor.Yellow);
            }
        }

        private static async Task UsingMongoAggFrameWork(IMongoCollection<SalesHeader> collection)
        {
            var queryMongo = await collection.Aggregate()
                .Unwind<SalesHeader, SalesDetailHelper>((x) => x.Details)
                .Group(x => new { x.Details.SpecialOfferId, x.Details.ProductId },
                    g => new
                    {
                        SpecialOfferIdPlusProductId = g.Key,
                        LineTotal = g.Sum(x => x.Details.LineTotal)
                    })
                .SortByDescending(p => p.LineTotal)
                .Group(x => x.SpecialOfferIdPlusProductId.SpecialOfferId,
                    g => new
                    {
                        SpecialOfferId = g.Key,
                        Product = g.First().SpecialOfferIdPlusProductId.ProductId,
                        LineTotal = g.Max(x => x.LineTotal)
                    })
                .SortByDescending(p => p.LineTotal)
                .Match(x => x.LineTotal > Limit)
                .ToListAsync();


            ConsoleEx.WriteLine("\tUsing MongoDB Aggregation framework", ConsoleColor.Magenta);
            Console.WriteLine($"{SpetailOfferId} {MaxProductId} {MaxTotal}");

            foreach (var d in queryMongo)
            {
                //Console.WriteLine(d.ToJson(jsonWritter));

                ConsoleEx.WriteLine(
                    $"{d.SpecialOfferId.ToString().PadRight(SpetailOfferId.Length)}" +
                    $" {d.Product.ToString().PadLeft(MaxProductId.Length)} " +
                    $"{d.LineTotal:N2}", ConsoleColor.Yellow);
            }
        }


        private static async Task UsingMongoAggFrameWorkWithLookUp(IMongoCollection<SalesHeader> collection)
        {
            var otherCollection = SampleConfig.CollSpetialOffer;
            var prodColl = SampleConfig.CollProducts;

            var queryMongo = await collection.Aggregate()
                .Unwind<SalesHeader, SalesDetailHelper>(x => x.Details)
                .Group(x => new { x.Details.SpecialOfferId, x.Details.ProductId },
                    g => new
                    {
                        SpecialOfferIdPlusProductId = g.Key,
                        LineTotal = g.Sum(x => x.Details.LineTotal)
                    })
                .SortByDescending(p => p.LineTotal)
                .Group(x => x.SpecialOfferIdPlusProductId.SpecialOfferId,
                    g => new SalesDetail
                    {
                        SpecialOfferId = g.Key,
                        ProductId = g.First().SpecialOfferIdPlusProductId.ProductId,
                        LineTotal = g.Max(x => x.LineTotal)
                    })
                .SortByDescending(p => p.LineTotal)
                .Match(x => x.LineTotal > Limit)
                .Lookup<SalesDetail, SpetialOffer, Result>(otherCollection, x => x.SpecialOfferId, y => y.SpecialOfferId,
                    x => x.SpetOffer)
                .Lookup<Result, Product, ResultProduct>(prodColl, x => x.ProductId, y => y.ProductId,
                    x => x.ProductName
                ).ToListAsync();



            ConsoleEx.WriteLine("\tUsing MongoDB Aggregation framework with Lookup", ConsoleColor.Magenta);
            Console.WriteLine($"{SpetailOfferId} {Description} {MaxProductId} {MaxTotal}");

            foreach (var d in queryMongo)
            {
                ConsoleEx.WriteLine
                (
                    $"{d.Id.ToString().PadRight(SpetailOfferId.Length)} " +
                    $"{d.SpetOffer.ElementAt(0).Description.Trim().PadRight(Description.Length)} " +
                    $"{d.ProductId.ToString().PadRight(MaxProductId.Length)} " +
                    $"{d.ProductName.ElementAt(0).ProductName.Trim().PadRight(ProductName.Length)} " +
                    $"{d.LineTotal:N2}",
                    ConsoleColor.Yellow
                );
            }
        }


        private static async Task UsingLinqChain(IMongoCollection<SalesHeader> collection)
        {
            var query = await collection.AsQueryable()
                .SelectMany(p => p.Details, (p, salesDetail) => new
                {
                    salesDetail.SpecialOfferId,
                    salesDetail.ProductId,
                    salesDetail.LineTotal
                })
                .GroupBy(p => new { p.SpecialOfferId, p.ProductId })
                .Select(g => new
                {
                    Name = g.Key,
                    LineTotal = g.Sum(x => x.LineTotal)
                })
                .OrderByDescending(p => p.LineTotal)
                .GroupBy(p => p.Name.SpecialOfferId)
                .Select(g => new
                {
                    Name = g.Key,
                    Product = g.First().Name.ProductId,
                    LineTotal = g.Max(x => x.LineTotal),
                })
                .OrderByDescending(p => p.LineTotal)
                .Where(p => p.LineTotal >= Limit).ToListAsync();
            //.Select(p => new { p.Name, p.Product, p.LineTotal }).ToListAsync();


            ConsoleEx.WriteLine("\tUsing LINQ", ConsoleColor.Magenta);
            Console.WriteLine($"{SpetailOfferId} {MaxProductId} {MaxTotal}");

            foreach (var e in query)
            {
                ConsoleEx.WriteLine(
                    $"{e.Name.ToString().PadRight(SpetailOfferId.Length)}" +
                    $" {e.Product.ToString().PadLeft(MaxProductId.Length)} " +
                    $"{e.LineTotal:N2}", ConsoleColor.Yellow);
            }
        }


        private static async Task UsingLinqChainWithLookup(IMongoCollection<SalesHeader> collection)
        {
            var spetialOfferColl = SampleConfig.CollSpetialOffer;
            var productColl = SampleConfig.CollProducts;

            var query = await collection.AsQueryable()
                    .SelectMany(p => p.Details, (p, salesDetail) => new
                    {
                        salesDetail.SpecialOfferId,
                        salesDetail.ProductId,
                        salesDetail.LineTotal
                    })
                    .GroupBy(p => new { p.SpecialOfferId, p.ProductId })
                    .Select(g => new
                    {
                        Name = g.Key,
                        LineTotal = g.Sum(x => x.LineTotal)
                    })
                    .OrderByDescending(p => p.LineTotal)
                    .GroupBy(p => p.Name.SpecialOfferId)
                    .Select(g => new
                    {
                        Name = g.Key,
                        Product = g.First().Name.ProductId,
                        LineTotal = g.Max(x => x.LineTotal),
                    })
                .Join(spetialOfferColl, x => x.Name, y => y.SpecialOfferId, (x, y) => new { x.Name, y.Description, x.LineTotal, x.Product })
                .Join(productColl, x => x.Product, y => y.ProductId, (x, y) => new { x.Name,x.Description, y.ProductName, x.LineTotal, x.Product })
                     .OrderByDescending(p => p.LineTotal)
                     .Where(p => p.LineTotal >= Limit).ToListAsync();
            //.Select(p => new { p.Name, p.Product, p.LineTotal }).ToListAsync();


            ConsoleEx.WriteLine("\tUsing LINQ with Lookup", ConsoleColor.Magenta);
            Console.WriteLine($"{SpetailOfferId} {Description} {MaxProductId}{ProductName} {MaxTotal}");

            foreach (var e in query)
            {
                ConsoleEx.WriteLine(
                    $"{e.Name.ToString().PadRight(SpetailOfferId.Length)}" +
                    $"{e.Description.PadRight(Description.Length)}" +
                    $" {e.Product.ToString().PadLeft(MaxProductId.Length)} " +
                    $" {e.ProductName.PadLeft(ProductName.Length)} " +
                    $"{e.LineTotal:N2}", ConsoleColor.Yellow);
            }
        }

        private const string SpetailOfferId = "Offer ID";
        private const string MaxProductId = "Product ";
        private const string MaxTotal = "Total";
        private const string Description = "Spetial Offer Description - Lookup Example";
        private const string ProductName = "Product Name - Lookup Example ";
        private const decimal Limit = 200000;
    }


}
