using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace MathCore.EF7.Extensions
{
    /// <summary>Класс методов-расширений для интерфейса <see cref="IQueryable{T}"/></summary>
    public static class QueryableExtensions
    {
        private static object PrivateFieldValue(this object obj, string FieldName) => obj?.GetType().GetField(FieldName, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(obj);

        private static T PrivateFieldValue<T>(this object obj, string FieldName) => (T)obj?.GetType().GetField(FieldName, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(obj);

        /// <summary>Получить текст SQL-запроса</summary>
        /// <typeparam name="T">Тип элементов результата запроса</typeparam>
        /// <param name="query">Объект запроса, из которого требуется получить текст SQL</param>
        /// <returns>Текст SQL, формируемый на основе запроса</returns>
        /// <remarks>
        /// Решение взято отсюда: stackoverflow.com/a/51583047 <see url="https://stackoverflow.com/a/51583047"/>
        /// </remarks>
        public static string ToSql<T>(this IQueryable<T> query) where T : class
        {
            using var enumerator = query.Provider.Execute<IEnumerable<T>>(query.Expression).GetEnumerator();
            var relational_command_cache = enumerator.PrivateFieldValue("_relationalCommandCache");
            var select_expression = relational_command_cache.PrivateFieldValue<SelectExpression>("_selectExpression");
            var factory = relational_command_cache.PrivateFieldValue<IQuerySqlGeneratorFactory>("_querySqlGeneratorFactory");

            var sql_generator = factory.Create();
            var command = sql_generator.GetCommand(select_expression);

            var sql = command.CommandText;
            return sql;
        }
    }
}
