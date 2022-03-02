using System.Collections.Concurrent;
using System.Globalization;
using System.Threading.Tasks.Dataflow;
using backtesting_engine;
using backtesting_engine_models;
using backtesting_engine_operations;
using Newtonsoft.Json;
using Report;
using Utilities;

namespace backtesting_engine_ingest;

public interface IConsumer
{
    Task ConsumeAsync(BufferBlock<PriceObj> buffer, CancellationToken cts);
    void ReviewEquity();
}

public class Consumer : IConsumer
{

    public async Task ConsumeAsync(BufferBlock<PriceObj> buffer, CancellationToken cts)
    {
        while (await buffer.OutputAvailableAsync())
        {
            cts.ThrowIfCancellationRequested();

            var priceObj = await buffer.ReceiveAsync();
            RandomStrategy.Invoke(priceObj);
            Positions.Review(priceObj);
            ReviewEquity();
        }
    }

    public void ReviewEquity()
    {
        
        if (Program.accountObj.hasAccountExceededDrawdownThreshold())
        {
            // close all trades
            Positions.CloseAll();
            
            // trigger final report
            Reporting.EndOfRunReport(Program.accountObj);

            // stop any more trades
            throw new Exception("Exceeded threshold PL:"+ Program.accountObj.pnl);
        }
    }

    // need to rename, it's not drawndown but percent change?
    

}

public class PropertyCopier<TParent, TChild> where TParent : class
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