using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MathCore.EF7.Clients;
using MathCore.EF7.Interfaces.Repositories;
using Microsoft.Extensions.Configuration.Memory;

namespace BlazorTestApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddScoped(typeof(IRepository<>), typeof(WebRepository<>))
               .AddScoped(typeof(IRepository<,>), typeof(WebRepository<,>));

            var config = new Dictionary<string, string> { { "ClientAddress", "https://localhost:44369" } };
            builder.Configuration.Add(new MemoryConfigurationSource { InitialData = config });

            await builder.Build().RunAsync();
        }
    }
}
