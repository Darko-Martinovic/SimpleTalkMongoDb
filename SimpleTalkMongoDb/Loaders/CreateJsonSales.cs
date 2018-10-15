using MongoDB.Bson;
using MongoDB.Bson.IO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using SimpleTalkMongoDb.Pocos;


namespace SimpleTalkMongoDb.Loaders
{
    internal static class Program
    {

        public static void Main(string[] args)
        {
            var t1 = DateTime.Now;
            Worker();
            var ts = DateTime.Now - t1;
            Console.WriteLine($"Total time elapsed {ts.Hours}:{ts.Minutes}:{ts.Seconds}");
            Console.WriteLine("Press any key to exit!");
            //Worker2();
            Console.ReadLine();
        }


        private static void Worker()

        {
            var ds = Common.GetDataSet();


            var counter = ds.Tables[0].Rows.Count;
            var k = 1;

            var result = new List<SalesHeader>();

            foreach (DataRow r in ds.Tables[0].Rows)
            {
                Console.WriteLine($"Processing record {k}/{counter}");
                var i = SalesHeaderPoco(r);


                var details = ds.Tables[1].AsEnumerable()
                    .Where(row => row.Field<int>("SalesOrderId") == i.SalesOrderId);

                SalesDetailPocos(details, r, i);

                k++;
                result.Add(i);
            }
            var ws = new JsonWriterSettings
            {
                OutputMode = JsonOutputMode.Strict,
                Indent = true,
                IndentChars = "\t",
                //NewLineChars = "\r\n", default
                GuidRepresentation = GuidRepresentation.CSharpLegacy

            };
            File.WriteAllText(@"C:\tmp\Sales.json", result.ToJson(ws));
        }

        private static void SalesDetailPocos(IEnumerable<DataRow> details, DataRow r, SalesHeader i)
        {
            foreach (var d in details)
            {
                var detail = new SalesDetail
                {
                    SalesOrderDetailId = (int) d["SalesOrderDetailID"],
                    OrderQty = (short) d["OrderQty"],
                    ProductId = (int) d["ProductID"],
                    SpecialOfferId = (int) d["SpecialOfferID"],
                    UnitPrice = (decimal) d["UnitPrice"],
                    UnitPriceDiscount = (decimal) d["UnitPriceDiscount"],
                    LineTotal = (decimal) d["LineTotal"],
                    Rowguid = r["rowguid"].ToString(),
                    ModifiedDate = (DateTime) r["ModifiedDate"]
                };

                if (!d.IsNull("CarrierTrackingNumber"))
                {
                    detail.CarrierTrackingNumber = (string) d["CarrierTrackingNumber"];
                }

                if (i.Details == null)
                {
                    i.Details = new List<SalesDetail>();
                }

                i.Details.Add(detail);
            }
        }

        private static SalesHeader SalesHeaderPoco(DataRow r)
        {
            var i = new SalesHeader
            {
                SalesOrderId = (int) r["SalesOrderId"],
                RevisionNumber = (byte) r["RevisionNumber"],
                OrderDate = (DateTime) r["OrderDate"],
                DueDate = (DateTime) r["DueDate"],
                ShipDate = (DateTime) r["ShipDate"],
                Status = (byte) r["Status"],
                OnlineOrderFlag = (bool) r["OnlineOrderFlag"],
                SalesOrderNumber = (string) r["SalesOrderNumber"],
                CustomerId = (int) r["CustomerId"],
                BillToAddressId = (int) r["BillToAddressId"],
                ShipToAddressId = (int) r["ShipToAddressId"],
                ShipMethodId = (int) r["ShipMethodID"],
                SubTotal = (decimal) r["SubTotal"],
                TaxAmt = (decimal) r["TaxAmt"],
                Freight = (decimal) r["Freight"],
                TotalDue = (decimal) r["TotalDue"],
                Rowguid = r["rowguid"].ToString(),
                ModifiedDate = (DateTime) r["ModifiedDate"]
            };
            if (!r.IsNull("PurchaseOrderNumber"))
                i.PurchaseOrderNumber = (string) r["PurchaseOrderNumber"];
            if (!r.IsNull("AccountNumber"))
                i.AccountNumber = (string) r["AccountNumber"];
            if (!r.IsNull("TerritoryId"))
                i.TerritoryId = (int) r["TerritoryId"];
            if (!r.IsNull("SalesPersonID"))
                i.SalesPersonId = (int) r["SalesPersonID"];
            if (!r.IsNull("CreditCardID"))
                i.CreditCardId = (int) r["CreditCardID"];
            if (!r.IsNull("CreditCardApprovalCode"))
                i.CreditCardApprovalCode = (string) r["CreditCardApprovalCode"];
            if (!r.IsNull("CurrencyRateID"))
                i.CurrencyRateId = (int) r["CurrencyRateID"];
            if (!r.IsNull("Comment"))
                i.Comment = (string) r["Comment"];
            return i;
        }

