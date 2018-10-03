using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

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

    public abstract class Result
    {
        [BsonId]
        public int Id { get; set; }
        public decimal LineTotal { get; set; }
        public IEnumerable<SpetialOffer> SpetOffer
        {
            get { yield break; }
        }

        public int ProductId => 0;
    }

    public abstract class ResultProduct
    {
        [BsonId]
        public int Id => 0;

        public decimal LineTotal => 0;

        public IEnumerable<SpetialOffer> SpetOffer
        {
            get { yield break; }
        }

        public int ProductId => 0;

        public IEnumerable<Product> ProductName
        {
            get { yield break; }
        }
    }


}
