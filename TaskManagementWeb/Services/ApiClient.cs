using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace TaskManagementWeb.Services
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime? _js;

        private string? _token;

        private static readonly JsonSerializerOptions JsonOptions =
            new() { PropertyNameCaseInsensitive = true };

        public ApiClient(HttpClient httpClient, IJSRuntime? js = null)
        {
            _httpClient = httpClient;
            _js = js;
        }

        public async Task InitializeAsync()
        {
            if (_js == null)
                return;

            _token = await _js.InvokeAsync<string?>("localStorage.getItem", "jwt");

            if (!string.IsNullOrEmpty(_token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _token);
            }
        }

        public string? GetToken() => _token;

        private void EnsureAuthHeader()
        {
            if (!string.IsNullOrEmpty(_token))
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            else
                _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        public async Task SetToken(string? token)
        {
            _token = token;

            if (string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;

                if (_js != null)
                    await _js.InvokeVoidAsync("localStorage.removeItem", "jwt");

                return;
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            if (_js != null)
                await _js.InvokeVoidAsync("localStorage.setItem", "jwt", token);
        }

        // GET
        public async Task<T?> GetAsync<T>(string url)
        {
            EnsureAuthHeader();
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return default;
            return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        }

        // POST (generic)
        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest body)
        {
            EnsureAuthHeader();

            var response = await _httpClient.PostAsJsonAsync(url, body);
            var json = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(json))
                return default;

            try
            {
                return JsonSerializer.Deserialize<TResponse>(json, JsonOptions);
            }
            catch
            {
                return default;
            }
        }

        // POST (raw)
        public Task<HttpResponseMessage> PostAsync(string url, object body)
        {
            EnsureAuthHeader();
            return _httpClient.PostAsJsonAsync(url, body);
        }

        // PUT (bool)
        public async Task<bool> PutAsync<TRequest>(string url, TRequest body)
        {
            EnsureAuthHeader();
            var response = await _httpClient.PutAsJsonAsync(url, body);
            return response.IsSuccessStatusCode;
        }

        // PUT (raw)
        public Task<HttpResponseMessage> PutAsync(string url, object body)
        {
            EnsureAuthHeader();
            return _httpClient.PutAsJsonAsync(url, body);
        }

        // PUT (typed)
        public async Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest body)
        {
            EnsureAuthHeader();

            var response = await _httpClient.PutAsJsonAsync(url, body);

            if (!response.IsSuccessStatusCode)
                return default;

            return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions);
        }

        // DELETE
        public async Task<bool> DeleteAsync(string url)
        {
            EnsureAuthHeader();
            var response = await _httpClient.DeleteAsync(url);
            return response.IsSuccessStatusCode;
        }

        public async Task<T?> DeleteWithResponseAsync<T>(string url)
        {
            EnsureAuthHeader();
            var response = await _httpClient.DeleteAsync(url);
            if (!response.IsSuccessStatusCode) return default;
            return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        }

        // PATCH
        public async Task<bool> PatchAsync<TRequest>(string url, TRequest body)
        {
            EnsureAuthHeader();

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Patch, url)
            {
                Content = content
            };

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
    }
}