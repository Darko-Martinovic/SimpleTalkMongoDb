
// Indexes build without blocking other operations since MongoDB 4.2, so the old
// { background: true } option is ignored and no longer needed.
db.adventureWorks2016.createIndex({ TerritoryId: 1, DueDate: 1 });
