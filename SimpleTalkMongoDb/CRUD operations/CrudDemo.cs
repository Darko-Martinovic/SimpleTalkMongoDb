using MongoDB.Driver;

using SimpleTalkMongoDb.Configuration;
using SimpleTalkMongoDb.Pocos;

using System;
using System.Threading.Tasks;

namespace SimpleTalkMongoDb.CRUD_operations
{
    internal static class CrudDemo
    {
        public static void Main(string[] args)
        {
            MainAsync().Wait();
            Console.ReadLine();
        }

        /// <summary>
        /// Performs various CRUD operaton on 'spetialOffer' collection
        /// </summary>
        /// <returns></returns>
        private static async Task MainAsync()
        {

            await InsertOne(SampleConfig.CollSpetialOffer);
            await InsertManyDemo(SampleConfig.CollSpetialOffer);
            //only works on V4.X and with replica sets
            //await InsertOneWithTransaction(SampleConfig.CollSpetialOffer);


            await ReplaceOneDemo(SampleConfig.CollSpetialOffer);
            await UpdateOneDemo(SampleConfig.CollSpetialOffer);
            await ReplaceUpsertDemo(SampleConfig.CollSpetialOffer);


            await DeleteOne(SampleConfig.CollSpetialOffer);
            await DeleteManyDemo(SampleConfig.CollSpetialOffer);
        }


        #region  Insert operation 

        private static async Task InsertOne(IMongoCollection<SpetialOffer> collection)
        {


            var so = new SpetialOffer
            {
                SpecialOfferId = IdToAdd,
                Description = "Test inserting one",
                Type = "New Product",
                Category = "Reseller"
            };


            await collection.InsertOneAsync(so);




            ConsoleEx.WriteLine("Inserting one document ", ConsoleColor.Red);
            ConsoleEx.WriteLine("------------------------");
            var result = await collection.Find(x => x.SpecialOfferId == IdToAdd).ToListAsync();

            // to find in mongo shell execute db.spetialOffer.find({ _id: 20})
            // to delete using mongo shell execute db.spetialOffer.remove({_id:20})
            foreach (var doc in result)
            {
                ConsoleEx.WriteLine($"SpetialOfferId {doc.SpecialOfferId}", ConsoleColor.Yellow);
                ConsoleEx.WriteLine($"Description    {doc.Description}");
                ConsoleEx.WriteLine($"Type           {doc.Type} ", ConsoleColor.Yellow);
                ConsoleEx.WriteLine($"Category       {doc.Category}");
            }

        }

        private static async Task InsertManyDemo(IMongoCollection<SpetialOffer> collSpetialOffer)
        {
            var so = new[]
            {
                new SpetialOffer
                {
                    SpecialOfferId = IdToAddMany,
                    Description = "Test inserting many 1",
                    Type = "New Product",
                    Category = "Reseller"
                },
                new SpetialOffer
                {
                SpecialOfferId = IdToAddMany2,
                Description = "Test inserting many 2",
                Type = "New Product",
                Category = "Reseller"
                }

            };
            var imo = new InsertManyOptions
            {
                IsOrdered = false
            };

            await collSpetialOffer.InsertManyAsync(so, imo);




            ConsoleEx.WriteLine("Inserting many documents ", ConsoleColor.Red);
            ConsoleEx.WriteLine("------------------------");

            var result = await collSpetialOffer.Find(x => x.SpecialOfferId >= IdToAddMany).ToListAsync();
            // to find in mongo shell execute  db.spetialOffer.find({_id:{$gte:30}})
            // to delete using mongo shell execute db.spetialOffer.remove({_id:{$gte:30}})
            foreach (var doc in result)
            {
                ConsoleEx.WriteLine($"SpetialOfferId {doc.SpecialOfferId}", ConsoleColor.Yellow);
                ConsoleEx.WriteLine($"Description    {doc.Description}", ConsoleColor.Yellow);
                ConsoleEx.WriteLine($"Type           {doc.Type} ", ConsoleColor.Yellow);
                ConsoleEx.WriteLine($"Category       {doc.Category}");
            }

        }

