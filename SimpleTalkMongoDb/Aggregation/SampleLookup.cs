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
            // Define a couple of names
            var names = new[]
            {
                new Person { FirstName = "Rita"},
                new Person { FirstName = "Katarina" }
            };
            // Define a couple of meanings
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
            // insert documents
            collPerson.InsertMany(names);
            collNameMeaning.InsertMany(meanings);
            // Make lookup 
            var result = await collPerson.Aggregate()
                .Lookup<Person, NameMeaning, LookedUpPerson>(collNameMeaning, 
                    x => x.FirstName, 
                    y => y.Name,
                    x => x.Meanings
            ).ToListAsync();

            ConsoleEx.WriteLine("The name is written in red and the name's meaning in Cyan ( the meaning is  from the second collection)", ConsoleColor.Yellow);
            foreach (var example in result)
            {
                ConsoleEx.WriteLine($"{example.FirstName.PadRight(10)}", ConsoleColor.Red);
                ConsoleEx.WriteLine($"{example.Meanings.ElementAt(0).Definition}", ConsoleColor.Cyan);
                ConsoleEx.WriteLine(" ".PadRight(70,'-'));
            }

        }

     
    }
}
