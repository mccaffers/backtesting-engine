using backtesting_engine;

namespace Utilities
{

    public static class GenericOHLC
    {

        public static List<OHLCObject> CalculateOHLC(PriceObj price, TimeSpan duration, List<OHLCObject> OHLCArray) {

            // If the array is empty lets start populating
            if(OHLCArray.Count() == 0){
                OHLCArray.Add(new OHLCObject(){
                    date=price.date,
                    open=price.ask,
                    high=price.bid,
                    low=price.bid,
                    close=price.bid
                });
            }

            var index = OHLCArray.Count()-1; // Get the last item

            // if we hit the minute threshold, build a new OHLC object
            if((price.date - OHLCArray[index].date).TotalMinutes > duration.TotalMinutes){
                OHLCArray.Add(new OHLCObject(){
                    date=price.date,
                    open=price.ask,
                    high=price.ask,
                    low=price.ask,
                    close=price.ask
                });
                index = OHLCArray.Count()-1; 
            }

            if(price.ask > OHLCArray[index].high){
                OHLCArray[index].high=price.ask;
            }

            if(price.ask < OHLCArray[index].low){ 
                OHLCArray[index].low=price.ask; 
            }

            OHLCArray[index].close=price.ask; 

            return OHLCArray;
        }

    }
}