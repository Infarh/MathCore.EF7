using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MathCore.EF7.Interfaces.Entities;

namespace MathCore.EF7.Interfaces.Repositories
{
    /// <summary>Репозиторий сущностей с определенным географическим положением</summary>
    /// <typeparam name="T">Тип сущности с определенным географическим положением</typeparam>
    /// <typeparam name="TKey">Тип первичного ключа сущности</typeparam>
    public interface IGPSRepository<T, in TKey> : IRepository<T, TKey> where T : IGPSEntity<TKey>, IEntity<TKey>
    {
        /// <summary>Проверка существования сущности по заданным координатам в ограниченном радиусе поиска</summary>
        /// <param name="Latitude">Широта</param>
        /// <param name="Longitude">Долгота</param>
        /// <param name="RangeInMeters">Максимальное расстояние (радиус) поиска в метрах</param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns>Истина, если в заданном радиусе поиска есть сущность</returns>
        Task<bool> ExistInLocation(double Latitude, double Longitude, double RangeInMeters, CancellationToken Cancel = default);

        /// <summary>Получить число сущностей в заданном радиусе поиска</summary>
        /// <param name="Latitude">Широта</param>
        /// <param name="Longitude">Долгота</param>
        /// <param name="RangeInMeters">Максимальное расстояние (радиус) поиска в метрах</param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns>Число сущностей, попадающих в заданный радиус поиска</returns>
        Task<int> GetCountInLocation(double Latitude, double Longitude, double RangeInMeters, CancellationToken Cancel = default);

        /// <summary>Получить все сущности в указанном радиусе поиска</summary>
        /// <param name="Latitude">Широта</param>
        /// <param name="Longitude">Долгота</param>
        /// <param name="RangeInMeters">Максимальное расстояние (радиус) поиска в метрах</param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns>Все сущности, попадающие в указанный радиус поиска</returns>
        Task<IEnumerable<T>> GetAllByLocationInRange(double Latitude, double Longitude, double RangeInMeters, CancellationToken Cancel = default);

        /// <summary>Получить все сущности в указанном радиусе поиска</summary>
        /// <param name="Latitude">Широта</param>
        /// <param name="Longitude">Долгота</param>
        /// <param name="RangeInMeters">Максимальное расстояние (радиус) поиска в метрах</param>
        /// <param name="Skip">Число сущностей, пропускаемых в начале выборки</param>
        /// <param name="Take">Число сущностей, извлекаемых из выборки</param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns>Сущности, попадающие в указанный радиус поиска</returns>
        Task<IEnumerable<T>> GetAllByLocationInRange(double Latitude, double Longitude, double RangeInMeters, int Skip, int Take, CancellationToken Cancel = default);

        /// <summary>Получить ближайшую сущность для указанных координат</summary>
        /// <param name="Latitude">Широта</param>
        /// <param name="Longitude">Долгота</param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns>Ближайшая к указанным координатам сущность</returns>
        Task<T> GetByLocation(double Latitude, double Longitude, CancellationToken Cancel = default);

        /// <summary>Получить ближайшую сущность для указанных координат на заданном максимальном удалении</summary>
        /// <param name="Latitude">Широта</param>
        /// <param name="Longitude">Долгота</param>
        /// <param name="RangeInMeters">Максимальное расстояние (радиус) поиска в метрах</param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns>Ближайшая к указанным координатам сущность в случае ее наличия</returns>
        Task<T> GetByLocationInRange(double Latitude, double Longitude, double RangeInMeters, CancellationToken Cancel = default);

        /// <summary>Получить страницу с сущностями из репозитория в заданном радиусе поиска</summary>
        /// <param name="Latitude">Широта</param>
        /// <param name="Longitude">Долгота</param>
        /// <param name="RangeInMeters">Максимальное расстояние (радиус) поиска в метрах</param>
        /// <param name="PageNumber">Номер страницы начиная с нуля</param>
        /// <param name="PageSize">Размер страницы</param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns>Страница с сущностями из репозитория в заданном радиусе поиска</returns>
        Task<IPage<T>> GetPageByLocationInRange(double Latitude, double Longitude, double RangeInMeters, int PageNumber, int PageSize, CancellationToken Cancel = default);

        /// <summary>Удаление сущности, ближайшей к точке с указанными координатами</summary>
        /// <param name="Latitude">Широта</param>
        /// <param name="Longitude">Долгота</param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns>Удаленная из репозитория сущность, ближайшая к указанным координатам</returns>
        Task<T> DeleteByLocation(double Latitude, double Longitude, CancellationToken Cancel = default);

        /// <summary>Удаление сущности, ближайшей к точке с указанными координатами с ограничением радиуса поиска</summary>
        /// <param name="Latitude">Широта</param>
        /// <param name="Longitude">Долгота</param>
        /// <param name="RangeInMeters">Максимальное расстояние (радиус) поиска в метрах</param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns>Удаленная из репозитория сущность, ближайшая к указанным координатам в случае ее наличия</returns>
        Task<T> DeleteByLocationInRange(double Latitude, double Longitude, double RangeInMeters, CancellationToken Cancel = default);
    }

    /// <summary>Репозиторий сущностей с определенным географическим положением</summary>
    /// <typeparam name="T">Тип сущности с определенным географическим положением</typeparam>
    public interface IGPSRepository<T> : IGPSRepository<T, int> where T : IGPSEntity<int> { }
}
