using EventEase.Models;

namespace EventEase.Services;

/// <summary>Handles registrations and attendance (check-in) state.</summary>
public class RegistrationService(BrowserStorage storage, EventService events)
{
    private const string StorageKey = "eventease.registrations.v1";
    private List<Registration> _items = [];

    public event Action? OnChange;

    public async Task InitializeAsync() =>
        _items = await storage.GetAsync<List<Registration>>(StorageKey) ?? [];

    public IReadOnlyList<Registration> All => _items;

    public int SeatsTaken(int eventId) => _items.Where(r => r.EventId == eventId).Sum(r => r.Tickets);

    public int SeatsRemaining(Event ev) => Math.Max(0, ev.Capacity - SeatsTaken(ev.Id));

    public List<Registration> ForEvent(int eventId) =>
        _items.Where(r => r.EventId == eventId).OrderBy(r => r.FullName).ToList();

    public List<Registration> ForEmail(string email) =>
        _items.Where(r => string.Equals(r.Email, email, StringComparison.OrdinalIgnoreCase))
              .OrderByDescending(r => r.RegisteredAt).ToList();

    public Registration? FindByCode(int eventId, string code) =>
        _items.FirstOrDefault(r => r.EventId == eventId &&
                                   string.Equals(r.ConfirmationCode, code.Trim(), StringComparison.OrdinalIgnoreCase));

    public bool IsRegistered(int eventId, string email) =>
        _items.Any(r => r.EventId == eventId && string.Equals(r.Email, email.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>Applies business rules that data annotations can't express (capacity, duplicates, closed events).</summary>
    public async Task<RegistrationResult> RegisterAsync(int eventId, RegistrationModel model)
    {
        var ev = events.GetById(eventId);
        if (ev is null) return RegistrationResult.Fail("This event no longer exists.");
        if (ev.IsPast) return RegistrationResult.Fail("Registration for this event has closed.");

        var email = model.Email.Trim().ToLowerInvariant();
        if (IsRegistered(eventId, email))
            return RegistrationResult.Fail("This email is already registered for this event.", nameof(RegistrationModel.Email));

        var remaining = SeatsRemaining(ev);
        if (remaining == 0) return RegistrationResult.Fail("Sorry, this event is sold out.");
        if (model.Tickets > remaining)
            return RegistrationResult.Fail($"Only {remaining} seat(s) remaining.", nameof(RegistrationModel.Tickets));

        var registration = new Registration
        {
            EventId = eventId,
            FullName = model.FullName.Trim(),
            Email = email,
            Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim(),
            Company = string.IsNullOrWhiteSpace(model.Company) ? null : model.Company.Trim(),
            Notes = string.IsNullOrWhiteSpace(model.Notes) ? null : model.Notes.Trim(),
            Tickets = model.Tickets,
            ConfirmationCode = NewCode()
        };

        _items.Add(registration);
        await PersistAsync();
        return RegistrationResult.Ok(registration);
    }

    public async Task SetCheckInAsync(Guid id, bool checkedIn)
    {
        var r = _items.FirstOrDefault(x => x.Id == id);
        if (r is null || r.CheckedIn == checkedIn) return;
        r.CheckedIn = checkedIn;
        r.CheckedInAt = checkedIn ? DateTime.Now : null;
        await PersistAsync();
    }

    public async Task CancelAsync(Guid id)
    {
        if (_items.RemoveAll(x => x.Id == id) > 0) await PersistAsync();
    }

    /// <summary>Adds sample attendees so the Attendance Tracker can be demoed quickly.</summary>
    public async Task SeedDemoAsync(int eventId, int count = 12)
    {
        var ev = events.GetById(eventId);
        if (ev is null) return;
        string[] first = ["Ayesha", "Bilal", "Chen", "Diego", "Elena", "Farhan", "Grace", "Hamza", "Isabel", "Jonas", "Kiran", "Layla", "Mateo", "Noor", "Omar", "Priya"];
        string[] last = ["Khan", "Ahmed", "Wei", "Garcia", "Rossi", "Malik", "Hopper", "Siddiqui", "Silva", "Berg", "Patel", "Haddad"];
        var rng = new Random();
        var added = 0;
        for (var i = 0; i < count * 3 && added < count && SeatsRemaining(ev) > 0; i++)
        {
            var name = $"{first[rng.Next(first.Length)]} {last[rng.Next(last.Length)]}";
            var email = $"{name.Replace(' ', '.').ToLowerInvariant()}@example.com";
            if (IsRegistered(eventId, email)) continue;
            var checkedIn = rng.NextDouble() < 0.4;
            _items.Add(new Registration
            {
                EventId = eventId, FullName = name, Email = email,
                Tickets = Math.Min(rng.Next(1, 3), SeatsRemaining(ev)),
                ConfirmationCode = NewCode(),
                RegisteredAt = DateTime.Now.AddHours(-rng.Next(1, 200)),
                CheckedIn = checkedIn, CheckedInAt = checkedIn ? DateTime.Now : null
            });
            added++;
        }
        await PersistAsync();
    }

    private string NewCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        string code;
        do
        {
            code = "EE-" + new string(Enumerable.Range(0, 6).Select(_ => chars[Random.Shared.Next(chars.Length)]).ToArray());
        } while (_items.Any(r => r.ConfirmationCode == code));
        return code;
    }

    private async Task PersistAsync()
    {
        await storage.SetAsync(StorageKey, _items);
        OnChange?.Invoke();
    }
}
