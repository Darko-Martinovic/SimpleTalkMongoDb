using MongoDB.Bson;
using MongoDB.Driver;

using SimpleTalkMongoDb.Configuration;

namespace SimpleTalkMongoDb.Modern
{
    /// <summary>
    /// MongoDB 4.0 (replica sets) and 4.2 (sharded clusters): multi-document transactions.
    /// CrudDemo's InsertOneWithTransaction, commented out in the article, starts, commits and aborts
    /// a transaction by hand. Today's way is WithTransactionAsync: it commits when the callback
    /// returns, aborts when it throws, and retries the whole callback on transient errors such as
    /// a write conflict.
    /// </summary>
    internal static class Transactions
    {
        private const int ProductId = 776;

        public static async Task RunAsync()
        {
            await ReplicaSet.EnsureAsync("Transactions");

            var db = SampleConfig.Db;
            await db.DropCollectionAsync("txInventory");
            await db.DropCollectionAsync("txOrders");
            var inventory = db.GetCollection<BsonDocument>("txInventory");
            var orders = db.GetCollection<BsonDocument>("txOrders");

            // Collections are created outside the transaction, before it starts
            await inventory.InsertOneAsync(new BsonDocument { { "_id", ProductId }, { "name", "Mountain-100 Black, 42" }, { "stock", 5 } });
            await db.CreateCollectionAsync("txOrders");

            ConsoleEx.WriteLine("Multi-document transactions (MongoDB 4.0), with WithTransactionAsync", ConsoleColor.Magenta);
            await ShowAsync(inventory, orders, "Before");

            using var session = await SampleConfig.Client.StartSessionAsync();

            // An order for 2 succeeds: the order and the lower stock are committed together
            await PlaceOrderAsync(session, inventory, orders, orderId: 1, quantity: 2);
            await ShowAsync(inventory, orders, "After ordering 2");

            // An order for 10 inserts the order, then finds too little stock and throws, so
            // WithTransactionAsync aborts: the order it already inserted is rolled back too
            await PlaceOrderAsync(session, inventory, orders, orderId: 2, quantity: 10);
            await ShowAsync(inventory, orders, "After trying to order 10");
        }

        private static async Task PlaceOrderAsync(
            IClientSessionHandle session, IMongoCollection<BsonDocument> inventory, IMongoCollection<BsonDocument> orders, int orderId, int quantity)
        {
            try
            {
                await session.WithTransactionAsync(async (s, cancellationToken) =>
                {
                    // Every operation in the transaction passes the session
                    await orders.InsertOneAsync(s, new BsonDocument { { "_id", orderId }, { "productId", ProductId }, { "quantity", quantity } }, cancellationToken: cancellationToken);

                    var taken = await inventory.UpdateOneAsync(
                        s,
                        Builders<BsonDocument>.Filter.Eq("_id", ProductId) & Builders<BsonDocument>.Filter.Gte("stock", quantity),
                        Builders<BsonDocument>.Update.Inc("stock", -quantity),
                        cancellationToken: cancellationToken);
                    if (taken.ModifiedCount == 0)
                    {
                        throw new InvalidOperationException($"only {await StockAsync(inventory, s)} in stock");
                    }

                    return true;
                }, new TransactionOptions(readConcern: ReadConcern.Snapshot, writeConcern: WriteConcern.WMajority));

                ConsoleEx.WriteLine($"Order {orderId} for {quantity}: committed", ConsoleColor.Yellow);
            }
            catch (InvalidOperationException e)
            {
                ConsoleEx.WriteLine($"Order {orderId} for {quantity}: aborted, {e.Message}", ConsoleColor.Red);
            }
        }

        private static async Task<int> StockAsync(IMongoCollection<BsonDocument> inventory, IClientSessionHandle session) =>
            (await inventory.Find(session, new BsonDocument("_id", ProductId)).FirstAsync())["stock"].AsInt32;

        private static async Task ShowAsync(IMongoCollection<BsonDocument> inventory, IMongoCollection<BsonDocument> orders, string when)
        {
            var stock = (await inventory.Find(new BsonDocument("_id", ProductId)).FirstAsync())["stock"];
            var placed = await orders.Find(FilterDefinition<BsonDocument>.Empty).ToListAsync();
            ConsoleEx.WriteLine(
                $"{when}: stock {stock}, orders: {(placed.Count == 0 ? "none" : string.Join(", ", placed.Select(o => $"#{o["_id"]} x{o["quantity"]}")))}",
                ConsoleColor.Cyan);
        }
    }
}
