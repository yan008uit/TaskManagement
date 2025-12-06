using Microsoft.JSInterop;
using System.Collections.Concurrent;

public class MockJsRuntime : IJSRuntime
{
    private readonly ConcurrentDictionary<string, object?> _storage = new();

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
    {
        return InvokeAsync<TValue>(identifier, CancellationToken.None, args);
    }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
        if (identifier == "localStorage.getItem" && args?.Length == 1)
        {
            var key = args[0]?.ToString() ?? "";
            _storage.TryGetValue(key, out var value);
            return new ValueTask<TValue>((TValue?)value!);
        }

        if (identifier == "localStorage.setItem" && args?.Length == 2)
        {
            var key = args[0]?.ToString() ?? "";
            var value = args[1];
            _storage[key] = value;
            return new ValueTask<TValue>(default(TValue)!);
        }

        if (identifier == "localStorage.removeItem" && args?.Length == 1)
        {
            var key = args[0]?.ToString() ?? "";
            _storage.TryRemove(key, out _);
            return new ValueTask<TValue>(default(TValue)!);
        }

        return new ValueTask<TValue>(default(TValue)!);
    }
}