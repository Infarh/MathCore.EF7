using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;
using MathCore.EF7.Clients;
using MathCore.EF7.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
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
               .AddScoped<IRepository<Student>,TestStudentApiClient>()
               .AddScoped(typeof(IPage<>), typeof(PageItems<>));
        }
        /// <summary> Реализация интерфейса постраничных данных </summary>
        private record PageItems<TEntity>(IEnumerable<TEntity> Items, int TotalCount, int PageNumber, int PageSize) : IPage<TEntity>
        {
            ///// <summary>Полное число страниц в выдаче</summary>
            //public int TotalPagesCount => PageSize <= 0 ? TotalCount : (int)Math.Ceiling((double)TotalCount / PageSize);
            ///// <summary>Существует ли предыдущая страница</summary>
            //public bool HasPrevPage => PageNumber > 0;
            ///// <summary>Существует ли следующая страница</summary>
            //public bool HasNextPage => PageNumber < TotalPagesCount - 1;//отсчёт от 0 страницы
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
                if (count > 0)
                {
                    var page = await rep.GetPage(0, 3);
                    Console.WriteLine($"{nameof(page.TotalCount)}={page.TotalCount}");
                    Console.WriteLine($"{nameof(page.PageNumber)}={page.PageNumber}");
                    Console.WriteLine($"{nameof(page.PageSize)}={page.PageSize}");
                    Console.WriteLine($"{nameof(page.TotalPagesCount)}={page.TotalPagesCount}");
                    Console.WriteLine($"{nameof(page.HasPrevPage)}={page.HasPrevPage}");
                    Console.WriteLine($"{nameof(page.HasNextPage)}={page.HasNextPage}");
                }

                var del1 = await rep.DeleteById(1);
                Console.WriteLine($"Delete id=1 : {del1 is not null}");
                var del2 = await rep.DeleteById(5);

                Console.WriteLine($"Delete id=5 : {del2 is not null}");

                var stud = new Student()
                {
                    Id = 2
                };
                var del3 = await rep.Delete(stud);
                Console.WriteLine($"Delete new Student with id = 2 : {del3 is not null}");

                var stud2 = new Student()
                {
                    Id = 6
                };
                var del4 = await rep.Delete(stud2);
                Console.WriteLine($"Delete new Student with id = 6 : {del4 is not null}");

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