        /// <summary>
        /// Transactions are supported on V4.0 and only for replica sets
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        // ReSharper disable once UnusedMember.Local
        private static async Task InsertOneWithTransaction(IMongoCollection<SpetialOffer> collection)
        {


            var so = new SpetialOffer
            {
                SpecialOfferId = IdToAdd,
                Description = "Test inserting one",
                Type = "New Product",
                Category = "Reseller"
            };


            // return IClientSessionHandle object
            var session = SampleConfig.Client.StartSession();

            // start transaction
            session.StartTransaction(new TransactionOptions(

                readConcern: ReadConcern.Snapshot,

                writeConcern: WriteConcern.WMajority));

            try
            {
                // Note we have to pass session object
                await collection.InsertOneAsync(session, so);
                await collection.InsertOneAsync(session, so);
                // the transaction is commited
                session.CommitTransaction();
            }
            catch (Exception)
            {
                // the transaction is aborted
                session.AbortTransaction();
            }



            ConsoleEx.WriteLine("Inserting one document ", ConsoleColor.Red);
            ConsoleEx.WriteLine("------------------------");
            var result = await collection.Find(x => x.SpecialOfferId == IdToAdd).ToListAsync();

            // to find in mongo shell execute db.spetialOffer.find({ _id: 20})
            // to delete using mongo shell execute db.spetialOffer.remove({_id:20})
            foreach (var doc in result)
            {
                ConsoleEx.WriteLine($"SpetialOfferId {doc.SpecialOfferId}", ConsoleColor.Yellow);
                ConsoleEx.WriteLine($"Description    {doc.Description}");
                ConsoleEx.WriteLine($"Type           {doc.Type} ", ConsoleColor.Yellow);
                ConsoleEx.WriteLine($"Category       {doc.Category}");
            }

        }


        #endregion
        
        #region  Update / Replace operation 

        private static async Task ReplaceOneDemo(IMongoCollection<SpetialOffer> collection)
        {

            // It is not possible to change ID !!!
            // if we specify the condition that has no matching document default behavior is to do nothing!
            var so = new SpetialOffer
            {
                SpecialOfferId = IdToAdd,
                Description = "NEW DESCRIPTION",
                Type = "NEW TYPE",
                Category = "NEW Reseller"
            };
            //var uo = new UpdateOptions
            //{
            //    IsUpsert = true
            //};
            var resultOfReplace = await collection.ReplaceOneAsync(x => x.SpecialOfferId == IdToAdd, so);



            ConsoleEx.WriteLine("Replacing one document ", ConsoleColor.Red);
            ConsoleEx.WriteLine("------------------------");
            ConsoleEx.WriteLine($"IsAcknowledged {resultOfReplace.IsAcknowledged}", ConsoleColor.Cyan);
            ConsoleEx.WriteLine($"IsModifiedCountAvailable {resultOfReplace.IsModifiedCountAvailable}", ConsoleColor.Cyan);
            ConsoleEx.WriteLine($"MatchedCount {resultOfReplace.MatchedCount}", ConsoleColor.Cyan);
            ConsoleEx.WriteLine($"UpsertedId {resultOfReplace.UpsertedId}", ConsoleColor.Cyan);
            ConsoleEx.WriteLine($"ModifiedCount {resultOfReplace.ModifiedCount}", ConsoleColor.Cyan);



            var result = await collection.Find(x => x.SpecialOfferId == IdToAdd).ToListAsync();
            // to find in mongo shell execute db.spetialOffer.find({ _id: 20})
            // to delete using mongo shell execute db.spetialOffer.remove({_id:20})
            ConsoleEx.WriteLine("Description should be now : " + so.Description, ConsoleColor.Cyan);
            foreach (var doc in result)
            {
                ConsoleEx.WriteLine($"SpetialOfferId {doc.SpecialOfferId}", ConsoleColor.Yellow);
                ConsoleEx.WriteLine($"Description    {doc.Description}", ConsoleColor.Yellow);
                ConsoleEx.WriteLine($"Type           {doc.Type} ", ConsoleColor.Yellow);
                ConsoleEx.WriteLine($"Category       {doc.Category}");
            }

        }

