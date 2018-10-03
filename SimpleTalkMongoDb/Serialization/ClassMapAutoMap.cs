using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using SimpleTalkMongoDb.Pocos;
using System;

namespace SimpleTalkMongoDb.Serialization
{
    internal static class ClassMapAutoMap
    {
        public static void Main(string[] args)
        {
            Worker();
            Console.WriteLine("Press any key to exit!");
            Console.ReadLine();
        }



        private static void Worker()
        {

            // Note that a class map must only be registered once(an exception will be thrown if you try to register the same class map more than once).
            // Usually you call RegisterClassMap from some code path that is known to execute only once(the Main method, the Application_Start event handler, etc…).
            if (!BsonClassMap.IsClassMapRegistered(typeof(TestSerializerWithOutAttributes)))
            {
                BsonClassMap.RegisterClassMap<TestSerializerWithOutAttributes>(cm =>
                {
                    // ! only read and write properties are mapped
                    // in the example cm.AutoMap is not used!
                    cm.AutoMap();
                   
                    // BsonId attribute
                    cm.MapIdProperty(c => c.SalesOrderId);

                    //Setting ElementName and Order
                    cm.GetMemberMap(c => c.Comment).SetElementName("description").SetOrder(1);

                    // Setting the default decimal serializer
                    cm.GetMemberMap(c => c.Salary).SetSerializer(new DecimalSerializer(BsonType.Decimal128));

                  


                    //Before was .SetSerializationOptions(new DateTimeSerializationOptions { DateOnly = true });
                    // We could accomplished the same task by applying custom serializer or specfiying pre-build serialzer DateTimeSerializer
                    //cm.MapProperty(c => c.OnlyDate).SetSerializer(new OnlyDateSerializer());
                    cm.GetMemberMap(c => c.OnlyDate).SetSerializer(new DateTimeSerializer(dateOnly: true));
                    cm.GetMemberMap(c => c.LocalTime).SetSerializer(new DateTimeSerializer(DateTimeKind.Local));



                });
            }


            var p = new TestSerializerWithOutAttributes()
            {
                SalesOrderId = 1,
                Comment = "Test",
                OnlyDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day),
                LocalTime = DateTime.Now,
                Salary = 120.1112223334455666m,
                MongoValueTypeDouble = 1.11122233344445556667778888999
            };
            var ws = new JsonWriterSettings
            {
                Indent = true,
            };

            ConsoleEx.WriteLine("The Serialization process is controlled by using ClassMap", ConsoleColor.Cyan);
            ConsoleEx.WriteLine(p.ToJson(ws), ConsoleColor.Yellow);
            var p1 = BsonSerializer.Deserialize<TestSerializerWithOutAttributes>(p.ToJson());

            Console.WriteLine(new
            {
                p1.LocalTime.Kind,
                Value = p1.LocalTime
            });


        }


    }


}
