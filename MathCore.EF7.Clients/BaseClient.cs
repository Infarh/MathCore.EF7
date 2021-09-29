using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
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
        /// <typeparam name="T">Тип нужных данных</typeparam>
        /// <param name="url">адрес</param>
        /// <param name="JsonOptions">Настройки сериализации</param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns></returns>
        protected async Task<T> GetAsync<T>(string url, JsonSerializerOptions JsonOptions, CancellationToken Cancel) 
        {
            var response = await _Client.GetAsync(url, Cancel);
            if (!response.IsSuccessStatusCode) return default;
            return await response.Content.ReadFromJsonAsync<T>(JsonOptions, Cancel);
        }

        /// <summary> Post </summary>
        /// <typeparam name="T">Тип нужных данных</typeparam>
        /// <param name="url">адрес</param>
        /// <param name="item">данные</param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns></returns>
        protected async Task<HttpResponseMessage> PostAsync<T>(string url, T item, CancellationToken Cancel)
        {
            var response = await _Client.PostAsJsonAsync(url, item, Cancel);
            return response.EnsureSuccessStatusCode();
        }

        /// <summary> Put </summary>
        /// <typeparam name="T">Тип нужных данных</typeparam>
        /// <param name="url">адрес</param>
        /// <param name="item">данные</param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns></returns>
        protected async Task<HttpResponseMessage> PutAsync<T>(string url, T item, CancellationToken Cancel)
        {
            var response = await _Client.PutAsJsonAsync(url, item, Cancel);
            return response.EnsureSuccessStatusCode();
        }

        /// <summary> Delete </summary>
        /// <param name="url">адрес</param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        protected async Task<HttpResponseMessage> DeleteAsync(string url, CancellationToken Cancel) => await _Client.DeleteAsync(url, Cancel);

    }
}
