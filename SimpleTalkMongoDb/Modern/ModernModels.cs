using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SimpleTalkMongoDb.Modern
{
    /// <summary>The key of a territory's sales in one month.</summary>
    public class TerritoryMonth
    {
        public int TerritoryId { get; set; }

        // The first of the month, in the sales' time zone (SalesCalendar)
        public DateTime Month { get; set; }
    }

    /// <summary>A territory's sales in one month, as $merge keeps them up to date.</summary>
    public class MonthlySales
    {
        [BsonId]
        public TerritoryMonth Id { get; set; } = new();

        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Total { get; set; }

        public int Orders { get; set; }
    }

    /// <summary>One territory's sales in one month, as a row the window functions run over.</summary>
    public class MonthTotal
    {
        public int TerritoryId { get; set; }
        public DateTime Month { get; set; }

        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Total { get; set; }
    }

    /// <summary>A sales person's sales on one day.</summary>
    public class DayTotal
    {
        [BsonId]
        public DateTime Day { get; set; }

        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Total { get; set; }

        public int Orders { get; set; }
    }

    /// <summary>What one customer bought in one territory.</summary>
    public class CustomerTotal
    {
        public int TerritoryId { get; set; }
        public int CustomerId { get; set; }

        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Total { get; set; }
    }

    /// <summary>An order as a measurement in a time series collection.</summary>
    public class SalesMeasurement
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public DateTime OrderDate { get; set; }

        // The metadata field: what the measurements are grouped into buckets by
        public int TerritoryId { get; set; }

        [BsonRepresentation(BsonType.Decimal128)]
        public decimal TotalDue { get; set; }
    }
}
