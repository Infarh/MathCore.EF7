using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Domain;
using MathCore.EF7.Clients;
using MathCore.EF7.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ConsoleClientTest
{
    internal class Program
    {
        private static IHost __Hosting;

        public static IHost Hosting => __Hosting ??= 
            CreateHostBuilder(Environment.GetCommandLineArgs())
           .Build();

        public static IHostBuilder CreateHostBuilder(string[] args) => Host
           .CreateDefaultBuilder(args)
           .ConfigureServices(ConfigureServices);

        private static void ConfigureServices(HostBuilderContext host, IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(WebRepository<>))
               .AddScoped(typeof(IRepository<,>), typeof(WebRepository<,>))
               .AddScoped<IRepository<Student>,TestStudentApiClient>();
        }

        public static async Task Main(string[] args)
        {
            using var host = Hosting;
            await host.StartAsync();
            

            Console.WriteLine("Hello World!");

            var rep = host.Services.GetRequiredService<IRepository<Student>>();
            try
            {
                var count =await rep.GetCount().ConfigureAwait(false);
                Console.WriteLine($"число строк в таблице {nameof(Student)} = {count}");
                var students = await rep.GetAll();
                foreach (var student in students)
                    Console.WriteLine($"{student.Id} {student.Name} {student.Age}");

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            await host.StopAsync();

            Console.WriteLine("Completed.");
        }
    }

    public class TestStudentApiClient : WebRepository<Student>, IRepository<Student>
    {
        public TestStudentApiClient(IConfiguration configuration, ILogger<WebRepository<Student>> logger) : base(configuration, logger/*,"api/Students"*/) //раскомментировать для замены точки
        {
        }

        public TestStudentApiClient(IConfiguration configuration, ILogger<WebRepository<Student>> logger, string serviceAddress) : base(configuration, logger, serviceAddress)
        {
        }
    }
}
