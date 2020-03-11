using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MathCore.EF7.Extensions
{
    public static class DbContextExtensions
    {
        /// <summary>Метод выполняет проверку наличия локальных незафиксированных в БД изменений</summary>
        /// <param name="context">Проверяемый контекст БД</param>
        /// <exception cref="InvalidOperationException">при наличии незафиксированных в БД изменений, хранимых в контексте</exception>
        public static DbContext ThrowIfHasChanges(this DbContext context)
        {
            if (context.ChangeTracker.HasChanges())
                throw new InvalidOperationException("В контексте БД есть незафиксированные изменения");
            return context;
        }

        public static IEnumerable<string> GetTableNames(this DbContext context)
        {
            //var names = context.Database.ExecuteSqlRaw()
            return Array.Empty<string>();
        }
    }
}
