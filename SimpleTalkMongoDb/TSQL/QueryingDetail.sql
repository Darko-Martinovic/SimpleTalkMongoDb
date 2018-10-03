--Top n per group problem
--Let's find top one product per spetial offer that has the greatest LineTotal
--and then limit the result to those spetial offers and products where sum of LineTotal is greater than the Limit
--The result should be sorted on Sum of LineTotal descanding.
--The result should contain only three records. 

-- Cross Apply in Detail
DECLARE @limit MONEY = 200000
;
WITH x
AS
(SELECT
	   SpecialOfferID
	  ,ProductID
	  ,SUM(LineTotal) amax
    FROM sales.SalesOrderDetail
    GROUP BY SpecialOfferID
		  ,ProductID),
y
AS
(SELECT
	   c.SpecialOfferID
	  ,a.ProductID MaxProduct
	  ,a.amax MaxTotal
    FROM x C
    CROSS APPLY (SELECT TOP (1)
		  *
	   FROM x AS O
	   WHERE O.SpecialOfferID = C.SpecialOfferID
	   ORDER BY amax DESC) AS A)
SELECT DISTINCT
    SpecialOfferID
   ,MaxProduct
   ,MaxTotal
FROM y
WHERE MaxTotal > @limit
ORDER BY y.MaxTotal DESC;

