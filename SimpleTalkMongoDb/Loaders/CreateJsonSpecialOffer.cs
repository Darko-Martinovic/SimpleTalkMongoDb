using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using SimpleTalkMongoDb.Configuration;
using SimpleTalkMongoDb.Pocos;

namespace SimpleTalkMongoDb.Loaders
{
    public class CreateJsonSpecialOffer
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
            var ds = Common.GetDataSet(SampleConfig.TsqlSpecialOffers);


            var counter = ds.Tables[0].Rows.Count;
            var k = 1;

            var result = new List<SpecialOffer>();

            foreach (DataRow r in ds.Tables[0].Rows)
            {
                Console.WriteLine($"Processing record {k}/{counter}");
                var i = SpecialOfferPoco(r);

                k++;
                result.Add(i);
            }
            var ws = new JsonWriterSettings
            {
                OutputMode = JsonOutputMode.RelaxedExtendedJson,
                Indent = true,
                IndentChars = "\t",
            };
            File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "SpecialOffer.json"), result.ToJson(ws));
        }

        private static SpecialOffer SpecialOfferPoco(DataRow r)
        {

            var i = new SpecialOffer
            {
                SpecialOfferId = (int)r["SpecialOfferID"],
                Type = r["Type"].ToString() ?? "",
                Category = r["Category"].ToString() ?? "",
                Description = r["Description"].ToString() ?? ""
            };

            return i;


        }


    }
}
