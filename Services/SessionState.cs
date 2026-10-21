using EventEase.Models;

namespace EventEase.Services;

/// <summary>
/// Application-wide user session state. Persists to localStorage with a sliding
/// 8-hour lifetime and notifies subscribers (layout, pages) on change.
/// </summary>
public class SessionState(BrowserStorage storage)
{
    private const string StorageKey = "eventease.session.v1";
    private static readonly TimeSpan Lifetime = TimeSpan.FromHours(8);

    public UserSession? Current { get; private set; }
    public bool IsAuthenticated => Current is not null && Current.ExpiresAt > DateTime.UtcNow;

    public event Action? OnChange;

    public async Task InitializeAsync()
    {
        var stored = await storage.GetAsync<UserSession>(StorageKey);
        if (stored is not null && stored.ExpiresAt > DateTime.UtcNow)
        {
            Current = stored;
            await TouchAsync(); // sliding expiration
        }
        else if (stored is not null)
        {
            await storage.RemoveAsync(StorageKey);
        }
    }

    public async Task SignInAsync(string name, string email)
    {
        var now = DateTime.UtcNow;
        Current = new UserSession
        {
            Name = name.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            StartedAt = now,
            ExpiresAt = now.Add(Lifetime)
        };
        await storage.SetAsync(StorageKey, Current);
        OnChange?.Invoke();
    }

    public async Task SignOutAsync()
    {
        Current = null;
        await storage.RemoveAsync(StorageKey);
        OnChange?.Invoke();
    }

    private async Task TouchAsync()
    {
        if (Current is null) return;
        Current.ExpiresAt = DateTime.UtcNow.Add(Lifetime);
        await storage.SetAsync(StorageKey, Current);
    }
}