        //private static void Worker2()
        //{
        //    var ds = GetDataSet();
        //    var ws = new JsonWriterSettings
        //    {
        //        OutputMode = JsonOutputMode.Strict,
        //        Indent = true,
        //        IndentChars = "\t",
        //        //NewLineChars = "\r\n", default
        //        GuidRepresentation = GuidRepresentation.CSharpLegacy

        //    };

        //    var dt = ds.Tables[0];

        //    var result = new List<SalesHeader>();
        //    var counter = 0;

        //    Parallel.ForEach(dt.AsEnumerable(), r =>
        //    {
        //        var progress = Interlocked.Increment(ref counter);
        //        Console.WriteLine("Processing row {0}", progress);

        //        var i = new SalesHeader
        //        {
        //            SalesOrderId = (int)r["SalesOrderId"],
        //            RevisionNumber = (byte)r["RevisionNumber"],
        //            OrderDate = (DateTime)r["OrderDate"],
        //            DueDate = (DateTime)r["DueDate"],
        //            ShipDate = (DateTime)r["ShipDate"],
        //            Status = (byte)r["Status"],
        //            OnlineOrderFlag = (bool)r["OnlineOrderFlag"],
        //            SalesOrderNumber = (string)r["SalesOrderNumber"],
        //            CustomerId = (int)r["CustomerId"],
        //            BillToAddressId = (int)r["BillToAddressId"],
        //            ShipToAddressId = (int)r["ShipToAddressId"],
        //            ShipMethodId = (int)r["ShipMethodID"],
        //            SubTotal = (decimal)r["SubTotal"],
        //            TaxAmt = (decimal)r["TaxAmt"],
        //            Freight = (decimal)r["Freight"],
        //            TotalDue = (decimal)r["TotalDue"],
        //            Rowguid = r["rowguid"].ToString(),
        //            ModifiedDate = (DateTime)r["ModifiedDate"]
        //        };
        //        if (!r.IsNull("PurchaseOrderNumber"))
        //            i.PurchaseOrderNumber = (string)r["PurchaseOrderNumber"];
        //        if (!r.IsNull("AccountNumber"))
        //            i.AccountNumber = (string)r["AccountNumber"];
        //        if (!r.IsNull("TerritoryId"))
        //            i.TerritoryId = (int)r["TerritoryId"];
        //        if (!r.IsNull("SalesPersonID"))
        //            i.SalesPersonId = (int)r["SalesPersonID"];
        //        if (!r.IsNull("CreditCardID"))
        //            i.CreditCardId = (int)r["CreditCardID"];
        //        if (!r.IsNull("CreditCardApprovalCode"))
        //            i.CreditCardApprovalCode = (string)r["CreditCardApprovalCode"];
        //        if (!r.IsNull("CurrencyRateID"))
        //            i.CurrencyRateId = (int)r["CurrencyRateID"];
        //        if (!r.IsNull("Comment"))
        //            i.Comment = (string)r["Comment"];



        //        var details = ds.Tables[1].AsEnumerable()
        //            .Where(row => row.Field<int>("SalesOrderId") == i.SalesOrderId);

        //        foreach (var d in details)
        //        {
        //            var detail = new SalesDetail
        //            {
        //                SalesOrderDetailId = (int)d["SalesOrderDetailID"],
        //                OrderQty = (short)d["OrderQty"],
        //                ProductId = (int)d["ProductID"],
        //                SpecialOfferId = (int)d["SpecialOfferID"],
        //                UnitPrice = (decimal)d["UnitPrice"],
        //                UnitPriceDiscount = (decimal)d["UnitPriceDiscount"],
        //                LineTotal = (decimal)d["LineTotal"],
        //                Rowguid = r["rowguid"].ToString(),
        //                ModifiedDate = (DateTime)r["ModifiedDate"]
        //            };

        //            if (!d.IsNull("CarrierTrackingNumber"))
        //            {
        //                detail.CarrierTrackingNumber = (string)d["CarrierTrackingNumber"];
        //            }
        //            if (i.Details == null)
        //            {
        //                i.Details = new List<SalesDetail>();
        //            }
        //            i.Details.Add(detail);
        //        }
        //        result.Add(i);


        //    });
        //    File.WriteAllText(@"C:\tmp\Sales.json", result.ToJson(ws));
        //}
    }
}
