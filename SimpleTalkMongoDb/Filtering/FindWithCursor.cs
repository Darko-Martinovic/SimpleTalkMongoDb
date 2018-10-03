using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using SimpleTalkMongoDb.Configuration;
using SimpleTalkMongoDb.Pocos;


namespace SimpleTalkMongoDb.Filtering
{
    internal static class FindWithCursor
    {
        private const int TotalConst = 5;
        private const int BatchSizeConst = 2;

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
            await UsingCursor(SampleConfig.Collection);

            // force to live all documents in memory!
            await UsingList(SampleConfig.Collection);

            // using for-each async
            await UsingForEachAsync(SampleConfig.Collection);
        }

        private static async Task UsingForEachAsync(IMongoCollection<SalesHeader> collection)
        {
            ConsoleEx.WriteLine("Using ForEachAsync", ConsoleColor.Magenta);
            Console.WriteLine("------------");
            var cursor = collection.Find(x => x.TerritoryId == 1)
                .Limit(TotalConst)
                .SortBy(x => x.DueDate)
                .Project(x => new {x.SalesOrderId, x.DueDate, x.AccountNumber});

             var query = cursor.ToString();

               await cursor.ForEachAsync(doc =>
                        ConsoleEx.WriteLine(
                            $"\tSalesHeaderId :{doc.SalesOrderId}  DueDate       :{doc.DueDate.ToLocalTime()}  AccountNumber :{doc.AccountNumber}",
                            ConsoleColor.Yellow)

                    );
        }

        private static async Task UsingList(IMongoCollection<SalesHeader> collection)
        {

            ConsoleEx.WriteLine("Using List", ConsoleColor.Magenta);
            Console.WriteLine("------------");
            var result = await collection.Find(x => x.TerritoryId==1)
                .Limit(TotalConst)
                .SortBy(x=>x.DueDate)
                .Project(x => new { x.SalesOrderId, x.DueDate, x.AccountNumber })
                .ToListAsync();

            foreach (var doc in result)
            {
                ConsoleEx.WriteLine(
                    $"\tSalesHeaderId :{doc.SalesOrderId}  DueDate       :{doc.DueDate.ToLocalTime()}  AccountNumber :{doc.AccountNumber}",
                    ConsoleColor.Yellow);
            }


        }

        private static async Task UsingCursor(IMongoCollection<SalesHeader> collection)
        {
            ConsoleEx.WriteLine("Using Cursor", ConsoleColor.Magenta);
            Console.WriteLine("------------");
            var fo = new FindOptions
            {
                BatchSize = BatchSizeConst,
                
            };
            var batchNo = 0;
            using (var cursor = collection.Find(x => x.TerritoryId ==1, fo)
                .Limit(TotalConst)
                .SortBy(x => x.DueDate)
                .Project(x => new { x.SalesOrderId, x.DueDate, x.AccountNumber })
                .ToCursorAsync())
            {


                while (await cursor.Result.MoveNextAsync())
                {
                    ConsoleEx.WriteLine($"Batch no: {++batchNo}", ConsoleColor.Red);
                    Console.WriteLine("-------------------------------------------");
                    foreach (var doc in cursor.Result.Current)
                    {
                        ConsoleEx.WriteLine(
                            $"\tSalesHeaderId :{doc.SalesOrderId}  DueDate       :{doc.DueDate.ToLocalTime()}  AccountNumber :{doc.AccountNumber}",
                            ConsoleColor.Yellow);

                    }

                }


            }


           ConsoleEx.WriteLine($"Should be :{ batchNo } batches",ConsoleColor.Red);
        }
    }
}
