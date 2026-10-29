using MongoDB.Bson;
using MongoDB.Driver;

using SimpleTalkMongoDb.Configuration;

namespace SimpleTalkMongoDb.Modern
{
    /// <summary>
    /// Transactions and change streams need a replica set (or a sharded cluster); a standalone
    /// server refuses them with an error that doesn't say so plainly. This says it first.
    /// </summary>
    internal static class ReplicaSet
    {
        public static async Task EnsureAsync(string feature)
        {
            var hello = await SampleConfig.Client.GetDatabase("admin").RunCommandAsync<BsonDocument>(new BsonDocument("hello", 1));
            var isReplicaSet = hello.Contains("setName");
            var isMongos = hello.TryGetValue("msg", out var msg) && msg == "isdbgrid";
            if (!isReplicaSet && !isMongos)
            {
                throw new InvalidOperationException(
                    $"{feature} need a replica set, and this MongoDB is a standalone server. " +
                    "Start the one in compose.yaml (a single-node replica set), or run mongod with --replSet.");
            }
        }
    }
}
