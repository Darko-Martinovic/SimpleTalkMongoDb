using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.Collections.Generic;

namespace CreateMongoJson.Pocos
{

    public enum MyEnum
    {
        First = 1,
        Second,
    }

    public class Book
    {
        public decimal Price;
        public string Title;
        public string Author;
    }


    /// <summary>
    /// The first level of inheritence
    /// </summary>
    public class SimplePocoDescended : SimplePoco
    {
        public int MySimpleInt { get; set; }

        public IEnumerable<Book> MyBooks { get; set; }
    }

    /// <summary>
    /// The first class
    /// </summary>
    public class SimplePoco
    {

        public int Id { get; set; }


        public Queue<int> MongoQueue { get; set; }

        public Stack<string> MongoStack { get; set; }


        public List<long> MongoList { get; set; }

        [BsonDictionaryOptions(DictionaryRepresentation.Document)]
        public Dictionary<string, int> MongoDictonary { get; set; }


        public MyEnum MongoEnum { get; set; }


    }


}
