using backtesting_engine_strategies;
using Microsoft.Extensions.DependencyInjection;

namespace Utilities;

public static class ServiceExtension {

    public static IServiceCollection RegisterStrategies(this IServiceCollection services)
    {
        foreach(var i in EnvironmentVariables.strategy.Split(",")){
            var _type = Type.GetType("backtesting_engine_strategies." + i) ?? default(Type);
            if(_type is not null && typeof(IStrategy).IsAssignableFrom(_type) ){
                services.AddSingleton(typeof(IStrategy), _type);
            }
        }

       if (!services.Any(x => x.ServiceType == typeof(IStrategy))){
           throw new ArgumentException("No Strategies defined");
       }

        return services;
    }
}
