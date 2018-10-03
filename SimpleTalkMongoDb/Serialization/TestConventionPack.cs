using System;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using SimpleTalkMongoDb.Pocos;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace SimpleTalkMongoDb.Serialization
{
    internal static class TestConventionPack
    {
        public static void Main(string[] args)
        {
           
            Worker();
           
            Console.WriteLine("Press any key to exit!");
            Console.ReadLine();
        }

        private static void Worker()
        {

            var conventions = new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new DecimalRepresentationConvention(BsonType.String),
                new DateOnlyRepresentation(BsonType.DateTime),
                new LocalDateRepresentation(BsonType.DateTime)


            };

            ConventionRegistry.Register("Test", conventions,
                type => type.FullName != null && type.FullName.Contains("TestSerializerWithOutAttributes"));




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

            ConsoleEx.WriteLine("The serialization process is controlled by using Convention Pack", ConsoleColor.Cyan);
            ConsoleEx.WriteLine(p.ToJson(ws), ConsoleColor.Yellow);
            var p1 = BsonSerializer.Deserialize<TestSerializerWithOutAttributes>(p.ToJson());

            Console.WriteLine(new
            {
                p1.LocalTime.Kind,
                Value = p1.LocalTime
            });


        }

        private class DateOnlyRepresentation : ConventionBase, IMemberMapConvention
        {

            public DateOnlyRepresentation(BsonType representation)
            {
                if (representation != BsonType.DateTime)
                {
                    throw new ArgumentException("Only datetime is allowed");
                }

            }

            public void Apply(BsonMemberMap memberMap)
            {
                if (memberMap.MemberType != typeof(DateTime))
                {
                    return;
                }
                if (memberMap.ElementName.ToLower().Contains("onlydate") == false)
                    return;

                memberMap.SetSerializer(new DateTimeSerializer(dateOnly: true));
            }
        }

        private class LocalDateRepresentation : ConventionBase, IMemberMapConvention
        {

            public LocalDateRepresentation(BsonType representation)
            {
                if (representation != BsonType.DateTime)
                {
                    throw new ArgumentException("Only datetime is allowed");
                }

            }

            public void Apply(BsonMemberMap memberMap)
            {
                if (memberMap.MemberType != typeof(DateTime))
                {
                    return;
                }
                if (memberMap.ElementName.ToLower().Contains("localtime") == false)
                    return;

                memberMap.SetSerializer(new DateTimeSerializer(DateTimeKind.Local));
            }
        }
        private class DecimalRepresentationConvention : ConventionBase, IMemberMapConvention
        {
            public DecimalRepresentationConvention(BsonType representation)
            {
                if (representation != BsonType.String)
                {
                    throw new ArgumentException("Only for strings");
                }
            }

            void IMemberMapConvention.Apply(BsonMemberMap memberMap)
            {
                if (memberMap.MemberType != typeof(decimal))
                {
                    return;
                }
                memberMap.SetSerializer(new DecimalSerializer(BsonType.Decimal128));
            }
        }




    }

    
}
