using CreateMongoJson.Pocos;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson.IO;

namespace SimpleTalkMongoDb.Serialization
{
    internal static class TestTypes
    {


        public static void Main(string[] args)
        {
            MainAsync().Wait();
            Console.ReadLine();
        }
        private static async Task MainAsync()
        {
            var db = MongoDatabase(out var collection);
            db.DropCollection(collection.CollectionNamespace.CollectionName);

            InsertSample(collection);

            await ShowResults(collection);
        }

        private static IMongoDatabase MongoDatabase(out IMongoCollection<SimplePoco> collection)
        {
            var client = new MongoClient();
            var db = client.GetDatabase("Types");
            collection = db.GetCollection<SimplePoco>("Test");
            return db;
        }

        private static async Task ShowResults(IMongoCollection<SimplePoco> collection)
        {
            var result = await collection.Find(x => true).ToListAsync();

            var ws = new JsonWriterSettings
            {
               OutputMode = JsonOutputMode.Strict,
               Indent = true
            };
            Console.WriteLine();
            ConsoleEx.WriteLine("Outputing result using JsonOutputMode.Strict", ConsoleColor.Red);
            Console.WriteLine();
            foreach (var e in result)
            {
                ConsoleEx.WriteLine($"{e.Id} {e.GetType()}", ConsoleColor.Magenta);
                ConsoleEx.WriteLine(e.ToJson(ws), ConsoleColor.Yellow);
                Console.WriteLine();
            }

            Console.WriteLine();
            ws.OutputMode = JsonOutputMode.Shell;
            Console.WriteLine();
            ConsoleEx.WriteLine("Outputing result using JsonOutputMode.Shell", ConsoleColor.Red);
            foreach (var e in result)
            {
                ConsoleEx.WriteLine($"{e.Id} {e.GetType()}", ConsoleColor.Magenta);
                ConsoleEx.WriteLine(e.ToJson(ws), ConsoleColor.Yellow);
                Console.WriteLine();
            }

        }

        private static void InsertSample(IMongoCollection<SimplePoco> col)
        {
            var mySimplePoco = new SimplePoco()
            {

                Id = 10,
                MongoQueue = new Queue<int>(),
                MongoList = new List<long> { 10, 20, 30 },
                MongoEnum = MyEnum.Second,
                MongoDictonary = new Dictionary<string, int> { { "First", 1 }, { "Second", 2 } },
            
            };

            mySimplePoco.MongoQueue.Enqueue(10);
            mySimplePoco.MongoQueue.Enqueue(20);
            mySimplePoco.MongoQueue.Enqueue(30);

            col.InsertOne(mySimplePoco);

            //
            var sampleDesc = new SimplePocoDescended()
            {
                MySimpleInt = 112,
                MyBooks = new[]
                {
                    new Book()
                    {
                        Author =  "Darko",
                        Title =  "Programming MongoDb"
                    },
                    new Book()
                    {
                        Author =  "Phil",
                        Title =  "Crime in Chicago"
                    }
                },
                Id = 20,
                MongoStack =  new Stack<string>(),
                MongoList = new List<long> { 1, 2, 3 },
                MongoEnum = MyEnum.First
            };
            sampleDesc.MongoStack.Push("First pushed on MongoStack");
            sampleDesc.MongoStack.Push("Second pushed on MongoStack");
            col.InsertOne(sampleDesc);

          

        }

    };
}

