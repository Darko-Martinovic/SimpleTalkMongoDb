using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace SimpleTalkMongoDb.Pocos
{
    public class TestSerializer
    {
        // Int type decorated by BsonId attribute
        [BsonId]
        public int SalesOrderId { get; set; }
        // String type decorated by BsonElement Attribute
        [BsonElement("description", Order = 1)]
        public string Comment { get; set; }
        // DateTime type decorate with DateOnly attribute
        [BsonDateTimeOptions(DateOnly = true)]
        public DateTime OnlyDate { get; set; }
        // DateTime decorated with DateTimeKind.Local
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime LocalTime { get; set; }
        // Decimal type decorated with BsonType.Decimal128
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Salary { get; set; }
        // Double decorated with AllowTruncation 
        [BsonRepresentation(BsonType.Double, AllowTruncation = true), BsonElement("myduble")]
        public double MongoValueTypeDouble { get; set; }
        // Bool 
        public bool MyBool { get; set; }

    }
    public class TestSerializerWithOutAttributes
    {
        // Int type
        public int SalesOrderId { get; set; }
        // String type
        public string Comment { get; set; }
        // DateTime 
        public DateTime OnlyDate { get; set; }
        // DateTime 
        public DateTime LocalTime { get; set; }
        // Decimal
        public decimal Salary { get; set; }
        // Double
        public double MongoValueTypeDouble { get; set; }

        public bool MyBool { get; set; }
    }

}
