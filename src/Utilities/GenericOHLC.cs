using backtesting_engine;

namespace Utilities
{

    public static class GenericOHLC
    {

        public static List<OHLCObject> CalculateOHLC(PriceObj priceObj, decimal price, TimeSpan duration, List<OHLCObject> OHLCArray) {

            // If the array is empty lets start populating
            if(OHLCArray.Count() == 0){
                OHLCArray.Add(new OHLCObject(){
                    date=priceObj.date,
                    open=price,
                    high=price,
                    low=price,
                    close=price
                });
            }

            var index = OHLCArray.Count()-1; // Get the last item

            // if we hit the minute threshold, build a new OHLC object
            var diff = priceObj.date.Subtract(OHLCArray[index].date).TotalMinutes;
            if(diff > duration.TotalMinutes){
                OHLCArray.Add(new OHLCObject(){
                    date=priceObj.date,
                    open=price,
                    high=price,
                    low=price,
                    close=price
                });
                index = OHLCArray.Count()-1; 
            }

            if(price > OHLCArray[index].high){
                OHLCArray[index].high=price;
            }

            if(price < OHLCArray[index].low){ 
                OHLCArray[index].low=price; 
            }

            OHLCArray[index].close=price; 

            return OHLCArray;
        }

    }
}