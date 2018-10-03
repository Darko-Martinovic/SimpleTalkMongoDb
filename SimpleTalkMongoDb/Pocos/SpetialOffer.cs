using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace SimpleTalkMongoDb.Pocos
{
    public class SpetialOffer
    {

        
        [BsonId]
        public int SpecialOfferId { get; set; }

        [BsonElement("offerType")]
        public string Type { get; set; }

        [BsonElement("category")]
        public string Category { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

    }

    public  class Result
    {
        [BsonId]
        public int Id { get; set; }
        public decimal LineTotal { get; set; }
        public IEnumerable<SpetialOffer> SpetOffer { get; set; }

        public int ProductId { get; set; }
    }

    public  class ResultProduct
    {
        [BsonId]
        public int Id { get; set; }

        public decimal LineTotal { get; set; }

        public IEnumerable<SpetialOffer> SpetOffer { get; set; }

        public int ProductId { get; set; }

        public IEnumerable<Product> ProductName { get; set; }
    }


}
