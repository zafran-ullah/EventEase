using System.ComponentModel.DataAnnotations;
using EventEase.Models;

namespace EventEase.Services;

/// <summary>
/// Central store for events. Registered as a singleton so every component
/// shares the same state; components subscribe to <see cref="OnChange"/>.
/// </summary>
public class EventService(BrowserStorage storage)
{
    private const string StorageKey = "eventease.events.v1";
    private List<Event> _events = [];
    private bool _initialized;

    public event Action? OnChange;

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        var stored = await storage.GetAsync<List<Event>>(StorageKey);
        // Drop stored events that have since passed and fall back to fresh seed data if nothing is left.
        _events = stored is { Count: > 0 } && stored.Any(e => !e.IsPast) ? stored : Seed();
        _initialized = true;
    }

    public IReadOnlyList<Event> GetAll() => _events.OrderBy(e => e.Date).ToList();

    public IReadOnlyList<Event> GetUpcoming(int take = int.MaxValue) =>
        _events.Where(e => !e.IsPast).OrderBy(e => e.Date).Take(take).ToList();

    public Event? GetById(int id) => _events.FirstOrDefault(e => e.Id == id);

    /// <summary>Validates and persists an updated event. Returns the list of validation errors (empty on success).</summary>
    public async Task<IReadOnlyList<string>> UpdateAsync(Event updated)
    {
        var errors = new List<ValidationResult>();
        if (!Validator.TryValidateObject(updated, new ValidationContext(updated), errors, validateAllProperties: true))
            return errors.Select(e => e.ErrorMessage ?? "Invalid value.").ToList();

        var index = _events.FindIndex(e => e.Id == updated.Id);
        if (index < 0) return ["This event no longer exists."];

        _events[index] = updated.Clone();
        await storage.SetAsync(StorageKey, _events);
        OnChange?.Invoke();
        return [];
    }

    public async Task ResetAsync()
    {
        _events = Seed();
        await storage.RemoveAsync(StorageKey);
        OnChange?.Invoke();
    }

    private static List<Event> Seed()
    {
        var d = DateTime.Today;
        return
        [
            new() { Id = 1, Name = "Future of Work Summit", Category = EventCategory.Conference, Location = "Grand Hyatt, Dubai",
                    Date = d.AddDays(6).AddHours(9), Capacity = 250,
                    Description = "A full-day summit exploring hybrid teams, AI copilots and the next decade of corporate culture with 20+ speakers." },
            new() { Id = 2, Name = "Design Systems Masterclass", Category = EventCategory.Workshop, Location = "Studio 54, Lahore",
                    Date = d.AddDays(10).AddHours(14), Capacity = 30,
                    Description = "Hands-on workshop: build a scalable design system from tokens to components. Laptops required." },
            new() { Id = 3, Name = "Founders & Funders Mixer", Category = EventCategory.Networking, Location = "Skyline Rooftop, Karachi",
                    Date = d.AddDays(13).AddHours(19), Capacity = 120,
                    Description = "An evening of curated introductions between early-stage founders and active angel investors." },
            new() { Id = 4, Name = "Winter Charity Gala", Category = EventCategory.Gala, Location = "Pearl Continental, Islamabad",
                    Date = d.AddDays(21).AddHours(20), Capacity = 400,
                    Description = "Black-tie dinner, live orchestra and silent auction supporting children's education programmes." },
            new() { Id = 5, Name = "Blazor Performance Deep Dive", Category = EventCategory.Webinar, Location = "Online · Zoom",
                    Date = d.AddDays(4).AddHours(16), Capacity = 1000,
                    Description = "Learn virtualization, render optimisation and AOT compilation techniques for production Blazor apps." },
            new() { Id = 6, Name = "Leadership Offsite Retreat", Category = EventCategory.Workshop, Location = "Serena Resort, Hunza",
                    Date = d.AddDays(35).AddHours(10), Capacity = 40,
                    Description = "Two days of strategy sessions, mountain hikes and facilitated leadership coaching for senior managers." }
        ];
    }
}
