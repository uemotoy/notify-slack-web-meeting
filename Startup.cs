using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(dcinc.jobs.Startup))]

namespace dcinc.jobs
{
  public class Startup : FunctionsStartup
  {
    public override void Configure(IFunctionsHostBuilder builder)
    {
      // https://github.com/Azure/azure-webjobs-sdk/wiki/Function-Filters
      // T.B.D 認可の確認でFilterできるか検証

      builder.Services.AddHttpClient();
    }
  }
}
