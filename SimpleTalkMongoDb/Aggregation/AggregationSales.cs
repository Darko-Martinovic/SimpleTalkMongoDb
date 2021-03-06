﻿using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

using SimpleTalkMongoDb.Configuration;
using SimpleTalkMongoDb.Pocos;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleTalkMongoDb.Aggregation
{
    // ReSharper disable once UnusedMember.Global
    internal static class AggregationSales
    {
        /// <summary>
        /// The example shows various way of aggregation.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static void Main()
        {
            MainAsync().Wait();
            Console.ReadLine();
        }


        /// <summary>
        /// How to accomplished aggregation by using IAggregateFluent, LINQ & 'MongoShellLikeSyntax'
        /// </summary>
        /// <returns></returns>
        private static async Task MainAsync()
        {


            await UsingIAggregateFluent(SampleConfig.Collection);

            await UsingLinq(SampleConfig.Collection);

            await UsingMongoShellLikeSyntax();
        }


        /// <summary>
        /// Using IAggregateFluent
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        private static async Task UsingIAggregateFluent(IMongoCollection<SalesHeader> collection)
        {

            var query = collection.Aggregate()
                .Group(x => new { x.TerritoryId, x.CustomerId },
                    g => new { TerritoryIdCustomerId = g.Key, TotalDue = g.Sum(x => x.TotalDue) })
                .SortBy(x => x.TotalDue)
                .Group(x => x.TerritoryIdCustomerId.TerritoryId, g => new
                {
                    TerritoryId = g.Key,
                    MaxCustomer = g.Last().TerritoryIdCustomerId.CustomerId,
                    MaxTotal = g.Last().TotalDue
                })
                .Match(x => x.MaxTotal > Limit)
                .Project(x => new
                {
                    x.TerritoryId,
                    MaxCust = new { Id = x.MaxCustomer, Total = x.MaxTotal },
                })
                .SortByDescending(x => x.MaxCust.Total);

            //var queryToExplain = query.ToString();

            var result = await query.ToListAsync();
            // Display the result
            ConsoleEx.WriteLine("\tUsing MongoDB IAggregateFluent", ConsoleColor.Magenta);

            ConsoleEx.WriteLine($"{TerId} {MaxCustoId} {MaxTotal}", ConsoleColor.Gray);
            foreach (var d in result)
            {
                //Console.WriteLine(d.ToJson(jsonWritter));

                ConsoleEx.WriteLine(
                    $"{d.TerritoryId.ToString().PadRight(TerId.Length)} {d.MaxCust.Id.ToString().PadRight(MaxCustoId.Length)} " +
                    $"{d.MaxCust.Total.ToString("n2").PadLeft(MaxTotal.Length)}", ConsoleColor.Yellow);
            }
        }


        /// <summary>
        /// Using LINQ
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        private static async Task UsingLinq(IMongoCollection<SalesHeader> collection)
        {
            // USING LINQ 
            ConsoleEx.WriteLine("\tUsing LINQ", ConsoleColor.Magenta);

            var pipeline = from o in
                               from z in collection.AsQueryable()
                               group z by new { z.TerritoryId, z.CustomerId }
                    into g
                               select new { TerritoryIdPlusCustomerId = g.Key, TotalDue = g.Sum(x => x.TotalDue) }
                           orderby o.TotalDue
                           group o by o.TerritoryIdPlusCustomerId.TerritoryId
                into g
                           orderby g.Key
                           select new
                           {
                               TerritoryId = g.Key,
                               MaxCustomer = new
                               { Name = g.Last().TerritoryIdPlusCustomerId.CustomerId, g.Last().TotalDue }
                           };
            // filter by limit and apply descaning order 
            pipeline = pipeline.Where(x => x.MaxCustomer.TotalDue > Limit).OrderByDescending(x => x.MaxCustomer.TotalDue);


            var pipeline2 = collection.AsQueryable()
                .GroupBy(z => new { z.TerritoryId, z.CustomerId })
                .Select(g => new { TerritoryIdPlusCustomerId = g.Key, TotalDue = g.Sum(x => x.TotalDue) })
                .OrderBy(o => o.TotalDue)
                .GroupBy(o => o.TerritoryIdPlusCustomerId.TerritoryId)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    TerritoryId = g.Key,
                    MaxCustomer = new { Name = g.Last().TerritoryIdPlusCustomerId.CustomerId, g.Last().TotalDue }
                })
                .Where(x => x.MaxCustomer.TotalDue > Limit)
                .OrderByDescending(x => x.MaxCustomer.TotalDue);


            //Console.WriteLine("---------------Pipeline --------------------");
            //Console.WriteLine(pipeline.ToString());
            //Console.WriteLine("--------------------------------------------");

            var result2 = await pipeline2.ToListAsync();
            Console.WriteLine($"{TerId} {MaxCustoId} {MaxTotal}");
            foreach (var d in result2)
            {
                ConsoleEx.WriteLine(
                    $"{d.TerritoryId.ToString().PadRight(TerId.Length)} {d.MaxCustomer.Name.ToString().PadRight(MaxCustoId.Length)} " +
                    $"{d.MaxCustomer.TotalDue.ToString("n2").PadLeft(MaxTotal.Length)}", ConsoleColor.Yellow);
            }
        }


        /// <summary>
        /// Using MongoDB shell like syntax
        /// </summary>
        /// <returns></returns>
        private static async Task UsingMongoShellLikeSyntax()
        {
            var group = new BsonDocument
            {
                {
                    "$group",
                    new BsonDocument
                    {
                        {
                            "_id", new BsonDocument
                            {
                                {"Territory", "$TerritoryId"},
                                {"Customer", "$CustomerId"},
                            }
                        },
                        {
                            "Total", new BsonDocument
                            {
                                {"$sum", "$TotalDue"}
                            }
                        }
                    }
                }
            };
            var sort = new BsonDocument
            {
                {
                    "$sort",
                    new BsonDocument
                    {
                        {"Total", 1}
                    }
                }
            };
            var group2 = new BsonDocument
            {
                {
                    "$group",
                    new BsonDocument
                    {
                        {
                            "_id", new BsonDocument
                            {
                                {"Territory", "$_id.Territory"},
                            }
                        },
                        {
                            "Total", new BsonDocument
                            {
                                {"$last", "$Total"},
                            }
                        },
                        {
                            "MaxCustomer", new BsonDocument
                            {
                                {"$last", "$_id.Customer"},
                            }
                        }
                    }
                }
            };
            var sort2 = new BsonDocument
            {
                {
                    "$sort",
                    new BsonDocument
                    {
                        {"Total", -1}
                    }
                }
            };
            var project = new BsonDocument
            {
                {
                    "$project",
                    new BsonDocument
                    {
                        {"_id", 0},
                        {"TerritoryId", "$_id.Territory"},
                        {"Customer", "$MaxCustomer"},
                        {"Total", "$Total"},
                    }
                }
            };
            var limit = new BsonDocument
            {
                {
                    "$limit", 3
                }
            };


            var pipelineLast = new[] { group, sort, group2, sort2, limit, project, };


            var collectionLast = SampleConfig.Collection;


            var resultLast = collectionLast.Aggregate<BsonDocument>(pipelineLast);

            var matchingExamples = await resultLast.ToListAsync();

            ConsoleEx.WriteLine("\tUsing MongoDB shell like coding - parsing BSonDocument", ConsoleColor.Magenta);

            Console.WriteLine($"{TerId} {MaxCustoId} {MaxTotal}");

            foreach (var example in matchingExamples)
            {
                ConsoleEx.WriteLine(
                    $"{example.GetElement("TerritoryId").Value.ToString().PadRight(TerId.Length)} {example.GetElement("Customer").Value.ToString().PadRight(MaxCustoId.Length)} " +
                    $"{((decimal)example.GetElement("Total").Value):N2}", ConsoleColor.Yellow);
            }

            //
        }





        private const string TerId = "Territory ID";
        private const string MaxCustoId = "MaxCustId";
        private const string MaxTotal = "MaxTotal";
        private const decimal Limit = 950000;

    }
}