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
        private readonly IJSRuntime _js;

        // The JWT currently in memory (may be null if logged out).
        private string? _token;

        // JSON settings used across all calls.
        private static readonly JsonSerializerOptions JsonOptions =
            new() { PropertyNameCaseInsensitive = true };

        public ApiClient(HttpClient httpClient, IJSRuntime js)
        {
            _httpClient = httpClient;
            _js = js;
        }

        // Loads the JWT from localStorage when the app starts.
        // If found, it attaches the token to HttpClient's headers.
        public async Task InitializeAsync()
        {
            _token = await _js.InvokeAsync<string?>("localStorage.getItem", "jwt");

            if (!string.IsNullOrEmpty(_token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _token);
            }
        }

        // Gets the current JWT token (or null if logged out).
        public string? GetToken() => _token;

        // Makes sure the Authorization header is set before requests.
        private void EnsureAuthHeader()
        {
            if (!string.IsNullOrEmpty(_token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _token);
            }
        }

        // Sets or clears the JWT token both in memory and in localStorage.
        public async Task SetToken(string? token)
        {
            _token = token;

            if (string.IsNullOrEmpty(token))
            {
                // User is logged out.
                _httpClient.DefaultRequestHeaders.Authorization = null;
                await _js.InvokeVoidAsync("localStorage.removeItem", "jwt");
            }
            else
            {
                // User is logged in.
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                await _js.InvokeVoidAsync("localStorage.setItem", "jwt", token);
            }
        }

        // GET request with typed response body.
        public async Task<T?> GetAsync<T>(string url)
        {
            EnsureAuthHeader();
            return await _httpClient.GetFromJsonAsync<T>(url, JsonOptions);
        }

        // POST request with typed response body.
        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest body)
        {
            EnsureAuthHeader();
            var response = await _httpClient.PostAsJsonAsync(url, body);

            if (!response.IsSuccessStatusCode)
                return default;

            return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions);
        }

        // PUT request that only cares about success.
        public async Task<bool> PutAsync<TRequest>(string url, TRequest body)
        {
            EnsureAuthHeader();
            var response = await _httpClient.PutAsJsonAsync(url, body);
            return response.IsSuccessStatusCode;
        }

        // Basic DELETE request that only cares about success.
        public async Task<bool> DeleteAsync(string url)
        {
            EnsureAuthHeader();
            var response = await _httpClient.DeleteAsync(url);
            return response.IsSuccessStatusCode;
        }

        // DELETE erquest with typed response body.
        public async Task<T?> DeleteWithResponseAsync<T>(string url)
        {
            EnsureAuthHeader();
            var response = await _httpClient.DeleteAsync(url);

            if (!response.IsSuccessStatusCode)
                return default;

            return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        }

        // PATCH request that only cares about success.
        public async Task<bool> PatchAsync<TRequest>(string url, TRequest body)
        {
            EnsureAuthHeader();

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var req = new HttpRequestMessage(HttpMethod.Patch, url)
            {
                Content = content
            };

            var response = await _httpClient.SendAsync(req);
            return response.IsSuccessStatusCode;
        }
    }
}