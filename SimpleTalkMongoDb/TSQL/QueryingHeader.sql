--Top n per group problem
--Let's find top one customer per territory that has the greatest TotalDue
--and then limit the result to those territories and customers where sum of TotalDue is greater than the Limit
--The result should be sorted on Sum of TotalDue descending.
--The result should contain only three records.

---1. Using cross apply in the Master table
--  Find max-min customer per teritory 
DECLARE @limit MONEY = 950000
;
WITH x
AS
(SELECT
	   TerritoryID
	  ,CustomerID
	  ,SUM(TotalDue) Amax
    FROM Sales.SalesOrderHeader
    GROUP BY TerritoryID
		  ,CustomerID)
SELECT
DISTINCT
    x.TerritoryID
   ,a.CustomerID MaxCustomer
   ,a.Amax MaxTotal
FROM x
CROSS APPLY (SELECT TOP (1)
	   *
    FROM x O
    WHERE O.TerritoryID = x.TerritoryID
    ORDER BY AMax DESC) AS A
WHERE a.Amax > @limit
ORDER BY a.Amax DESC;

-- 2. Using first_value
;
WITH x
AS
(SELECT
	   TerritoryID
	  ,CustomerID
	  ,SUM(TotalDue) Amax
    FROM Sales.SalesOrderHeader
    GROUP BY TerritoryID
		  ,CustomerID),
x1
AS
(SELECT
    DISTINCT
	   x.TerritoryID
	  ,FIRST_VALUE(CustomerID) OVER (PARTITION BY TerritoryId
	   ORDER BY Amax DESC
	   ) AS MaxCustomer
	  ,FIRST_VALUE(Amax) OVER (PARTITION BY TerritoryId
	   ORDER BY Amax DESC
	   ) AS MaxTotal
    FROM x)
SELECT
    TerritoryID
   ,MaxCustomer
   ,MaxTotal
FROM X1
WHERE MaxTotal > @limit
ORDER BY MaxTotal DESC;

--- 3. Using row_number
;
WITH x
AS
(SELECT
	   TerritoryID
	  ,CustomerID
	  ,SUM(TotalDue) amax
    FROM sales.SalesOrderHeader
    GROUP BY TerritoryID
		  ,CustomerID),
Y1
AS
(SELECT
	   *
	  ,ROW_NUMBER() OVER (PARTITION BY X.TerritoryID
	   ORDER BY X.AMAX DESC) rkmax
    FROM X)
SELECT
    Y1.TerritoryID
   ,Y1.CustomerID MaxCustomer
   ,y1.amax MAXTotal
FROM Y1
WHERE rkMax = 1
AND Y1.amax > @limit
ORDER BY MAXTotal DESC;



