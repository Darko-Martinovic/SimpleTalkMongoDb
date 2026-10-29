# SimpleTalkMongoDb

Programming MongoDB 8 with the .NET driver 3.x on .NET 10: a console app of small examples, one per topic, on the AdventureWorks2016 sales data.

The examples accompany the Simple Talk article [How to Program with MongoDB Using the .NET Driver](https://www.red-gate.com/simple-talk/dotnet/net-development/how-to-program-with-mongodb-using-the-net-driver/). The code as the article was written (MongoDB 4.0.9, driver 2.7, .NET Framework 4.8) is kept under the tag [`v1-article`](https://github.com/Darko-Martinovic/SimpleTalkMongoDb/tree/v1-article).

## Quick start

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download) and MongoDB 8.

1. **Start MongoDB.** Either a local MongoDB on `localhost:27017`, or the container in `compose.yaml`:

   ```bash
   docker compose up -d        # or: podman compose up -d
   ```

   The container is `mongodb/mongodb-atlas-local`: a single-node replica set with Atlas Search and Vector Search. If port 27017 is taken (by a local MongoDB, for example), start it with `MONGO_PORT=27018` and point the app at that port (see Configuration).

2. **Load the sample data** (about 31,000 sales orders, products and special offers from `SimpleTalkMongoDb/Data`), and create the user the authentication example signs in as:

   ```bash
   dotnet run --project SimpleTalkMongoDb -- seed
   ```

3. **Run the examples:**

   ```bash
   dotnet run --project SimpleTalkMongoDb                     # pick from a menu
   dotnet run --project SimpleTalkMongoDb -- aggregation-sales # run one
   dotnet run --project SimpleTalkMongoDb -- list              # list them all
   ```

Each example runs in a process of its own, as each had its own `Main` before: the serialization examples register class maps and conventions for the whole process.

## Configuration

`SimpleTalkMongoDb/appsettings.json` holds the connection strings and the database and collection names. Override any of them with user secrets or environment variables, for example:

```bash
# A MongoDB on another port, or with credentials
export ConnectionStrings__Mongo="mongodb://localhost:27018/?directConnection=true"

# Or keep it out of the shell
dotnet user-secrets --project SimpleTalkMongoDb set "ConnectionStrings:Mongo" "mongodb://user:password@localhost:27017/?authSource=admin"
```

`directConnection=true` lets a single-node replica set in a container be reached from the host.

## :white_check_mark: Aggregation

Aggregation operations group values from multiple documents together and can perform a variety of operations on the grouped data to return a single result. The examples use the aggregation pipeline, through LINQ, `IAggregateFluent` and shell-like `BsonDocument` stages. (Map-reduce, which the article also mentions, is deprecated since MongoDB 5.0; use the aggregation pipeline.)

Example | Class | Description
---|---|---
`aggregation-sales` | AggregationSales | Basic aggregation using LINQ, IAggregateFluent and shell-like BsonDocuments
`aggregation-unwind` | AggregationUnwind | Various aggregations on embedded documents with the `$unwind` operator
`sample-lookup` | SampleLookup | Combining two collections with the `$lookup` operator
`sample-unwind` | SampleUnwind | Using the `$unwind` operator

## :white_check_mark: Authentication

Example | Class | Description
---|---|---
`auth` | Auth | Connecting to MongoDB with credentials and a connection string

## :white_check_mark: Configuration

Class | Description
---|---
SampleConfig | Reads `appsettings.json` and holds the client, databases and collections. Unlike an ADO.NET connection, a MongoDB client is a cheap resource that is meant to be shared.

## :white_check_mark: CRUD operations

Example | Class | Description
---|---|---
`crud` | CrudDemo | Basic CRUD operations: insert, replace, update, upsert and delete

## :white_check_mark: Filtering

Example | Class | Description
---|---|---
`explain` | Explain | Getting the query plan from code
`filter-detail` | FilterDetail | Querying for a document nested in an array
`filter-null-or-missing` | FilterForNullOrMissingFields | Querying for null or missing fields
`filter-header` | FilterHeader | Various ways to query documents
`filter-nested` | FilterNestedDocuments | Querying nested documents
`find-with-cursor` | FindWithCursor | Using cursors, lists and `ForEachAsync`

The same queries for `mongosh` are in `SimpleTalkMongoDb/Filtering/Mongo shell`.

## :white_check_mark: Serialization

Example | Class | Description
---|---|---
`attribute-decoration` | AttributeDecoration | Customising serialization with attributes
`class-map` | ClassMap | Customising serialization with a ClassMap
`class-map-automap` | ClassMapAutoMap | A ClassMap with AutoMap
`convention-pack` | TestConventionPack | Customising serialization with a ConventionPack
`test-types` | TestTypes | How various .NET types are serialized

## :white_check_mark: Loaders

The loaders regenerate the JSON files in `SimpleTalkMongoDb/Data` from a SQL Server with [AdventureWorks2016](https://learn.microsoft.com/sql/samples/adventureworks-install-configure), set in `ConnectionStrings:AdventureWorks`. They write to the current directory. You only need them to rebuild the data; `seed` loads the files as they are.

Example | Class | Description
---|---|---
`create-json-products` | CreateJsonProducts | Writes Product.json
`create-json-sales` | CreateJsonSales | Writes Sales.json
`create-json-special-offers` | CreateJsonSpecialOffer | Writes SpecialOffer.json

## What changed since the article

The examples do what they did, on current versions. Where the article's code no longer applies:

- **One app, one menu.** Each example had its own `Main`, and the project's startup object picked which one ran. Now `Program.cs` runs any of them by name. `async Task` replaces `.Wait()` and `.Result`.
- **Configuration** moved from `App.config` to `appsettings.json`, user secrets and environment variables.
- **Seeding.** `seed` replaces `mongoimport` (`Data/MongoImports.cmd` still works) and `Authentication/AddUser.js`. The JSON files are no longer compiled into the executable.
- **Driver 3.x**:
  - LINQ3 is the only LINQ provider. A `$group` that projects into a class now writes the key under the property's name instead of in `_id`, so `AggregationUnwind`'s lookup result reads `SpecialOfferId`.
  - A `decimal` without attributes is stored as `Decimal128`; driver 2.x stored it as a string. The serialization examples show it.
  - `FindOptions.Modifiers` and the `$explain` modifier are gone; `Explain` runs the `explain` command.
  - `JsonOutputMode.Strict` is gone; `RelaxedExtendedJson` replaces it in `TestTypes` and the loaders.
- **Server**: the shell scripts are for `mongosh`, and index builds no longer need `{ background: true }`.
- **Names**: `SpetialOffer` is now `SpecialOffer`, so the collection is `specialOffer` (run `seed` again).

## Tests

`dotnet test --solution SimpleTalkMongoDb.slnx` seeds a MongoDB in a container ([Testcontainers](https://dotnet.testcontainers.org/)) and runs every example except the loaders. It needs Docker, or Podman with its Docker-compatible API; with Podman, set `TESTCONTAINERS_RYUK_DISABLED=true`.

## Data

The sample data comes from Microsoft's AdventureWorks2016 sample database.
