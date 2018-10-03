using MongoDB.Bson;
using System.Collections.Generic;

namespace SimpleTalkMongoDb.Pocos
{
    public class Person
    {
        public string FirstName;
    }
    public class NameMeaning
    {
        public ObjectId Id { get; set; }
        public string Name;
        public string Definition;


    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public class LookedUpPerson
    {

        public ObjectId Id { get; set; }

        public string FirstName { get; set; }

        public IEnumerable<NameMeaning> Meanings { get; set; }
    }
}
