using System;
using System.Net;
using MongoDB.Bson;
using MongoDB.Driver;
using SimpleTalkMongoDb.Pocos;


namespace SimpleTalkMongoDb.Authentication
{
    public static class Auth
    {

        public static void Main(string[] args)
        {
            Worker();
        }

        /// <summary>
        /// Shows various way how to connect to MongoDB
        /// </summary>
        private static void Worker()
        {

            var credential =
                MongoCredential.CreateCredential(databaseName: Dbase, username: UserName, password: Passwrod);

            var settings = new MongoClientSettings
            {
                Credential = credential
            };
            var mongoClient = new MongoClient(settings);
            var db = mongoClient.GetDatabase($"{Dbase}");
            var numberOfDocuments = db.GetCollection<SalesHeader>($"{Collection}")
                .CountDocumentsAsync(new BsonDocument()).Result;
            //Using connectionString
            var connectionString = $"mongodb://{UserName}:{Passwrod}@localhost:27017/{Dbase}";

            mongoClient = new MongoClient(connectionString);
            db = mongoClient.GetDatabase($"{Dbase}");
            var numberOfDocument2 = db.GetCollection<SalesHeader>($"{Collection}")
                .CountDocumentsAsync(new BsonDocument()).Result;


            // Using SecureString
            var ss = new NetworkCredential($"{UserName}", $"{Passwrod}").SecurePassword;


             credential =
                MongoCredential.CreateCredential(databaseName: Dbase, username: UserName, password: ss);


            settings = new MongoClientSettings
            {
                Credential = credential
            };

            mongoClient = new MongoClient(settings);


            db = mongoClient.GetDatabase($"{Dbase}");

            var numberOfDocuments3 = db.GetCollection<SalesHeader>($"{Collection}")
                .CountDocumentsAsync(new BsonDocument()).Result;

            ConsoleEx.WriteLine("In order to prove that we are successfully connected, let's count the number of documents in the 'adventureWorks2016' collection.", ConsoleColor.Cyan);
            ConsoleEx.WriteLine($"Number of documents : {numberOfDocuments}/{numberOfDocument2}/{numberOfDocuments3}", ConsoleColor.Yellow);
            ConsoleEx.WriteLine($"I did not find useful commands in the mongo shell to display active connections.", ConsoleColor.Red);
            ConsoleEx.WriteLine("'db.serverStatus().connections' will help, but in the resulting document, there is no detailed information about connections");
            ConsoleEx.WriteLine("'netstat –b' will helps too!");
            Console.ReadLine();
        }

        #region Constants

        private const string UserName = "usrSimpleTalk";
        private const string Passwrod = "pwdSimpleTalk";
        private const string Dbase = "simpleTalk";
        private const string Collection = "adventureWorks2016";


        #endregion

    }

}

