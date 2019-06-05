using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace SimpleTalkMongoDb.Pocos
{
    public class Inventory
    {
        public ObjectId Id { get; set; }
        public string Item;
        public string Status;
        public int Qty;
        public Size S;
    }

    public class Size
    {
        public decimal H { get; set; }
        public  decimal W { get; set; }
        public  MeasureType Uom { get; set; }
    }

    public enum MeasureType
    {
        In, 
        Cm
    }
}
