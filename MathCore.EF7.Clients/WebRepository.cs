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
using Microsoft.Extensions.Logging;

namespace MathCore.EF7.Clients
{

    /// <summary> Клиент к репозиторию </summary>
    /// <typeparam name="T">тип сущности</typeparam>
    /// <typeparam name="Tkey">тип идентификатора</typeparam>
    public class WebRepository<T, Tkey> : IRepository<T, Tkey> where T : IEntity<Tkey>
    {
        /// <summary> Клиент </summary>
        protected readonly HttpClient _Client;
        /// <summary> логгер </summary>
        protected readonly ILogger<WebRepository<T, Tkey>> _Logger;

        /// <summary> Конструктор </summary>
        /// <param name="client">клиент</param>
        /// <param name="logger">логгер</param>
        public WebRepository(HttpClient client, ILogger<WebRepository<T, Tkey>> logger)
        {
            _Client = client;
            _Logger = logger;
        }

        private const string BaseLogRow = "Запрос к репозиторию";

        private static string ToValueRow<Tvalue>(params Tvalue[] values) =>
            values is not null && values.Length > 0 ? string.Join(", ", values.Select(v => $"{nameof(v)}={v}")) : string.Empty;

        #region Implementation of IRepository<T,in Tkey>

        /// <inheritdoc />
        public async Task<bool> IsEmpty(CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(T)} {nameof(IsEmpty)}");

            var response = await _Client.GetAsync($"IsEmpty", Cancel).ConfigureAwait(false);
            return response.StatusCode != HttpStatusCode.NotFound && response.IsSuccessStatusCode;
        }

        /// <inheritdoc />
        public async Task<bool> ExistId(Tkey Id, CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(T)} {nameof(ExistId)} - {ToValueRow(Id)}");

            var response = await _Client.GetAsync($"exist/id/{Id}", Cancel).ConfigureAwait(false);
            return response.StatusCode != HttpStatusCode.NotFound && response.IsSuccessStatusCode;
        }

        /// <inheritdoc />
        public async Task<bool> Exist(T item, CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(T)} {nameof(Exist)} - {ToValueRow(item.Id)}");

            var response = await _Client.PostAsJsonAsync($"exist", item, Cancel).ConfigureAwait(false);
            return response.StatusCode != HttpStatusCode.NotFound && response.IsSuccessStatusCode;
        }

        /// <inheritdoc />
        public async Task<int> GetCount(CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(T)} {nameof(GetCount)}");
            return await _Client.GetFromJsonAsync<int>("count", Cancel).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<T>> GetAll(CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(T)} {nameof(GetAll)}");
            return await _Client.GetFromJsonAsync<IEnumerable<T>>("", Cancel).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<T>> Get(int Skip, int Count, CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(T)} {nameof(Get)} - {ToValueRow(Skip, Count)}");

            return await _Client.GetFromJsonAsync<IEnumerable<T>>($"items[{Skip}:{Count}]", Cancel).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<IPage<T>> GetPage(int PageNumber, int PageSize, CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(T)} {nameof(GetPage)} - {ToValueRow(PageNumber, PageSize)}");

            var response = await _Client.GetAsync($"page[{PageNumber}/{PageSize}]", Cancel).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return new PageItems
                {
                    Items = Enumerable.Empty<T>(),
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
        private class PageItems : IPage<T>
        {
            #region Implementation of IPage<out T>

            public IEnumerable<T> Items { get; init; }

            public int TotalCount { get; init; }

            public int PageNumber { get; init; }

            public int PageSize { get; init; }

            #endregion
        }

        /// <inheritdoc />
        public async Task<T> GetById(int Id, CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(T)} {nameof(GetById)} - {ToValueRow(Id)}");

            return await _Client.GetFromJsonAsync<T>($"{Id}", Cancel).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<T> Add(T item, CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(T)} {nameof(Add)}");

            var response = await _Client.PostAsJsonAsync("", item, Cancel).ConfigureAwait(false);
            var result = await response
               .EnsureSuccessStatusCode()
               .Content
               .ReadFromJsonAsync<T>(cancellationToken: Cancel)
               .ConfigureAwait(false);
            return result;
        }

        /// <inheritdoc />
        public async Task AddRange(IEnumerable<T> items, CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(T)} {nameof(AddRange)}");
            await _Client.PostAsJsonAsync("range", items, Cancel).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<T> Update(T item, CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(T)} {nameof(Update)} - {item.Id}");

            var response = await _Client.PutAsJsonAsync("", item, Cancel).ConfigureAwait(false);
            var result = await response
               .EnsureSuccessStatusCode()
               .Content
               .ReadFromJsonAsync<T>(cancellationToken: Cancel)
               .ConfigureAwait(false);
            return result;

        }

        /// <inheritdoc />
        public async Task<T> UpdateById(Tkey id, Action<T> ItemUpdated, CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(T)} {nameof(UpdateById)} - {ToValueRow(id)}");
            var response = await _Client.PutAsJsonAsync($"id/{id}", ItemUpdated, Cancel).ConfigureAwait(false);

            var result = await response
               .EnsureSuccessStatusCode()
               .Content
               .ReadFromJsonAsync<T>(cancellationToken: Cancel)
               .ConfigureAwait(false);
            return result;
        }

        /// <inheritdoc />
        public async Task UpdateRange(IEnumerable<T> items, CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(T)} {nameof(UpdateRange)}");

            await _Client.PutAsJsonAsync($"range", items, Cancel).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<T> Delete(T item, CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(T)} {nameof(Delete)} - {ToValueRow(item.Id)}");

            var request = new HttpRequestMessage(HttpMethod.Delete, "")
            {
                Content = JsonContent.Create(item)
            };

            var response = await _Client.SendAsync(request, Cancel).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return default;

            var result = await response
               .EnsureSuccessStatusCode()
               .Content
               .ReadFromJsonAsync<T>(cancellationToken: Cancel)
               .ConfigureAwait(false);
            return result;
        }

        /// <inheritdoc />
        public async Task DeleteRange(IEnumerable<T> items, CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(T)} {nameof(DeleteRange)}");

            var request = new HttpRequestMessage(HttpMethod.Delete, "range")
            {
                Content = JsonContent.Create(items)
            };

            await _Client.SendAsync(request, Cancel).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<T> DeleteById(Tkey id, CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(T)} {nameof(DeleteById)} - {ToValueRow(id)}");

            var response = await _Client.DeleteAsync($"{id}", Cancel).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return default;

            var result = await response
               .EnsureSuccessStatusCode()
               .Content
               .ReadFromJsonAsync<T>(cancellationToken: Cancel)
               .ConfigureAwait(false);
            return result;

        }

        /// <inheritdoc />
        public async Task<int> SaveChanges(CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(T)} {nameof(SaveChanges)}");

            return await _Client.GetFromJsonAsync<int>("save", Cancel).ConfigureAwait(false);
        }

        #endregion
    }

    /// <inheritdoc />
    public class WebRepository<T> : WebRepository<T, int> where T : IEntity<int>
    {
        /// <inheritdoc />
        public WebRepository(HttpClient client, ILogger<WebRepository<T, int>> logger) : base(client, logger)
        {
        }
    }
}
