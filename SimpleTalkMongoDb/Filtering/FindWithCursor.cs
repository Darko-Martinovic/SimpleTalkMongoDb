using MongoDB.Driver;
using SimpleTalkMongoDb.Configuration;
using SimpleTalkMongoDb.Pocos;
using System;
using System.Threading.Tasks;


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

            ConsoleEx.WriteLine("Let's find all documents that have : ", ConsoleColor.Cyan);
            ConsoleEx.WriteLine("\tTerritoryId equals to 1  and", ConsoleColor.Red);
            ConsoleEx.WriteLine("\tSalesPersonId equals to 283 and  ", ConsoleColor.Red);
            ConsoleEx.WriteLine("\tTotal Due greater than 90000.  ", ConsoleColor.Red);
            ConsoleEx.WriteLine("\tLimit the result to only the first five documents", ConsoleColor.Red);
            ConsoleEx.WriteLine("\tThen sort the result by Due Date,", ConsoleColor.Red);
            ConsoleEx.WriteLine("\tand project to display only SalesOrderId, DueDate and AccountNumber", ConsoleColor.Red);

            await UsingCursor(SampleConfig.Collection);

            // force to live all documents in memory!
            await UsingList(SampleConfig.Collection);

            // using for-each async
            await UsingForEachAsync(SampleConfig.Collection);


            Console.WriteLine("We could get the same result in SSMS by issuing the following query");
            ConsoleEx.WriteLine("SELECT TOP 5     " +
                                "SalesOrderID   ,DueDate   ,AccountNumber " +
                                "FROM sales.SalesOrderHeader " +
                                "WHERE TerritoryID = 1 AND SalesPersonID = 283 " +
                                "AND TotalDue > 90000  ORDER BY DueDate; ");

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
            using (var cursor = collection.Find(
                    x => x.TerritoryId == 1 &&
                         x.SalesPersonId == 283 &&
                         x.TotalDue > 90000
                    , fo)
                .Limit(TotalConst)
                .SortBy(x => x.DueDate)
                .Project(x => new { x.SalesOrderId, x.DueDate, x.AccountNumber })
                .ToCursorAsync())



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





            ConsoleEx.WriteLine($"Should be :{ batchNo } batches", ConsoleColor.Red);
        }

        private static async Task UsingList(IMongoCollection<SalesHeader> collection)
        {

            ConsoleEx.WriteLine("Using List", ConsoleColor.Magenta);
            Console.WriteLine("------------");
            var result = await collection.Find(
                    x => x.TerritoryId == 1 &&
                         x.SalesPersonId == 283 &&
                         x.TotalDue > 90000)
                .Limit(TotalConst)
                .SortBy(x => x.DueDate)
                .Project(x => new { x.SalesOrderId, x.DueDate, x.AccountNumber })
                .ToListAsync();

            foreach (var doc in result)
            {
                ConsoleEx.WriteLine(
                    $"\tSalesHeaderId :{doc.SalesOrderId}  DueDate       :{doc.DueDate.ToLocalTime()}  AccountNumber :{doc.AccountNumber}",
                    ConsoleColor.Yellow);
            }


        }

        private static async Task UsingForEachAsync(IMongoCollection<SalesHeader> collection)
        {
            ConsoleEx.WriteLine("Using ForEachAsync", ConsoleColor.Magenta);
            Console.WriteLine("------------");
            var cursor = collection.Find(
                    x => x.TerritoryId == 1 &&
                         x.SalesPersonId == 283 &&
                         x.TotalDue > 90000)
                .Limit(TotalConst)
                .SortBy(x => x.DueDate)
                .Project(x => new { x.SalesOrderId, x.DueDate, x.AccountNumber });

            //var query = cursor.ToString();

            await cursor.ForEachAsync(doc =>
                ConsoleEx.WriteLine(
                    $"\tSalesHeaderId :{doc.SalesOrderId}  DueDate       :{doc.DueDate.ToLocalTime()}  AccountNumber :{doc.AccountNumber}",
                    ConsoleColor.Yellow)

            );
        }
    }
}
