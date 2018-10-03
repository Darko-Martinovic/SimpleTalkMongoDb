using MongoDB.Bson.Serialization.Attributes;

namespace SimpleTalkMongoDb.Pocos
{
    public class Product
    {
        [BsonId]
        public int ProductId { get; set; }

        [BsonElement("productNumber")]
        public string ProductNumber { get; set; }

        [BsonElement("productName")]
        public string ProductName { get; set; }

        [BsonElement("subCategory")]
        public string SubCategoryName { get; set; }

        [BsonElement("category")]
        public string Category { get; set; }
    }
}
