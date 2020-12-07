using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Traders.Functions.ApiClients;

[assembly: FunctionsStartup(typeof(Traders.Functions.Startup))]

namespace Traders.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();

            builder.Services.AddSingleton<ITradersApiClient, TradersApiHttpClient>();
        }
    }
}