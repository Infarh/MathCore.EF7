using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using MathCore.EF7.Interfaces.Entities;
using MathCore.EF7.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MathCore.EF7.Clients
{

    /// <summary> Клиент к репозиторию </summary>
    /// <typeparam name="TEntity">тип сущности</typeparam>
    /// <typeparam name="TKey">тип идентификатора</typeparam>
    public class WebRepository<TEntity, TKey> : BaseClient, IRepository<TEntity, TKey> where TEntity : IEntity<TKey>
    {
        /// <summary> логгер </summary>
        protected readonly ILogger<WebRepository<TEntity, TKey>> _Logger;

        /// <summary> Конструктор - адрес клиента api/TEntity</summary>
        /// <param name="configuration">Конфигурация</param>
        /// <param name="logger">логгер</param>
        public WebRepository(IConfiguration configuration, ILogger<WebRepository<TEntity, TKey>> logger) : base(configuration, $"api/{nameof(TEntity)}")
        {
            _Logger = logger;
        }

        /// <summary> Конструктор </summary>
        /// <param name="configuration">Конфигурация</param>
        /// <param name="logger">логгер</param>
        /// <param name="serviceAddress">адрес сервиса</param>
        public WebRepository(IConfiguration configuration, ILogger<WebRepository<TEntity, TKey>> logger, string serviceAddress) : base(configuration, serviceAddress)
        {
            _Logger = logger;
        }


        private const string BaseLogRow = "Запрос к репозиторию";

        private static string ToValueRow<Tvalue>(params Tvalue[] values) =>
            values is not null && values.Length > 0 ? string.Join(", ", values.Select(v => $"{nameof(v)}={v}")) : string.Empty;

        #region Implementation of IRepository<T,in Tkey>

        /// <inheritdoc />
        public async Task<bool> IsEmpty(CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(TEntity)} {nameof(IsEmpty)}");

            var response = await _Client.GetAsync($"{ServiceAddress}/isempty", Cancel).ConfigureAwait(false);
            return response.StatusCode != HttpStatusCode.NotFound && response.IsSuccessStatusCode;
        }

        /// <inheritdoc />
        public async Task<bool> ExistId(TKey Id, CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(TEntity)} {nameof(ExistId)} - {ToValueRow(Id)}");

            var response = await _Client.GetAsync($"{ServiceAddress}/exist/id/{Id}", Cancel).ConfigureAwait(false);
            return response.StatusCode != HttpStatusCode.NotFound && response.IsSuccessStatusCode;
        }

        /// <inheritdoc />
        public async Task<bool> Exist(TEntity item, CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(TEntity)} {nameof(Exist)} - {ToValueRow(item.Id)}");

            var response = await _Client.PostAsJsonAsync($"{ServiceAddress}/exist", item, Cancel).ConfigureAwait(false);
            return response.StatusCode != HttpStatusCode.NotFound && response.IsSuccessStatusCode;
        }

        /// <inheritdoc />
        public async Task<int> GetCount(CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(TEntity)} {nameof(GetCount)}");
            return await _Client.GetFromJsonAsync<int>($"{ServiceAddress}/count", Cancel).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TEntity>> GetAll(CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(TEntity)} {nameof(GetAll)}");
            return await _Client.GetFromJsonAsync<IEnumerable<TEntity>>($"{ServiceAddress}", Cancel).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TEntity>> Get(int Skip, int Count, CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(TEntity)} {nameof(Get)} - {ToValueRow(Skip, Count)}");

            return await _Client.GetFromJsonAsync<IEnumerable<TEntity>>($"{ServiceAddress}/items[{Skip}:{Count}]", Cancel).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<IPage<TEntity>> GetPage(int PageNumber, int PageSize, CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(TEntity)} {nameof(GetPage)} - {ToValueRow(PageNumber, PageSize)}");

            var response = await _Client.GetAsync($"{ServiceAddress}/page[{PageNumber}/{PageSize}]", Cancel).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return new PageItems
                {
                    Items = Enumerable.Empty<TEntity>(),
                    PageNumber = PageNumber,
                    PageSize = PageSize,
                    TotalCount = 0
                };

            return await response
               .EnsureSuccessStatusCode()
               .Content
               .ReadFromJsonAsync<PageItems>(cancellationToken: Cancel)
               .ConfigureAwait(false);
        }
        private class PageItems : IPage<TEntity>
        {
            #region Implementation of IPage<out T>

            public IEnumerable<TEntity> Items { get; init; }

            public int TotalCount { get; init; }

            public int PageNumber { get; init; }

            public int PageSize { get; init; }

            #endregion
        }

        /// <inheritdoc />
        public async Task<TEntity> GetById(TKey Id, CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(TEntity)} {nameof(GetById)} - {ToValueRow(Id)}");

            return await _Client.GetFromJsonAsync<TEntity>($"{ServiceAddress}/{Id}", Cancel).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<TEntity> Add(TEntity item, CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(TEntity)} {nameof(Add)}");

            var response = await _Client.PostAsJsonAsync($"{ServiceAddress}", item, Cancel).ConfigureAwait(false);
            var result = await response
               .EnsureSuccessStatusCode()
               .Content
               .ReadFromJsonAsync<TEntity>(cancellationToken: Cancel)
               .ConfigureAwait(false);
            return result;
        }

        /// <inheritdoc />
        public async Task AddRange(IEnumerable<TEntity> items, CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(TEntity)} {nameof(AddRange)}");
            await _Client.PostAsJsonAsync($"{ServiceAddress}/range", items, Cancel).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<TEntity> Update(TEntity item, CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(TEntity)} {nameof(Update)} - {item.Id}");

            var response = await _Client.PutAsJsonAsync($"{ServiceAddress}", item, Cancel).ConfigureAwait(false);
            var result = await response
               .EnsureSuccessStatusCode()
               .Content
               .ReadFromJsonAsync<TEntity>(cancellationToken: Cancel)
               .ConfigureAwait(false);
            return result;

        }

        /// <inheritdoc />
        public async Task<TEntity> UpdateById(TKey id, Action<TEntity> ItemUpdated, CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(TEntity)} {nameof(UpdateById)} - {ToValueRow(id)}");
            var response = await _Client.PutAsJsonAsync($"{ServiceAddress}/id/{id}", ItemUpdated, Cancel).ConfigureAwait(false);

            var result = await response
               .EnsureSuccessStatusCode()
               .Content
               .ReadFromJsonAsync<TEntity>(cancellationToken: Cancel)
               .ConfigureAwait(false);
            return result;
        }

        /// <inheritdoc />
        public async Task UpdateRange(IEnumerable<TEntity> items, CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(TEntity)} {nameof(UpdateRange)}");

            await _Client.PutAsJsonAsync($"{ServiceAddress}/range", items, Cancel).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<TEntity> Delete(TEntity item, CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(TEntity)} {nameof(Delete)} - {ToValueRow(item.Id)}");

            var request = new HttpRequestMessage(HttpMethod.Delete, $"{ServiceAddress}")
            {
                Content = JsonContent.Create(item)
            };

            var response = await _Client.SendAsync(request, Cancel).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return default;

            var result = await response
               .EnsureSuccessStatusCode()
               .Content
               .ReadFromJsonAsync<TEntity>(cancellationToken: Cancel)
               .ConfigureAwait(false);
            return result;
        }

        /// <inheritdoc />
        public async Task DeleteRange(IEnumerable<TEntity> items, CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(TEntity)} {nameof(DeleteRange)}");

            var request = new HttpRequestMessage(HttpMethod.Delete, $"{ServiceAddress}/range")
            {
                Content = JsonContent.Create(items)
            };

            await _Client.SendAsync(request, Cancel).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<TEntity> DeleteById(TKey id, CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(TEntity)} {nameof(DeleteById)} - {ToValueRow(id)}");

            var response = await _Client.DeleteAsync($"{ServiceAddress}/{id}", Cancel).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return default;

            var result = await response
               .EnsureSuccessStatusCode()
               .Content
               .ReadFromJsonAsync<TEntity>(cancellationToken: Cancel)
               .ConfigureAwait(false);
            return result;

        }

        /// <inheritdoc />
        public async Task<int> SaveChanges(CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(TEntity)} {nameof(SaveChanges)}");

            return await _Client.GetFromJsonAsync<int>($"{ServiceAddress}/save", Cancel).ConfigureAwait(false);
        }

        #endregion
    }

    /// <inheritdoc />
    public class WebRepository<T> : WebRepository<T, int> where T : IEntity<int>
    {
        /// <inheritdoc />
        public WebRepository(IConfiguration configuration, ILogger<WebRepository<T, int>> logger) : base(configuration, logger)
        {
        }

        /// <inheritdoc />
        public WebRepository(IConfiguration configuration, ILogger<WebRepository<T, int>> logger, string serviceAddress) : base(configuration, logger, serviceAddress)
        {
        }
    }
}
