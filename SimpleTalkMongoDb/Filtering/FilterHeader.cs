using MongoDB.Driver;
using MongoDB.Driver.Linq;

using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using SimpleTalkMongoDb.Configuration;
using SimpleTalkMongoDb.Pocos;

namespace SimpleTalkMongoDb.Filtering
{
    // ReSharper disable once UnusedMember.Global
    internal static class FilterHeader
    {
        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once UnusedParameter.Global
        public static void Main(string[] args)
        {
            MainAsync().Wait();
            Console.ReadLine();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static async Task MainAsync()
        {

            await UsingMongoAggFrameWork(SampleConfig.Collection);
            await UsingLinq(SampleConfig.Collection);
        }


        /// <summary>
        /// Using LINQ
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        private static async Task UsingLinq(IMongoCollection<SalesHeader> collection)
        {
// LINQ

            var list2 = await (collection.AsQueryable()
                    .Where(z => (z.TerritoryId == 1 || z.TerritoryId == 2) && z.TotalDue > Limit &&
                                z.DueDate >= new DateTime(2014, 1, 1))
                    .Select(z => new
                    {
                        z.SalesOrderId,
                        z.CustomerId,
                        z.TotalDue,
                        z.OrderDate
                    })
                    .OrderByDescending(x => x.TotalDue)
                    .ThenByDescending(x => x.OrderDate))
                .Take(3)
                .ToListAsync();


            ConsoleEx.WriteLine("SalesOrderId CustomerId TotalDue   OrderDate", ConsoleColor.Magenta);
            Console.WriteLine("------------------------------------------------------");

            foreach (var e in list2)
            {
                Console.WriteLine($"{e.SalesOrderId.ToString().PadRight(SalesOrderId.Length)} " +
                                  $"{e.CustomerId.ToString().PadRight(CustomerId.Length)} " +
                                  $"{e.TotalDue:N2} " +
                                  $"{e.OrderDate.ToLocalTime():dd.MM.yyyyy}", ConsoleColor.Yellow);
            }
        }


        /// <summary>
        /// Using IFindFluent
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        private static async Task UsingMongoAggFrameWork(IMongoCollection<SalesHeader> collection)
        {

            var query =  collection.Find(x =>
                    (x.TerritoryId == 1 ||
                     x.TerritoryId == 2) && x.TotalDue > Limit && x.DueDate >= new DateTime(2014, 1, 1))
                .Project(x => new
                {
                    x.SalesOrderId,
                    x.CustomerId,
                    x.TotalDue,
                    x.OrderDate
                })
                .SortByDescending(x => x.TotalDue)
                .ThenByDescending(x => x.OrderDate)
                .Limit(3);

            var queryResult = query.ToString();
            var list = await query.ToListAsync();





            ConsoleEx.WriteLine("Using MongoDB aggregation framework");
            Console.WriteLine();
            ConsoleEx.WriteLine("SalesOrderId CustomerId TotalDue   OrderDate", ConsoleColor.Magenta);
            Console.WriteLine("------------------------------------------------------");

            foreach (var e in list)
            {
                Console.WriteLine($"{e.SalesOrderId.ToString().PadRight(SalesOrderId.Length)} " +
                                  $"{e.CustomerId.ToString().PadRight(CustomerId.Length)} " +
                                  $"{e.TotalDue:N2} " +
                                  $"{e.OrderDate.ToLocalTime():dd.MM.yyyy}", ConsoleColor.Yellow);
            }


            Console.WriteLine();
            ConsoleEx.WriteLine("Using MongoDB LINQ");
            Console.WriteLine();
        }

        private const string SalesOrderId = "SalesOrderId";
        private const string CustomerId = "CustomerId";
        private const decimal Limit = 10000;


    }
}