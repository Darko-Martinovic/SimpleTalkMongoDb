using System;
using MongoDB.Driver;

using SimpleTalkMongoDb.Pocos;

using System.Configuration;
using MongoDB.Bson;

namespace SimpleTalkMongoDb.Configuration
{
    public static class SampleConfig
    {

        static SampleConfig()
        {
            try
            {
                // Path to the AdventureWorks2016 database 
                ConnectionString = ConfigurationManager.ConnectionStrings["ConnStr"].ConnectionString;

                // MongoDB Client
                Client = new MongoClient(ConfigurationManager.AppSettings["MongoConnStr"]);

                // MongoDB Database - default 'simpleTalk'
                Db = Client.GetDatabase(ConfigurationManager.AppSettings["MongoDbName"]);

                // To demonstrate $lookup operator, 'sampleLookup' database is used
                DbSampleLookup = Client.GetDatabase(ConfigurationManager.AppSettings["SampleLookup"]);

                // The main collection 'adventureWorks2016'
                Collection = Db.GetCollection<SalesHeader>(ConfigurationManager.AppSettings["MongoCollectionName"]);

                // The collection that supports dynamic schema
                Db.GetCollection<BsonDocument>(ConfigurationManager.AppSettings["MongoCollectionName"]);

                // The Spetial Offer Collection
                CollSpetialOffer = Db.GetCollection<SpetialOffer>(ConfigurationManager.AppSettings["MongoSpetialOfferCollectionName"]);

                // The Product Collection
                CollProducts = Db.GetCollection<Product>(ConfigurationManager.AppSettings["MongoProducts"]);


                // Collections used to demonstate $lookup operator
                CollPerson = DbSampleLookup.GetCollection<Person>(ConfigurationManager.AppSettings["MongoPersons"]);
                CollItems = DbSampleLookup.GetCollection<Items>(ConfigurationManager.AppSettings["MongoItems"]);
                CollMeanings = DbSampleLookup.GetCollection<NameMeaning>(ConfigurationManager.AppSettings["MongoMeanings"]);

                // T-SQL statements used to generate JSON files
                TsqlSales = ConfigurationManager.AppSettings["TSQL"];
                TsqlSpetailOffers = ConfigurationManager.AppSettings["TSQLSpetialOffers"];
                TsqlProducts = ConfigurationManager.AppSettings["TSQLProducts"];

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


        public static string ConnectionString { get; private set; }

        public static MongoClient Client { get; }

        public static IMongoDatabase Db { get; set; }

        public static IMongoDatabase DbSampleLookup { get; private set; }


        public static IMongoCollection<SalesHeader> Collection { get; private set; }

        public static IMongoCollection<SpetialOffer> CollSpetialOffer { get; private set; }

        public static IMongoCollection<Product> CollProducts { get; private set; }

        public static IMongoCollection<Person> CollPerson { get; set; }

        public static IMongoCollection<Items> CollItems { get; set; }


        public static IMongoCollection<NameMeaning> CollMeanings { get; set; }


        public static string TsqlSales { get; }

        public static string TsqlSpetailOffers { get; }

        public static string TsqlProducts { get; }



    }
}
