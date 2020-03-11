using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace MathCore.EF7.Extensions
{
    public static class DbSetExtensions
    {
        #region IdentityInsert

        /// <summary>Объект, обеспечивающий автоматизацию обратного переключения при вызове метода <see cref="IDisposable"/>.<see cref="IDisposable.Dispose()"/></summary>
        /// <typeparam name="T">Тип элементов данных набора</typeparam>
        private class DbSetIdentityInsert<T> : IDisposable where T : class
        {
            /// <summary>Имя таблицы, для которой осуществляется управление</summary>
            private readonly string _TableName;

            /// <summary>Контекст БД, осуществляющий связь с БД</summary>
            private readonly DbContext _Context;

            /// <summary>
            /// Инициализация нового экземпляра <see cref="DbSetIdentityInsert{T}"/>
            /// При завершении работы конструктора в БД выполняется SQL-команда SET IDENTITY_INSERT [dbo].[TableName] ON
            /// </summary>
            /// <param name="Set">Набор данных контекста</param>
            public DbSetIdentityInsert(DbSet<T> Set)
            {
                _Context = Set.GetContext();
                _TableName = Set.GetTableName();
                _Context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT [dbo].[{0}] ON", _TableName);
            }

            /// <summary>Вызов данного метода осуществляет выполнение SQL-команды SET IDENTITY_INSERT [dbo].[TableName] OFF</summary>
            public void Dispose() => _Context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT [dbo].[{0}] OFF", _TableName);
        }

        /// <summary>Переключить режим таблицы для изменения значений первичных ключей</summary>
        /// <typeparam name="T">Тип элементов данных набора</typeparam>
        /// <param name="Set">Набор данных контекста</param>
        /// <returns>Объект, обеспечивающий автоматизацию обратного переключения при вызове метода <see cref="IDisposable"/>.<see cref="IDisposable.Dispose()"/></returns>
        public static IDisposable IdentityInsert<T>(this DbSet<T> Set) where T : class => new DbSetIdentityInsert<T>(Set);

        #endregion

        /// <summary>Получить экземпляр контекста БД из объекта набора данных</summary>
        /// <typeparam name="T">Тип элементов данных набора</typeparam>
        /// <param name="Set">Набор данных контекста</param>
        /// <returns>Контекст БД, которому принадлежит набор данных</returns>
        public static DbContext GetContext<T>(this DbSet<T> Set)
            where T : class
        {
            var infrastructure = (IInfrastructure<IServiceProvider>)Set;
            var service_provider = infrastructure.Instance;
            var context_service = (ICurrentDbContext)service_provider.GetService(typeof(ICurrentDbContext));
            return context_service.Context;
        }

        /// <summary>Получить имя таблицы БД с которой связан набор данных</summary>
        /// <typeparam name="T">Тип элементов данных набора</typeparam>
        /// <param name="Set">Набор данных контекста</param>
        /// <returns>Имя таблицы в БД с которой связан набор данных</returns>
        public static string GetTableName<T>(this DbSet<T> Set)
            where T : class
        {
            var context = Set.GetContext();
            var model = context.Model;
            var entity_types = model.GetEntityTypes();
            var entity_type = entity_types.First(type => type.ClrType == typeof(T));
            var table_name_annotation = entity_type.GetAnnotation("Relational:TableName");
            return table_name_annotation.Value.ToString();
        }
    }
}