#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using MathCore.EF7.Interfaces.Entities;

namespace MathCore.EF7.Extensions
{
    /// <summary>Методы-расширения для сущностей с геопозиционированием</summary>
    public static class GPSEntityExtensions
    {
        /// <summary>Упорядочить по удалению от указанной точки и выбрать лишь те элементы, что попадают в указанный радиус</summary>
        /// <typeparam name="TGpsEntity">Тип элементов</typeparam>
        /// <typeparam name="TKey">Тип ключа сущности</typeparam>
        /// <param name="query">Исходный запрос</param>
        /// <param name="Latitude">Широта опорной точки</param>
        /// <param name="Longitude">Долгота опорной точки</param>
        /// <param name="Range">Ограничивающий радиус в метрах</param>
        /// <returns>Запрос элементов вокруг указанной точки в заданном радиусе</returns>
        public static IQueryable<TGpsEntity> OrderByDistanceInRange<TGpsEntity, TKey>(
            this IQueryable<TGpsEntity> query,
            double Latitude,
            double Longitude,
            double Range)
        where TGpsEntity : IGPSEntity<TKey>
        {
            const double earth_radius = 6_378_137d;
            const double to_rad = Math.PI / 180;

            var latitude = Latitude * to_rad;
            var longitude = Longitude * to_rad;

            return
                from item in query
                let d_lat = item.Latitude * to_rad - latitude
                let d_lon = item.Longitude * to_rad - longitude
                let sin_d_lat05 = Math.Sin(d_lat / 2)
                let sin_d_lon05 = Math.Sin(d_lon / 2)
                let a = sin_d_lat05 * sin_d_lat05
                    + Math.Cos(item.Latitude * to_rad) * Math.Cos(latitude)
                    * sin_d_lon05 * sin_d_lon05
                let r = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a)) * earth_radius
                where r <= Range
                orderby r
                select item;
        }

        /// <summary>Отсортировать по увеличению дальности от указанной точки</summary>
        /// <typeparam name="T">Тип элемента, имеющего географические координаты</typeparam>
        /// <typeparam name="Tkey">Тип ключа сущности</typeparam>
        /// <param name="query">Запрос</param>
        /// <param name="Latitude">Широта указанной точки</param>
        /// <param name="Longitude">Долгота указанной точки</param>
        /// <returns>Запрос, содержащий последовательность элементов, упорядоченную по удалению от указанной точки</returns>
        public static IQueryable<T> OrderByDistance<T, Tkey>(this IQueryable<T> query, double Latitude, double Longitude)
            where T : IGPSEntity<Tkey> =>
            query.OrderBy(item => (item.Latitude - Latitude) * (item.Latitude - Latitude) + (item.Longitude - Longitude) * (item.Longitude - Longitude));

        /// <summary>Получить ближайший объект к указанной точке</summary>
        /// <typeparam name="T">Тип элемента, имеющего географические координаты</typeparam>
        /// <typeparam name="Tkey">Тип ключа сущности</typeparam>
        /// <param name="query">Запрос</param>
        /// <param name="Latitude">Широта указанной точки</param>
        /// <param name="Longitude">Долгота указанной точки</param>
        /// <returns>Первый ближайший элемент к указанной точке</returns>
        public static T Closest<T, Tkey>(this IQueryable<T> query, double Latitude, double Longitude)
            where T : IGPSEntity<Tkey> => query
               .OrderByDistance<T, Tkey>(Latitude, Longitude)
               .First();

        /// <summary>Получить ближайший объект к указанной точке</summary>
        /// <typeparam name="T">Тип элемента, имеющего географические координаты</typeparam>
        /// <typeparam name="Tkey">Тип ключа сущности</typeparam>
        /// <param name="query">Запрос</param>
        /// <param name="Latitude">Широта указанной точки</param>
        /// <param name="Longitude">Долгота указанной точки</param>
        /// <returns>Первый ближайший элемент к указанной точке</returns>
        public static T? ClosestOrDefault<T, Tkey>(this IQueryable<T> query, double Latitude, double Longitude)
            where T : IGPSEntity<Tkey> => query
           .OrderByDistance<T, Tkey>(Latitude, Longitude)
           .FirstOrDefault();

        /// <summary>Отсортировать по увеличению дальности от указанной точки</summary>
        /// <typeparam name="T">Тип элемента, имеющего географические координаты</typeparam>
        /// <typeparam name="Tkey">Тип ключа сущности</typeparam>
        /// <param name="items">Последовательность элементов</param>
        /// <param name="Latitude">Широта указанной точки</param>
        /// <param name="Longitude">Долгота указанной точки</param>
        /// <returns>Последовательность элементов, содержащий последовательность элементов, упорядоченную по удалению от указанной точки</returns>
        public static IEnumerable<T> OrderByDistance<T, Tkey>(this IEnumerable<T> items, double Latitude, double Longitude)
            where T : IGPSEntity<Tkey> =>
            items.OrderBy(item => (item.Latitude - Latitude) * (item.Latitude - Latitude) + (item.Longitude - Longitude) * (item.Longitude - Longitude));

        /// <summary>Получить ближайший объект к указанной точке</summary>
        /// <typeparam name="T">Тип элемента, имеющего географические координаты</typeparam>
        /// <typeparam name="Tkey">Тип ключа сущности</typeparam>
        /// <param name="items">Последовательность элементов</param>
        /// <param name="Latitude">Широта указанной точки</param>
        /// <param name="Longitude">Долгота указанной точки</param>
        /// <returns>Первый ближайший элемент к указанной точке</returns>
        public static T Closest<T,Tkey>(this IEnumerable<T> items, double Latitude, double Longitude)
            where T : IGPSEntity<Tkey> => items
               .OrderByDistance<T, Tkey>(Latitude, Longitude)
               .First();

        /// <summary>Получить ближайший объект к указанной точке</summary>
        /// <typeparam name="T">Тип элемента, имеющего географические координаты</typeparam>
        /// <typeparam name="Tkey">Тип ключа сущности</typeparam>
        /// <param name="items">Последовательность элементов</param>
        /// <param name="Latitude">Широта указанной точки</param>
        /// <param name="Longitude">Долгота указанной точки</param>
        /// <returns>Первый ближайший элемент к указанной точке</returns>
        public static T? ClosestOrDefault<T, Tkey>(this IEnumerable<T> items, double Latitude, double Longitude)
            where T : IGPSEntity<Tkey> => items
           .OrderByDistance<T, Tkey>(Latitude, Longitude)
           .FirstOrDefault();
    }
}
