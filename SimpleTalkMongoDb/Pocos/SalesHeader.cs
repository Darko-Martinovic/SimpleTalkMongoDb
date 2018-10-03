using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SimpleTalkMongoDb.Pocos
{
   
    public class SalesHeader 
    {
        [BsonId]
        public int SalesOrderId { get; set; }

        public byte RevisionNumber { get; set; }


        //[BsonDateTimeOptions(DateOnly = true,Kind = DateTimeKind.Local)]
        public DateTime OrderDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime ShipDate { get; set; }
        //
        public byte Status { get; set; }
        public bool OnlineOrderFlag { get; set; }
        public string SalesOrderNumber { get; set; }
        public string PurchaseOrderNumber { get; set; }
        public string AccountNumber { get; set; }
        //
        public int CustomerId { get; set; }
        public int SalesPersonId { get; set; }
        public int TerritoryId { get; set; }
        public int BillToAddressId { get; set; }
        public int ShipToAddressId { get; set; }
        //
        public int ShipMethodId { get; set; }
        public int CreditCardId { get; set; }
        public string CreditCardApprovalCode { get; set; }
        public int CurrencyRateId { get; set; }

        //BsonSerializer(typeof(CustomSerializationProvider))]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal SubTotal { get; set; }
        //
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal TaxAmt { get; set; }

        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Freight { get; set; }

        [BsonRepresentation(BsonType.Decimal128)]
        public decimal TotalDue { get; set; }

        public string Comment { get; set; }

        public string Rowguid { get; set; }
        //
        public DateTime ModifiedDate { get; set; }


        public List<SalesDetail> Details { get; set; }

    }
    [BsonIgnoreExtraElements]
    public class SalesDetail
    {

        public int SalesOrderDetailId { get; set; }
        public string CarrierTrackingNumber { get; set; }
        public short OrderQty { get; set; }
        public int ProductId { get; set; }
        public int SpecialOfferId { get; set; }

        [BsonRepresentation(BsonType.Decimal128)]
        public decimal UnitPrice { get; set; }

        [BsonRepresentation(BsonType.Decimal128)]
        public decimal UnitPriceDiscount { get; set; }

        [BsonRepresentation(BsonType.Decimal128)]
        public decimal LineTotal { get; set; }

        public string Rowguid { get; set; }
        //
        public DateTime ModifiedDate { get; set; }


    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public  class SalesDetailHelper
    {

        public SalesDetail Details { get; set; }



    }

    public class SalesDetailHelper2
    {
  
        public int Id { get; set; }
        public SalesDetail Details { get; set; }



    }
}
