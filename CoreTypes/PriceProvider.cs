using System;

namespace CoreTypes
{
    public class PriceProvider
    {
        public decimal Bid=-1, Ask=-1, LastPrice=-1;
        public int BidSize=-1, AskSize=-1, LastSize=-1;
        public DateTime BidTime=DateTime.MinValue, AskTime=DateTime.MinValue, LastTime=DateTime.MinValue;
        
        public void Update(DateTime dt, TickInfo ti)
        {
            switch (ti.Tag)
            {
                case 0:
                    BidSize = (int)ti.Value;
                    BidTime = dt;
                    break;
                case 1:
                    Bid = (decimal)ti.Value;
                    BidTime = dt;
                    break;
                case 2:
                    Ask = (decimal)ti.Value;
                    AskTime = dt;
                    break;
                case 3:
                    AskSize = (int)ti.Value;
                    AskTime = dt;
                    break;
                case 4:
                    LastPrice = (decimal)ti.Value;
                    LastTime = dt;
                    break;
                case 5:
                    LastSize = (int)ti.Value;
                    LastTime = dt;
                    break;
            }
        }

        public (decimal bid, decimal ask, decimal last) LastPrices => (Bid, Ask, LastPrice);
    }
}
