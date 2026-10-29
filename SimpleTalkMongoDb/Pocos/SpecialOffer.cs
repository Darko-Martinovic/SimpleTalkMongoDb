using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace SimpleTalkMongoDb.Pocos
{
    public class SpecialOffer
    {

        
        [BsonId]
        public int SpecialOfferId { get; set; }

        [BsonElement("offerType")]
        public string Type { get; set; } = "";

        [BsonElement("category")]
        public string Category { get; set; } = "";

        [BsonElement("description")]
        public string Description { get; set; } = "";

    }

    // The group projects into SalesDetail, so the offer id arrives as SpecialOfferId:
    // LINQ3 (driver 3.x) doesn't map it to _id as LINQ2 did
    [BsonIgnoreExtraElements]
    public class Result
    {
        public int SpecialOfferId { get; set; }
        public decimal LineTotal { get; set; }
        public IEnumerable<SpecialOffer> SpecialOffers { get; set; } = [];

        public int ProductId { get; set; }
    }

    // The group projects into SalesDetail, so the offer id arrives as SpecialOfferId:
    // LINQ3 (driver 3.x) doesn't map it to _id as LINQ2 did
    [BsonIgnoreExtraElements]
    public class ResultProduct
    {
        public int SpecialOfferId { get; set; }

        public decimal LineTotal { get; set; }

        public IEnumerable<SpecialOffer> SpecialOffers { get; set; } = [];

        public int ProductId { get; set; }

        public IEnumerable<Product> ProductName { get; set; } = [];
    }


}
