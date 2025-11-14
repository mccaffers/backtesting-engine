using backtesting_engine;
using backtesting_engine.interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Utilities;

public static class ServiceExtension {

    public static IServiceCollection RegisterStrategies(this IServiceCollection services)
    {
        foreach(var i in TradingVariables.STRATEGY.Value().Split(",")){
            var _type = Type.GetType("backtesting_engine_strategies." + i + ",strategies") ?? default(Type);
            if(_type is not null && typeof(IStrategy).IsAssignableFrom(_type) ){
                services.AddSingleton(typeof(IStrategy), _type);
            }
        }

        CheckStrategyExistsAndAreOfTypeIStragey(services);
        return services;
    }

    private static void CheckStrategyExistsAndAreOfTypeIStragey(IServiceCollection services){
        if (!services.Any(x => x.ServiceType == typeof(IStrategy))){
           throw new ArgumentException("No Strategies defined");
        }
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