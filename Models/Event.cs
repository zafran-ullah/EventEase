using System.ComponentModel.DataAnnotations;

namespace EventEase.Models;

public enum EventCategory
{
    Conference,
    Workshop,
    Networking,
    Gala,
    Webinar
}

/// <summary>
/// Core event entity. Data annotations drive validation both in the
/// EventCard inline editor and in <see cref="Services.EventService"/>.
/// </summary>
public class Event
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Event name is required.")]
    [StringLength(80, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 80 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Event date is required.")]
    [FutureDate(ErrorMessage = "Event date must be in the future.")]
    public DateTime Date { get; set; } = DateTime.Today.AddDays(7).AddHours(18);

    [Required(ErrorMessage = "Location is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Location must be between 2 and 100 characters.")]
    public string Location { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string Description { get; set; } = string.Empty;

    [Range(1, 10_000, ErrorMessage = "Capacity must be between 1 and 10,000.")]
    public int Capacity { get; set; } = 100;

    public EventCategory Category { get; set; } = EventCategory.Conference;

    public bool IsPast => Date < DateTime.Now;

    /// <summary>Shallow copy used for draft editing so that the original isn't mutated until Save.</summary>
    public Event Clone() => (Event)MemberwiseClone();
}

/// <summary>Validates that a <see cref="DateTime"/> lies in the future.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class FutureDateAttribute : ValidationAttribute
{
    public override bool IsValid(object? value) =>
        value is not DateTime date || date > DateTime.Now;
}
