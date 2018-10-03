using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System;
using MongoDB.Bson.IO;
using SimpleTalkMongoDb.Pocos;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace SimpleTalkMongoDb.Serialization
{
    internal static class AtributteDecoration
    {
        public static void Main(string[] args)
        {
            Worker();
            Console.WriteLine("Press any key to exit!");
            Console.ReadLine();
        }

        private static void Worker()
        {

            // instantiated an object of the class without attributes
            var pNoAttributes = new TestSerializerWithOutAttributes()
            {
                SalesOrderId = 1,
                Comment = "Test",
                OnlyDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day),
                LocalTime = DateTime.Now,
                Salary = 120.1112223334455666m,
                MongoValueTypeDouble = 1.11122233344445556667778888999,
                MyBool = true

            };
            // instantiated an object of the class which properties are decorated by attributes
            var pWithAttributes = new TestSerializer
            {
                SalesOrderId = 1,
                Comment = "Test",
                OnlyDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day),
                LocalTime = DateTime.Now,
                Salary = 120.1112223334455666m,
                MongoValueTypeDouble = 1.11122233344445556667778888999,
                MyBool = true

            };
            var ws = new JsonWriterSettings
            {
                Indent = true,
            };

            ConsoleEx.WriteLine(
                "Will display serialization of an object which class's properties are NOT decorated by attributes",
                ConsoleColor.Cyan);

            // ToJson actually calls BsonSerializer which performs serialization.
            // JsonWriterSettings that is passed as parameter controls the behaviour of serialization.
            ConsoleEx.WriteLine(pNoAttributes.ToJson(ws), ConsoleColor.Yellow);



            ConsoleEx.WriteLine(
                "Will display serialization of an object which class's properties are decorated by attributes",
                ConsoleColor.Cyan);
            ConsoleEx.WriteLine(pWithAttributes.ToJson(ws), ConsoleColor.Yellow);

            // performs deserialization
            var p1 = BsonSerializer.Deserialize<TestSerializer>(pWithAttributes.ToJson());

            ConsoleEx.WriteLine("Will display deserialization of datatime field using LocalTime.Kind", ConsoleColor.Cyan);
            Console.WriteLine(new
            {
                p1.LocalTime.Kind,
                Value = p1.LocalTime
            });


        }

       

    }
}
