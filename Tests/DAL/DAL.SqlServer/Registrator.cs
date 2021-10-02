using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Context;
using MathCore.EF7.Contexts;
using MathCore.EF7.Interfaces.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DAL.SqlServer
{
    /// <summary>Регистратор сервисов слоя данных для SQL-сервера</summary>
    public static class Registrator
    {
        /// <summary>Добавить контекст данных в контейнер сервисов для подключения к SQL-серверу</summary>
        /// <param name="services">Коллекция сервисов</param>
        /// <param name="ConnectionString">Строка подключения к серверу</param>
        /// <returns>Коллекция сервисов</returns>
        public static IServiceCollection AddTestDbContextSqlServer(this IServiceCollection services, string ConnectionString) =>
            services
               .AddDbContext<TestContext>(opt => opt.UseSqlServer(ConnectionString, o => o.MigrationsAssembly(typeof(Registrator).Assembly.FullName)))
               .AddScoped<IDbInitializer, DBInitializer<TestContext>>();

        /// <summary>Добавить фабрику контекста данных в контейнер сервисов для подключения к SQL-серверу</summary>
        /// <param name="services">Коллекция сервисов</param>
        /// <param name="ConnectionString">Строка подключения к серверу</param>
        /// <returns>Коллекция сервисов</returns>
        public static IServiceCollection AddTestDbContextFactorySqlServer(this IServiceCollection services, string ConnectionString)
        {
            services.AddDbContextFactory<TestContext>(
                opt => opt.UseSqlServer(ConnectionString, o => o.MigrationsAssembly(typeof(Registrator).Assembly.FullName)));
            services.AddScoped<IDbInitializer, DBFactoryInitializer<TestContext>>();
            return services;
        }
    }
}
