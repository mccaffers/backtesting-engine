using System.Threading.Tasks.Dataflow;
using backtesting_engine;
using backtesting_engine.interfaces;
using backtesting_engine_operations;
using backtesting_engine_strategies;

namespace backtesting_engine_ingest;

public class Consumer : IConsumer
{
    readonly IEnumerable<IStrategy>? strategies;
    readonly IPositions positions;

    public Consumer(IEnumerable<IStrategy> strategies, IPositions positions) {

        this.strategies = strategies;
        this.positions = positions;
    }

    private DateTime lastReceived = DateTime.Now;
    private DateTime priceTimeWindow = DateTime.MinValue;

    public async Task ConsumeAsync(BufferBlock<PriceObj> buffer, CancellationToken cts)
    {
        while (await buffer.OutputAvailableAsync())
        {
            // Cancel this task if a cancellation token is received
            cts.ThrowIfCancellationRequested();

            // Get the symbol data off the buffer
            var priceObj = await buffer.ReceiveAsync();
            
        
            // take in 1 hour (priceObj) over 100 millisecionds (local)
            if(priceTimeWindow==DateTime.MinValue){
                priceTimeWindow=priceObj.date;
            }

            await CacheRequests(priceObj);
        }
    }

    private async Task CacheRequests(PriceObj priceObj){
        // Clock the local time, check it's under 100 milliseconds
        if(DateTime.Now.Subtract(lastReceived).TotalSeconds <= 0.4){

            // Have we sent an hour
            if(priceObj.date.Subtract(priceTimeWindow).TotalMinutes<20){
                await ProcessTick(priceObj);
            } else {
                await Task.Delay(100);
                await CacheRequests(priceObj);
            }

        } else {
            lastReceived=DateTime.Now;
            priceTimeWindow=priceObj.date;
            await CacheRequests(priceObj);
        }
    }

    private async Task ProcessTick(PriceObj priceObj){

        // Invoke all the strategies defined in configuration
        foreach (var i in strategies ?? Array.Empty<IStrategy>())
        {
            await i.Invoke(priceObj);
        }

        // Review open positions, check if the new symbol data meets the threshold for LIMI/STOP levels
        await this.positions.Review(priceObj);
        this.positions.TrailingStopLoss(priceObj);
        this.positions.ReviewEquity();
        this.positions.PushRequests(priceObj);
    }

}

public static class PropertyCopier<TParent, TChild> where TParent : class
                                            where TChild : class
{
    public static void Copy(TParent parent, TChild child)
    {
        var parentProperties = parent.GetType().GetProperties();
        var childProperties = child.GetType().GetProperties();
        foreach (var parentProperty in parentProperties)
        {
            foreach (var childProperty in childProperties)
            {
                if (parentProperty.Name == childProperty.Name && parentProperty.PropertyType == childProperty.PropertyType)
                {
                    childProperty.SetValue(child, parentProperty.GetValue(parent));
                    break;
                }
            }
        }
    }
}