using System;
using DAL.Context;
using DAL.Repositories;
using MathCore.EF7.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DAL.Service
{
    public static class Registrator
    {
        public static IServiceCollection AddTestRepositories(this IServiceCollection services) =>
            services
               .AddScoped(typeof(IRepository<>), typeof(Test_DbRepository<>));

        public static IServiceCollection AddTestRepositoryFactories(this IServiceCollection services) =>
            services
               .AddScoped(typeof(IRepository<>), typeof(Test_DbContextFactoryRepository<>));
    }
    public static class ServicesExtensions
    {
        /// <summary>Получить контекст БД RRJ-Express</summary>
        /// <param name="services">Провайдер сервисов</param>
        /// <returns>Контекст БД</returns>
        public static TestContext GetTestContext(this IServiceProvider services) => services.GetRequiredService<TestContext>();

        /// <summary>Получить фабрику контекстов БД RRJ-Express</summary>
        /// <param name="services">Провайдер сервисов</param>
        /// <returns>Фабрика контекстов БД RRJ-Express</returns>
        public static IDbContextFactory<TestContext> GetTestContextFactory(this IServiceProvider services) =>
            services.GetRequiredService<IDbContextFactory<TestContext>>();

        /// <summary>Получить контекст БД RRJ-Express</summary>
        /// <param name="scope">Область видимости провайдера сервисов</param>
        /// <returns>Контекст БД</returns>
        public static TestContext GetTestContext(this IServiceScope scope) => scope.ServiceProvider.GetTestContext();

        /// <summary>Получить фабрику контекстов БД RRJ-Express</summary>
        /// <param name="scope">Область видимости провайдера сервисов</param>
        /// <returns>Фабрика контекстов БД RRJ-Express</returns>
        public static IDbContextFactory<TestContext> GetTestContextFactory(this IServiceScope scope) => scope.ServiceProvider.GetTestContextFactory();
    }
}
