using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration.Memory;
using TestCommon;
using TestCommon.Service;

namespace BlazorServerWebApp.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddScoped<IStudentService, StudentsClient>();
            builder.Services.AddScoped<OdataClient>();

            var config = new Dictionary<string, string> { { "ClientAddress", "https://localhost:44369" } };
            builder.Configuration.Add(new MemoryConfigurationSource { InitialData = config });

            await builder.Build().RunAsync();
        }
    }
}
