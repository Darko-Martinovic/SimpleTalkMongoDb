using System;
using System.Net;
using MongoDB.Bson;
using MongoDB.Driver;
using SimpleTalkMongoDb.Configuration;
using SimpleTalkMongoDb.Pocos;


namespace SimpleTalkMongoDb.Authentication
{
    public static class Auth
    {

        public static Task RunAsync() => Worker();

        /// <summary>
        /// Shows various way how to connect to MongoDB
        /// </summary>
        private static async Task Worker()
        {

            var credential =
                MongoCredential.CreateCredential(databaseName: Dbase, username: UserName, password: Password);

            // The configured connection's settings (server, TLS and so on) with the sample user's credential
            var settings = MongoClientSettings.FromConnectionString(SampleConfig.MongoConnectionString);
            settings.Credential = credential;
            var mongoClient = new MongoClient(settings);
            var db = mongoClient.GetDatabase($"{Dbase}");
            var numberOfDocuments = await db.GetCollection<SalesHeader>($"{Collection}")
                .CountDocumentsAsync(new BsonDocument());
            //Using connectionString
            // MongoUrlBuilder escapes the user name and password, which a URI needs for characters such as '#' or '@'
            var connectionString = new MongoUrlBuilder(SampleConfig.MongoConnectionString)
            {
                Username = UserName,
                Password = Password,
                AuthenticationSource = Dbase
            }.ToString();

            mongoClient = new MongoClient(connectionString);
            db = mongoClient.GetDatabase($"{Dbase}");
            var numberOfDocument2 = await db.GetCollection<SalesHeader>($"{Collection}")
                .CountDocumentsAsync(new BsonDocument());


            // Using SecureString
            var ss = new NetworkCredential($"{UserName}", $"{Password}").SecurePassword;


             credential =
                MongoCredential.CreateCredential(databaseName: Dbase, username: UserName, password: ss);


            settings = MongoClientSettings.FromConnectionString(SampleConfig.MongoConnectionString);
            settings.Credential = credential;

            mongoClient = new MongoClient(settings);


            db = mongoClient.GetDatabase($"{Dbase}");

            var numberOfDocuments3 = await db.GetCollection<SalesHeader>($"{Collection}")
                .CountDocumentsAsync(new BsonDocument());

            ConsoleEx.WriteLine($"In order to prove that we are successfully connected, let's count the number of documents in the '{Collection}' collection.", ConsoleColor.Cyan);
            ConsoleEx.WriteLine($"Number of documents : {numberOfDocuments}/{numberOfDocument2}/{numberOfDocuments3}", ConsoleColor.Yellow);
            ConsoleEx.WriteLine($"I did not find useful commands in the mongo shell to display active connections.", ConsoleColor.Red);
            ConsoleEx.WriteLine("'db.serverStatus().connections' will help, but in the resulting document, there is no detailed information about connections");
            ConsoleEx.WriteLine("'netstat –b' will helps too!");
        }

        #region Constants

        // The user the seed command creates in the configured database, and the collection it counts
        private static readonly string UserName = SampleConfig.Settings["SampleUser"]!;
        private static readonly string Password = SampleConfig.Settings["SamplePassword"]!;
        private static readonly string Dbase = SampleConfig.Db.DatabaseNamespace.DatabaseName;
        private static readonly string Collection = SampleConfig.Collection.CollectionNamespace.CollectionName;


        #endregion

    }

}

