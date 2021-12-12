using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        public WebRepository(IConfiguration configuration, ILogger<WebRepository<TEntity, TKey>> logger) : base(configuration, $"api/{typeof(TEntity).Name}")
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


        private static readonly string BaseLogRow = $"Запрос к репозиторию { typeof(TEntity).Name}";

        private static string ToValueRow<Tvalue>(params Tvalue[] values) =>
            values is not null && values.Length > 0 ? string.Join(", ", values.Select(v => $"{nameof(v)}={v}")) : string.Empty;

        #region Implementation of IRepository<T,in Tkey>

        /// <inheritdoc />
        public virtual async Task<bool> IsEmpty(CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(IsEmpty)}");

            var response = await GetAsync<bool>($"{ServiceAddress}/isempty", Cancel).ConfigureAwait(false);
            //var response = await _Client.GetAsync($"{ServiceAddress}/isempty", Cancel).ConfigureAwait(false);
            return response/*.StatusCode != HttpStatusCode.NotFound && response.IsSuccessStatusCode*/;
        }

        /// <inheritdoc />
        public virtual async Task<bool> ExistId(TKey Id, CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(ExistId)} - {ToValueRow(Id)}");

            var response = await GetAsync<bool>($"{ServiceAddress}/exist/id/{Id}", Cancel).ConfigureAwait(false);
            //var response = await _Client.GetAsync($"{ServiceAddress}/exist/id/{Id}", Cancel).ConfigureAwait(false);
            return response/*.StatusCode != HttpStatusCode.NotFound && response.IsSuccessStatusCode*/;
        }

        /// <inheritdoc />
        public virtual async Task<bool> Exist(TEntity item, CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(Exist)} - {ToValueRow(item.Id)}");

            var response = await PostAsync($"{ServiceAddress}/exist", item, Cancel).ConfigureAwait(false);
            //var response = await _Client.PostAsJsonAsync($"{ServiceAddress}/exist", item, Cancel).ConfigureAwait(false);
            return response.StatusCode != HttpStatusCode.NotFound && response.IsSuccessStatusCode;
        }

        /// <inheritdoc />
        public virtual async Task<int> GetCount(CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(GetCount)}");
            return await GetAsync<int>($"{ServiceAddress}/count", Cancel).ConfigureAwait(false);
            //return await _Client.GetFromJsonAsync<int>($"{ServiceAddress}/count", Cancel).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TEntity>> GetAll(CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(GetAll)}");
            return await GetAsync<IEnumerable<TEntity>>($"{ServiceAddress}", Cancel).ConfigureAwait(false);
            //return await _Client.GetFromJsonAsync<IEnumerable<TEntity>>($"{ServiceAddress}", Cancel).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TEntity>> Get(int Skip, int Count, CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(Get)} - {ToValueRow(Skip, Count)}");

            return await GetAsync<IEnumerable<TEntity>>($"{ServiceAddress}/items[{Skip}/{Count}]", Cancel).ConfigureAwait(false);
            //return await _Client.GetFromJsonAsync<IEnumerable<TEntity>>($"{ServiceAddress}/items[{Skip}:{Count}]", Cancel).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public virtual async Task<IPage<TEntity>> GetPage(int PageNumber, int PageSize, CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(GetPage)} - {ToValueRow(PageNumber, PageSize)}");

            var response = await GetAsync<PageItems>($"{ServiceAddress}/page[{PageNumber}/{PageSize}]", Cancel).ConfigureAwait(false);
            //var response = await _Client.GetAsync($"{ServiceAddress}/page[{PageNumber}/{PageSize}]", Cancel).ConfigureAwait(false);

            if (response is null)
                return new PageItems(Enumerable.Empty<TEntity>(), PageNumber, PageSize, 0); 

            return response;
        }

        private record PageItems(IEnumerable<TEntity> Items, int TotalCount, int PageNumber, int PageSize) : IPage<TEntity>;

        /// <inheritdoc />
        public virtual async Task<TEntity> GetById(TKey Id, CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(GetById)} - {ToValueRow(Id)}");

            return await GetAsync<TEntity>($"{ServiceAddress}/{Id}", Cancel).ConfigureAwait(false);
            //return await _Client.GetFromJsonAsync<TEntity>($"{ServiceAddress}/{Id}", Cancel).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> Add(TEntity item, CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(Add)}");

            var response = await PostAsync($"{ServiceAddress}", item, Cancel).ConfigureAwait(false);
            //var response = await _Client.PostAsJsonAsync($"{ServiceAddress}", item, Cancel).ConfigureAwait(false);
            var result = await response
               .EnsureSuccessStatusCode()
               .Content
               .ReadFromJsonAsync<TEntity>(cancellationToken: Cancel)
               .ConfigureAwait(false);
            return result;
        }

        /// <inheritdoc />
        public virtual async Task AddRange(IEnumerable<TEntity> items, CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(AddRange)}");
            await PostAsync($"{ServiceAddress}/range", items, Cancel).ConfigureAwait(false);
            //await _Client.PostAsJsonAsync($"{ServiceAddress}/range", items, Cancel).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> Update(TEntity item, CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(Update)} - {item.Id}");

            var response = await PutAsync($"{ServiceAddress}", item, Cancel).ConfigureAwait(false);

            //var response = await _Client.PutAsJsonAsync($"{ServiceAddress}", item, Cancel).ConfigureAwait(false);

            var result = await response
               .EnsureSuccessStatusCode()
               .Content
               .ReadFromJsonAsync<TEntity>(cancellationToken: Cancel)
               .ConfigureAwait(false);
            return result;

        }

        /// <inheritdoc />
        public virtual async Task UpdateRange(IEnumerable<TEntity> items, CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(UpdateRange)}");

            await PutAsync($"{ServiceAddress}/range", items, Cancel).ConfigureAwait(false);
            //await _Client.PutAsJsonAsync($"{ServiceAddress}/range", items, Cancel).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> Delete(TEntity item, CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(Delete)} - {ToValueRow(item.Id)}");

            var response = await DeleteAsync(item, Cancel).ConfigureAwait(false);
            //var request = new HttpRequestMessage(HttpMethod.Delete, $"{ServiceAddress}")
            //{
            //    Content = JsonContent.Create(item)
            //};

            //var response = await _Client.SendAsync(request, Cancel).ConfigureAwait(false);
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
        public virtual async Task DeleteRange(IEnumerable<TEntity> items, CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(DeleteRange)}");

            await DeleteAsync(items, Cancel).ConfigureAwait(false);

            //var request = new HttpRequestMessage(HttpMethod.Delete, $"{ServiceAddress}/range")
            //{
            //    Content = JsonContent.Create(items)
            //};

            //await _Client.SendAsync(request, Cancel).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> DeleteById(TKey id, CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(DeleteById)} - {ToValueRow(id)}");

            var response = await DeleteAsync($"{ServiceAddress}/{id}", Cancel).ConfigureAwait(false);
            //var response = await _Client.DeleteAsync($"{ServiceAddress}/{id}", Cancel).ConfigureAwait(false);
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
        public virtual async Task<int> SaveChanges(CancellationToken Cancel = default)
        {
            _Logger.Log(LogLevel.Debug, $"{BaseLogRow} {nameof(SaveChanges)}");

            return await GetAsync<int>($"{ServiceAddress}/save", Cancel).ConfigureAwait(false);
            //return await _Client.GetFromJsonAsync<int>($"{ServiceAddress}/save", Cancel).ConfigureAwait(false);
        }

        #endregion
    }

    /// <inheritdoc cref="WebRepository{TEntity, TKey}" />
    public class WebRepository<TEntity> : WebRepository<TEntity, int>, IRepository<TEntity> where TEntity : IEntity<int>
    {
        /// <inheritdoc />
        public WebRepository(IConfiguration configuration, ILogger<WebRepository<TEntity>> logger) : base(configuration, logger)
        {
        }

        /// <inheritdoc />
        public WebRepository(IConfiguration configuration, ILogger<WebRepository<TEntity>> logger, string serviceAddress) : base(configuration, logger, serviceAddress)
        {
        }
    }
}
