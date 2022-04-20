using System;
using System.Collections.Generic;
using System.Linq;

namespace X1.IBKR
{

    public class MktDepthUpdate
    {
        public int tickerId { get; set; }
        public int position { get; set; }  // the order book's row being updated

        public int operation { get; set; } // 	how to udpate the order book :
        // 0 = insert (insert this new order into the row identified by 'position')· 
        // 1 = update (update the existing order in the row identified by 'position')· 
        // 2 = delete (delete the existing order at the row identified by 'position').

        public int side { get; set; }  // 0 for ask, 1 for bid
        public decimal price { get; set; }
        public decimal size { get; set; }
    }


    public class IBOrderBook
    {
        public List<List<decimal>> asks;
        public List<List<decimal>> bids;
        // public SortedDictionary<decimal, decimal> asks;
        // public SortedDictionary<decimal, decimal> bids;
        public IBOrderBook()
        {
            // asks = new SortedDictionary<decimal, decimal>();
            // bids = new SortedDictionary<decimal, decimal>();
            asks = new List<List<decimal>>{
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
            };
            bids = new List<List<decimal>>{
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
                new List<decimal>{ 0, 0 },
            };
        }

        public void update(List<string> msg)
        {
            // Console.WriteLine("====== ORDER BOOK DATA ======");
            // Console.WriteLine("tickerId: {0}", msg[0]);
            // Console.WriteLine("position: {0}", msg[1]);
            // Console.WriteLine("operation: {0}", msg[2]);
            // Console.WriteLine("side: {0}", msg[3]);
            // Console.WriteLine("price: {0}", msg[4]);
            // Console.WriteLine("size: {0}", msg[5]);
            // Console.WriteLine("");


            int operation = Convert.ToInt32(msg[2]);
            int side = Convert.ToInt32(msg[3]);

            int position = Convert.ToInt32(msg[1]);

            if (side == 0) // ask
            {
                // use position = (len - position) to reverse asks
                if (operation == 0)
                {
                    // 0 = insert (insert this new order into the row identified by 'position')·
                    asks.Insert((10 - position), new List<decimal> {
                        decimal.Parse(msg[4]), // price  
                        decimal.Parse(msg[5]) // size
                        });
                }
                else if (operation == 1)  // 1 = update 
                {
                    // 1 = update (update the existing order in the row identified by 'position')·
                    asks[10 - position][0] = decimal.Parse(msg[4]);
                    asks[10 - position][1] = decimal.Parse(msg[5]);
                }
                else if (operation == 2) // 2 = delete
                {
                    // 2 = delete (delete the existing order at the row identified by 'position').
                    asks.RemoveAt(10 - position);
                }
            }
            else
            { // bid
                if (operation == 0)
                {
                    // 0 = insert (insert this new order into the row identified by 'position')·
                    bids.Insert(position, new List<decimal> {
                       decimal.Parse(msg[4]), // price  
                        decimal.Parse(msg[5]) // size
                    });
                }
                else if (operation == 1)  // 1 = update 
                {
                    // 1 = update (update the existing order in the row identified by 'position')·
                    bids[position][0] = decimal.Parse(msg[4]);
                    bids[position][1] = decimal.Parse(msg[5]);
                }
                else if (operation == 2) // 2 = delete
                {
                    // 2 = delete (delete the existing order at the row identified by 'position').
                    bids.RemoveAt(position);
                }
            }


        }
    }


}