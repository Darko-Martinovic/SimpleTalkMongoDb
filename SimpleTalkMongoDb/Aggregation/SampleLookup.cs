using MongoDB.Driver;

using System;
using System.Linq;
using System.Threading.Tasks;
using SimpleTalkMongoDb.Configuration;
using SimpleTalkMongoDb.Pocos;

namespace SimpleTalkMongoDb.Aggregation
{
    internal static class SampleLookup
    {


        public static void Main()
        {
            MainAsync().Wait();
            Console.ReadLine();
        }

        //
        //source: http://www.sheknows.com/baby-names
        //
        private static async Task MainAsync()
        {
            var names = new[]
            {
                new Person { FirstName = "Rita"},
                new Person { FirstName = "Katarina" }
            };

            var meanings = new[]
            {
                new NameMeaning
                {
                    Name = "Rita",
                    Definition = @"In American the meaning of the name Rita is: Pearl."

                },
                new NameMeaning
                {
                    Name = "Katarina",
                    Definition = @"In Greek the meaning of the name Katarina is: Pure."
                }
            };

            var db = SampleConfig.DbSampleLookup;
            var collPerson = SampleConfig.CollPerson;
            var collNameMeaning = SampleConfig.CollMeanings;
            // drop collections 
            db.DropCollection(collPerson.CollectionNamespace.CollectionName);
            db.DropCollection(collNameMeaning.CollectionNamespace.CollectionName);

            collPerson.InsertMany(names);
            collNameMeaning.InsertMany(meanings);

            var result = await collPerson.Aggregate()
                .Lookup<Person, NameMeaning, LookedUpPerson>(collNameMeaning, 
                    x => x.FirstName, 
                    y => y.Name,
                    x => x.Meanings
            ).ToListAsync();

            foreach (var example in result)
            {
                Console.WriteLine($"{example.FirstName.PadRight(10)} {example.Meanings.ElementAt(0).Definition}");
            }

        }

     
    }
}
