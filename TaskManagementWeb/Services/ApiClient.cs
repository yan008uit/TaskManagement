using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace TaskManagementWeb.Services
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _js;

        // Cached JWT token (synced with localStorage)
        private string? _token;

        // JSON options
        private static readonly JsonSerializerOptions _jsonOptions =
            new() { PropertyNameCaseInsensitive = true };

        public HttpClient HttpClient => _httpClient;

        public ApiClient(HttpClient httpClient, IJSRuntime js)
        {
            _httpClient = httpClient;
            _js = js;
        }

        // Ensures Authorization header is set before requests
        private void EnsureAuthHeader()
        {
            if (!string.IsNullOrEmpty(_token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _token);
            }
        }

        public string? GetToken() => _token;

        // Loads token from localStorage on startup
        public async Task InitializeAsync()
        {

            _token = await _js.InvokeAsync<string?>("localStorage.getItem", "jwt");

            if (!string.IsNullOrEmpty(_token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _token);
            }
        }

        // Saves, updates, or removes token safely
        public async Task SetToken(string? token)
        {
            _token = token;

            // Still update HttpClient header even without JS
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }

            if (token == null)
                await _js.InvokeVoidAsync("localStorage.removeItem", "jwt");
            else
                await _js.InvokeVoidAsync("localStorage.setItem", "jwt", token);
        }

        // GET
        public async Task<TResponse> GetAsync<TResponse>(string path)
        {
            EnsureAuthHeader();

            var response = await _httpClient.GetAsync(path);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Status {response.StatusCode}: {content}");

            return JsonSerializer.Deserialize<TResponse>(content, _jsonOptions)
                   ?? throw new Exception("Failed to deserialize API response.");
        }

        // POST
        public async Task<TResponse> PostAsync<TRequest, TResponse>(string path, TRequest body)
        {
            EnsureAuthHeader();

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(path, content);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Status {response.StatusCode}: {responseText}");

            return JsonSerializer.Deserialize<TResponse>(responseText, _jsonOptions)
                   ?? throw new Exception("Failed to deserialize API response.");
        }

        // PUT with body
        public async Task<bool> PutAsync<TRequest>(string path, TRequest body)
        {
            EnsureAuthHeader();

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(path, content);
            return response.IsSuccessStatusCode;
        }

        // PUT without body
        public async Task<bool> PutAsync(string path)
        {
            EnsureAuthHeader();

            var response = await _httpClient.PutAsync(path, null);
            return response.IsSuccessStatusCode;
        }

        // DELETE
        public async Task<bool> DeleteAsync(string path)
        {
            EnsureAuthHeader();

            var response = await _httpClient.DeleteAsync(path);
            return response.IsSuccessStatusCode;
        }

        // PATCH
        public async Task<bool> PatchAsync<TRequest>(string path, TRequest body)
        {
            EnsureAuthHeader();

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Patch, path)
            {
                Content = content
            };

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
    }
}