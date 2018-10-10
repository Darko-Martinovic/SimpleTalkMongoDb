using MongoDB.Driver;

using System;
using System.Threading.Tasks;
using SimpleTalkMongoDb.Configuration;
using SimpleTalkMongoDb.Pocos;

namespace SimpleTalkMongoDb.Aggregation
{
    internal static class SampleUnwind
    {


        public static void Main()
        {
            MainAsync().Wait();
            Console.ReadLine();
        }

      
        private static async Task MainAsync()
        {
           
          
            var collection = SampleConfig.Collection;


            var result = await collection.Aggregate().Limit(1)
                .Unwind<SalesHeader, SalesDetailHelper>(x => x.Details)
                .Project(x=>new {x.Details.SalesOrderDetailId})
                .ToListAsync();
               

            foreach (var example in result)
            {
                Console.WriteLine($"{example.SalesOrderDetailId.ToString().PadRight(10)}");
            }

        }

     
    }
}
