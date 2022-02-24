using System.Collections.Concurrent;
using System.Globalization;
using System.Threading.Tasks.Dataflow;
using backtesting_engine;
using backtesting_engine_models;
using backtesting_engine_operations;
using Newtonsoft.Json;
using Utilities;

namespace backtesting_engine_ingest;

public interface IConsumer
{
    Task ConsumeAsync(BufferBlock<PriceObj> buffer, CancellationToken cts);
    void ReviewEquity();
}

public class Consumer : IConsumer
{

    private EnvironmentVariables env { get; }
    private decimal maximumDrawndownPercentage { get;} 
    private decimal accountEquity { get; }

    public Consumer(EnvironmentVariables? env = null)
    {
        this.env = env ?? new EnvironmentVariables(); // Allow injectable env variables
        this.accountEquity = decimal.Parse(this.env.Get("accountEquity"));
        this.maximumDrawndownPercentage = decimal.Parse(this.env.Get("maximumDrawndownPercentage"));
    }

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
        
        if (ExceededDrawdownThreshold())
        {
            // close all trades
            Positions.CloseAll();
            
            // trigger final report

            // stop any more trades
            throw new Exception("Exceeded threshold PL:"+ UpdatePL());
        }
    }

    public decimal UpdatePL(){
        var pl = Program.tradeHistory.Sum(x => x.Value.profit);
        return accountEquity + pl + Positions.GetOpenPL();
    }

    // need to rename, it's not drawndown but percent change?
    public bool ExceededDrawdownThreshold(){
        return (UpdatePL() < accountEquity*(1-(maximumDrawndownPercentage/100)));
    }

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