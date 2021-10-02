using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace MathCore.EF7.Clients
{
    /// <summary>
    /// Базовый клиент с реализациями
    /// </summary>
    public abstract class BaseClient
    {
        /// <summary> Http клиент </summary>
        protected readonly HttpClient _Client;
        /// <summary> адрес сервиса </summary>
        protected string ServiceAddress { get; }

        /// <summary>
        /// Базовый конструктор
        /// </summary>
        /// <param name="configuration">конфигурация</param>
        /// <param name="serviceAddress">адрес сервиса</param>
        protected BaseClient(IConfiguration configuration, string serviceAddress)
        {
            ServiceAddress = serviceAddress;
            _Client = new HttpClient
            {
                BaseAddress = new Uri(configuration["ClientAddress"])
            };

            _Client.DefaultRequestHeaders.Accept.Clear();
            _Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary> Get </summary>
        /// <typeparam name="TEntity">Тип нужных данных</typeparam>
        /// <param name="url">адрес</param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns></returns>
        protected async Task<TEntity> GetAsync<TEntity>(string url, CancellationToken Cancel = default)
        {
            var response = await _Client.GetAsync(url, Cancel);
            if (response.StatusCode != HttpStatusCode.NotFound && response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<TEntity>(cancellationToken: Cancel);
            return default;
        }

        /// <summary> Post </summary>
        /// <typeparam name="TItem">Тип нужных данных</typeparam>
        /// <param name="url">адрес</param>
        /// <param name="item">данные</param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns></returns>
        protected async Task<HttpResponseMessage> PostAsync<TItem>(string url, TItem item, CancellationToken Cancel = default)
        {
            var response = await _Client.PostAsJsonAsync(url, item, Cancel);
            return response.EnsureSuccessStatusCode();
        }

        /// <summary> Put </summary>
        /// <typeparam name="TItem">Тип нужных данных</typeparam>
        /// <param name="url">адрес</param>
        /// <param name="item">данные</param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns></returns>
        protected async Task<HttpResponseMessage> PutAsync<TItem>(string url, TItem item, CancellationToken Cancel = default)
        {
            var response = await _Client.PutAsJsonAsync(url, item, Cancel);
            return response.EnsureSuccessStatusCode();
        }

        /// <summary> Удаление </summary>
        /// <param name="url">адрес</param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        protected async Task<HttpResponseMessage> DeleteAsync(string url, CancellationToken Cancel = default) => await _Client.DeleteAsync(url, Cancel);

        /// <summary> Удаление элемента (ов) </summary>
        /// <param name="url"> адрес</param>
        /// <param name="item"> данные </param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        protected async Task<HttpResponseMessage> DeleteAsync<TContent>(string url, TContent item, CancellationToken Cancel = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{ServiceAddress}")
            {
                Content = JsonContent.Create(item)
            };

            var response = await _Client.SendAsync(request, Cancel).ConfigureAwait(false);
            return response;
        }
    }
}
