using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using SimpleTalkMongoDb.Pocos;

namespace SimpleTalkMongoDb.Loaders
{
    public class CreateJsonSpetialOffer
    {

        public static void Main(string[] args)
        {
            var t1 = DateTime.Now;
            Worker();
            var ts = DateTime.Now - t1;
            Console.WriteLine($"Total time elapsed {ts.Hours}:{ts.Minutes}:{ts.Seconds}");
            Console.WriteLine("Press any key to exit!");
            //Worker2();
            Console.ReadLine();
        }
        private static void Worker()

        {
            var ds = Common.GetDataSet();


            var counter = ds.Tables[0].Rows.Count;
            var k = 1;

            var result = new List<SpetialOffer>();

            foreach (DataRow r in ds.Tables[0].Rows)
            {
                Console.WriteLine($"Processing record {k}/{counter}");
                var i = SpetialOfferPoco(r);

                k++;
                result.Add(i);
            }
            var ws = new JsonWriterSettings
            {
                OutputMode = JsonOutputMode.Strict,
                Indent = true,
                IndentChars = "\t",
                //NewLineChars = "\r\n", default
                GuidRepresentation = GuidRepresentation.CSharpLegacy

            };
            File.WriteAllText(@"C:\tmp\SpetailOffer.json", result.ToJson(ws));
        }

        private static SpetialOffer SpetialOfferPoco(DataRow r)
        {

            var i = new SpetialOffer
            {
                SpecialOfferId = (int)r["SpecialOfferID"],
                Type = r["Type"].ToString(),
                Category = r["Category"].ToString(),
                Description = r["Description"].ToString()
            };

            return i;


        }


    }
}
