using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace SimpleTalkMongoDb.Pocos
{
    public class Items
    {
        public ObjectId Id { get; set; }
        public string Item;
        public List<Instock> Details { get; set; }
    }

    public class InStockHelper
    {

        public Instock Details { get; set; }
        public string Item { get; set; }



    }
    public class Instock
    {
        public Instock(string wWare, int qty)
        {
            WareHouse = wWare;
            Qty = qty;
        }
        public string WareHouse { get; set; }
        public int Qty { get; set; }
    }
}
