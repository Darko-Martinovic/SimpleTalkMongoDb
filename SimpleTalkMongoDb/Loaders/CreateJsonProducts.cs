using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using SimpleTalkMongoDb.Configuration;
using SimpleTalkMongoDb.Pocos;

namespace SimpleTalkMongoDb.Loaders
{
    public static class CreateJsonProducts
    {
        public static void Main(string[] args)
        {
            var t1 = DateTime.Now;
            Worker();
            var ts = DateTime.Now - t1;
            Console.WriteLine($"Total time elapsed {ts.Hours}:{ts.Minutes}:{ts.Seconds}");
            Console.WriteLine("Press any key to exit!");
            Console.ReadLine();
        }
        private static void Worker()

        {
            var ds = GetDataSet();


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
                OutputMode = JsonOutputMode.Strict,
                Indent = true,
                IndentChars = "\t"

            };
            System.IO.File.WriteAllText(@"C:\tmp\Product.json", result.ToJson(ws));
        }

        private static Product ProductPoco(DataRow r)
        {

            var i = new Product()
            {
                ProductId = (int)r["ProductID"],
                ProductName = r["Name"].ToString(),
                SubCategoryName= r["SubKategoryName"].ToString(),
                Category = r["CategoryName"].ToString()
            };

            return i;


        }

        private static DataSet GetDataSet()
        {
            var ds = new DataSet();

            var sql = SampleConfig.TsqlProducts;

            using (var cnn = new SqlConnection(SampleConfig.ConnectionString))
            {
                try
                {
                    cnn.Open();
                    using (var cmd = new SqlCommand(sql, cnn))
                    {
                        using (var a = new SqlDataAdapter(cmd))
                        {
                            a.Fill(ds);
                        }
                    }

                    cnn.Close();
                }
                catch (Exception ex)
                {
                    if (Debugger.IsAttached)
                        Debugger.Break();
                    Console.WriteLine($"Exception : {ex.Message}");
                    Console.ReadLine();
                }
            }

            return ds;
        }

    }
}
