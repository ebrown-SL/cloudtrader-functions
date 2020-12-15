using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using CloudTrader.Functions.ApiClients;

[assembly: FunctionsStartup(typeof(CloudTrader.Functions.Startup))]

namespace CloudTrader.Functions
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