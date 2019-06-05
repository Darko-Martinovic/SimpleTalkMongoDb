# SimpleTalkMongoDb
Programming MongoDB v4.0.9 by using .NET driver v2.7 & .NET framework 4.8. 

## :white_check_mark: Aggregation

Aggregation operations group values from multiple documents together and can perform a variety of operations on
the grouped data to return a single result. MongoDB provides three ways to perform aggregation: 
•	the aggregation pipeline, 
•	the single purpose aggregation methods,
•	the map-reduce function


Class Name                                       | Description
-------------------------------------------------|---------------------------------------------------------------------------------
AggregationSales                                 | Shows how to performs basic aggregation by using LINQ, IAggregateFluent and more...
AggregationUnwind                                | Shows verious aggregation on embedded documents by applying Unwind operator.
SampleLookup                                     | Shows how to combine two collections by applying Lookup operator.
SampleUnwind                                     | Shows how to use Unwind operator.

## :white_check_mark: Authentication

Class Name                                       | Description
-------------------------------------------------|---------------------------------------------------------------------------------
Auth                                             | Shows how to conect to MongoDB

## :white_check_mark: Configuration

Class Name                                       | Description
-------------------------------------------------|---------------------------------------------------------------------------------
SampleConfig                                     | Unlike ADO.NET, MongoDB client is a cheap resource. 

## :white_check_mark: CRUD operation

Class Name                                       | Description
-------------------------------------------------|---------------------------------------------------------------------------------
CrudDemo                                         | Shows how to performs basics CRUD operation. 

## :white_check_mark: Filtering

Class Name                                       | Description
-------------------------------------------------|---------------------------------------------------------------------------------
Explain                                          | Shows how to get the query plan from the code.
FilterDetails                                    | Shows how to query for a documement nested in an array
FilterForNullOrMissingFields                     | Shows hot wo query for Null or Missing Fields
FilterHeader                                     | Shows various way to query MongoDB documents.
FindWithCursor                                   | Shows how to use cursors

## :white_check_mark: Loaders

Class Name                                       | Description
-------------------------------------------------|---------------------------------------------------------------------------------
CreateJsonProducts                               | Shows how to create JSON file that holds information about products.
CreateJsonSales                                  | Shows how to create JSON file that holds information about sales.
CreateJsonSpetialOffer                           | Shows how to create JSON file that holds information about spetial offers.

## :white_check_mark: Serialization

Class Name                                       | Description
-------------------------------------------------|---------------------------------------------------------------------------------
AttributeDescoration                             | Shows how to use attributes in order to customize serialization process.
ClassMap                                         | Shows how to use ClassMap.
ClassMapAutoMap                                  | Shows how to use ClassMap with AutoMap feature.
TestConventionPack                               | Shows how to use Convention Pack.
TestTypes                                        | Shows various option within serialization.
