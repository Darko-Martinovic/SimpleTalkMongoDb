// Shows the execution plan
db.adventureWorks2016.explain("executionStats").aggregate([{ "$group": { "_id": { "TerritoryId": "$TerritoryId", "CustomerId": "$CustomerId" }, "TotalDue": { "$sum": "$TotalDue" } } },
    { "$sort": { "TotalDue": 1 } }, { "$group": { "_id": "$_id.TerritoryId", "MaxCustomer": { "$last": "$_id.CustomerId" }, "MaxTotal": { "$last": "$TotalDue" } } }, { "$match": { "MaxTotal": { "$gt": NumberDecimal("950000") } } }, { "$project": { "TerritoryId": "$_id", "MaxCust": { "Id": "$MaxCustomer", "Total": "$MaxTotal" }, "_id": 0 } }, { "$sort": { "MaxCust.Total": -1 } }])

//allPlansExecution
db.adventureWorks2016.explain("allPlansExecution").aggregate([{ "$group": { "_id": { "TerritoryId": "$TerritoryId", "CustomerId": "$CustomerId" }, "TotalDue": { "$sum": "$TotalDue" } } },
    { "$sort": { "TotalDue": 1 } }, { "$group": { "_id": "$_id.TerritoryId", "MaxCustomer": { "$last": "$_id.CustomerId" }, "MaxTotal": { "$last": "$TotalDue" } } }, { "$match": { "MaxTotal": { "$gt": NumberDecimal("950000") } } }, { "$project": { "TerritoryId": "$_id", "MaxCust": { "Id": "$MaxCustomer", "Total": "$MaxTotal" }, "_id": 0 } }, { "$sort": { "MaxCust.Total": -1 } }])

// Shows the result
db.adventureWorks2016.aggregate([{ "$group": { "_id": { "TerritoryId": "$TerritoryId", "CustomerId": "$CustomerId" }, "TotalDue": { "$sum": "$TotalDue" } } },
    { "$sort": { "TotalDue": 1 } }, { "$group": { "_id": "$_id.TerritoryId", "MaxCustomer": { "$last": "$_id.CustomerId" }, "MaxTotal": { "$last": "$TotalDue" } } }, { "$match": { "MaxTotal": { "$gt": NumberDecimal("950000") } } }, { "$project": { "TerritoryId": "$_id", "MaxCust": { "Id": "$MaxCustomer", "Total": "$MaxTotal" }, "_id": 0 } }, { "$sort": { "MaxCust.Total": -1 } }])