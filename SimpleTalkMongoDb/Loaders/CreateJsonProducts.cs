using System;
using System.Collections.Generic;
using System.Data;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using SimpleTalkMongoDb.Configuration;
using SimpleTalkMongoDb.Pocos;

namespace SimpleTalkMongoDb.Loaders
{
    public static class CreateJsonProducts
    {
        public static Task RunAsync()
        {
            var t1 = DateTime.Now;
            Worker();
            var ts = DateTime.Now - t1;
            Console.WriteLine($"Total time elapsed {ts.Hours}:{ts.Minutes}:{ts.Seconds}");
            return Task.CompletedTask;
        }
        private static void Worker()

        {
            var ds = Common.GetDataSet(SampleConfig.TsqlProducts);


            var counter = ds.Tables[0].Rows.Count;
            var k = 1;

            var result = new List<Product>();

            foreach (DataRow r in ds.Tables[0].Rows)
            {
                Console.WriteLine($"Processing record {k}/{counter}");
                var i = ProductPoco(r);

                k++;
                result.Add(i);
            }
            var ws = new JsonWriterSettings
            {
                OutputMode = JsonOutputMode.RelaxedExtendedJson,
                Indent = true,
                IndentChars = "\t"

            };
            System.IO.File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "Product.json"), result.ToJson(ws));
        }

        private static Product ProductPoco(DataRow r)
        {

            var i = new Product()
            {
                ProductId = (int)r["ProductID"],
                ProductName = r["Name"].ToString() ?? "",
                SubCategoryName= r["SubKategoryName"].ToString() ?? "",
                Category = r["CategoryName"].ToString() ?? ""
            };

            return i;


        }
    }
}
