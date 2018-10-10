db.adventureWorks2016
    .find({
            "$or": [{ "TerritoryId": 1 }, { "TerritoryId": 2 }],
            "TotalDue": { "$gt": NumberDecimal("10000") },
            "DueDate": { "$gte": ISODate("2013-12-31T23:00:00Z") }
        },
        { "_id": 1, "CustomerId": 1, "OrderDate": 1, "TotalDue": 1 }).sort({ "TotalDue": -1, "OrderDate": -1 })
    .limit(3);
//"executionStats"
db.adventureWorks2016.explain("executionStats").find({ "$or": [{ "TerritoryId": 1 }, { "TerritoryId": 2 }], "TotalDue": { "$gt": NumberDecimal("10000") }, "DueDate": { "$gte": ISODate("2013-12-31T23:00:00Z") } }, { "_id": 1, "CustomerId": 1, "OrderDate": 1, "TotalDue": 1 }).sort({ "TotalDue": -1, "OrderDate": -1 }).limit(3);