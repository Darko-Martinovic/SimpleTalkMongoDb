db.adventureWorks2016.createIndex(
    { CustomerId: 1, TerritoryId: 1, TotalDue: 1})
db.adventureWorks2016.createIndex(
    { TotalDue: -1 })

db.adventureWorks2016.createIndex(
    { TerritoryId: 1, DueDate: 1 })
