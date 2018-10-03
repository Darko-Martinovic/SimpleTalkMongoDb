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