        private static async Task UpdateOneDemo(IMongoCollection<SpetialOffer> collection)
        {


            var ud = Builders<SpetialOffer>.Update.Set("Description", "Descripton from update");
            var resultOfUpdate = await collection.UpdateOneAsync(x => x.SpecialOfferId == IdToAdd, ud);

            ConsoleEx.WriteLine("Update one document ", ConsoleColor.Red);
            ConsoleEx.WriteLine("------------------------");


            ConsoleEx.WriteLine($"IsAcknowledged {resultOfUpdate.IsAcknowledged}", ConsoleColor.Cyan);
            ConsoleEx.WriteLine($"IsModifiedCountAvailable {resultOfUpdate.IsModifiedCountAvailable}", ConsoleColor.Cyan);
            ConsoleEx.WriteLine($"MatchedCount {resultOfUpdate.MatchedCount}", ConsoleColor.Cyan);
            ConsoleEx.WriteLine($"UpsertedId {resultOfUpdate.UpsertedId}", ConsoleColor.Cyan);
            ConsoleEx.WriteLine($"ModifiedCount {resultOfUpdate.ModifiedCount}", ConsoleColor.Cyan);

            var result = await collection.Find(x => x.SpecialOfferId == IdToAdd).ToListAsync();


            ConsoleEx.WriteLine("Description should be now : 'Descripton from update'", ConsoleColor.Cyan);
            foreach (var doc in result)
            {
                ConsoleEx.WriteLine($"SpetialOfferId {doc.SpecialOfferId}", ConsoleColor.Yellow);
                ConsoleEx.WriteLine($"Description    {doc.Description}", ConsoleColor.Yellow);
                ConsoleEx.WriteLine($"Type           {doc.Type} ", ConsoleColor.Yellow);
                ConsoleEx.WriteLine($"Category       {doc.Category}");
            }

        }

        private static async Task ReplaceUpsertDemo(IMongoCollection<SpetialOffer> collection)
        {

            // if we specify the condition that has no matching document default behavior is to do nothing!
            var so = new SpetialOffer
            {
                SpecialOfferId = IdToUpsert,
                Description = "Upsert demo",
                Type = "Upsert type",
                Category = "Upsert Category"
            };
            await collection.ReplaceOneAsync(x => x.SpecialOfferId == IdToUpsert, so, new UpdateOptions
            {
                IsUpsert = true
            });

            ConsoleEx.WriteLine("Update/upsert document ", ConsoleColor.Red);
            ConsoleEx.WriteLine("------------------------");
            var result = await collection.Find(x => x.SpecialOfferId == IdToUpsert).ToListAsync();
            // to find in mongo shell execute db.spetialOffer.find({ _id: 20})
            // to delete using mongo shell execute db.spetialOffer.remove({_id:20})
            ConsoleEx.WriteLine("We are now upserting : " + so.Description, ConsoleColor.Cyan);
            foreach (var doc in result)
            {
                ConsoleEx.WriteLine($"SpetialOfferId {doc.SpecialOfferId}", ConsoleColor.Yellow);
                ConsoleEx.WriteLine($"Description    {doc.Description}", ConsoleColor.Yellow);
                ConsoleEx.WriteLine($"Type           {doc.Type} ", ConsoleColor.Yellow);
                ConsoleEx.WriteLine($"Category       {doc.Category}");
            }

        }

        #endregion
        
        #region  Delete operation 

        private static async Task DeleteOne(IMongoCollection<SpetialOffer> collSpetialOffer)
        {
            Console.WriteLine();
            ConsoleEx.WriteLine($"Deleting document SpetialOfferId = {IdToAdd}", ConsoleColor.Red);
            Console.WriteLine();
            var result = await collSpetialOffer.DeleteOneAsync(x => x.SpecialOfferId == IdToAdd);
            Console.WriteLine($"Is deleted completed : {result.IsAcknowledged}");
        }

        private static async Task DeleteManyDemo(IMongoCollection<SpetialOffer> collSpetialOffer)
        {
            Console.WriteLine();
            ConsoleEx.WriteLine($"Deleting documents SpetialOfferId >= {IdToAddMany}", ConsoleColor.Red);
            Console.WriteLine();
            var result = await collSpetialOffer.DeleteManyAsync(x => x.SpecialOfferId >= IdToAddMany);
            Console.WriteLine($"Is deleted completed : {result.IsAcknowledged} number of deleted record : {result.DeletedCount}");
        }


        #endregion

        #region Constants

        private const int IdToAdd = 20;
        private const int IdToAddMany = 30;
        private const int IdToAddMany2 = 31;
        private const int IdToUpsert = 40;




        #endregion








    }
}
