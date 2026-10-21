using System.Text.Json;
using Microsoft.JSInterop;

namespace EventEase.Services;

/// <summary>Typed JSON wrapper around window.localStorage.</summary>
public class BrowserStorage(IJSRuntime js)
{
    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var json = await js.InvokeAsync<string?>("localStorage.getItem", key);
            return string.IsNullOrEmpty(json) ? default : JsonSerializer.Deserialize<T>(json);
        }
        catch (JsonException)
        {
            // Corrupted / outdated payload: discard it instead of crashing the app.
            await RemoveAsync(key);
            return default;
        }
        catch (JSException)
        {
            return default; // storage unavailable (e.g. privacy mode)
        }
    }

    public async Task SetAsync<T>(string key, T value)
    {
        try { await js.InvokeVoidAsync("localStorage.setItem", key, JsonSerializer.Serialize(value)); }
        catch (JSException) { /* storage full / unavailable – keep in-memory state */ }
    }

    public async Task RemoveAsync(string key)
    {
        try { await js.InvokeVoidAsync("localStorage.removeItem", key); }
        catch (JSException) { }
    }
}
