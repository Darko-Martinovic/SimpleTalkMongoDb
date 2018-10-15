// Let's find all documents that have TerritoryId 1 or 2, Total Due greater than 10000 and Due Date greater or equal to 01/01/2014
// Then sort the result descending  by Total Due and Order Date
// and finally, use just the first three documents.
db.adventureWorks2016.find({ "$or": [{ "TerritoryId": 1 }, { "TerritoryId": 2 }], "TotalDue": { "$gt": NumberDecimal("10000") }, "DueDate": { "$gte": ISODate("2013-12-31T23:00:00Z") } }, { "_id": 1, "CustomerId": 1, "OrderDate": 1, "TotalDue": 1 }).sort({ "TotalDue": -1, "OrderDate": -1 }).limit(3);

//
// using explain to get the query plan
//
db.adventureWorks2016.explain("executionStats").find({ "$or": [{ "TerritoryId": 1 }, { "TerritoryId": 2 }], "TotalDue": { "$gt": NumberDecimal("10000") }, "DueDate": { "$gte": ISODate("2013-12-31T23:00:00Z") } }, { "_id": 1, "CustomerId": 1, "OrderDate": 1, "TotalDue": 1 }).sort({ "TotalDue": -1, "OrderDate": -1 }).limit(3);