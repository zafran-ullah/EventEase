using System.ComponentModel.DataAnnotations;

namespace EventEase.Models;

/// <summary>A persisted registration (one party, one or more tickets).</summary>
public class Registration
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int EventId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public int Tickets { get; set; } = 1;
    public string? Notes { get; set; }
    public string ConfirmationCode { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; } = DateTime.Now;
    public bool CheckedIn { get; set; }
    public DateTime? CheckedInAt { get; set; }
}

/// <summary>Form model for the Registration Form, with strict input validation.</summary>
public class RegistrationModel
{
    [Required(ErrorMessage = "Please enter your full name.")]
    [StringLength(60, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 60 characters.")]
    [RegularExpression(@"^[\p{L}][\p{L}\s'.\-]*$", ErrorMessage = "Name may only contain letters, spaces, apostrophes, dots and hyphens.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your email address.")]
    [StringLength(120, ErrorMessage = "Email cannot exceed 120 characters.")]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$", ErrorMessage = "Please enter a valid email address (e.g. name@company.com).")]
    public string Email { get; set; } = string.Empty;

    [RegularExpression(@"^\+?[0-9\s\-()]{7,20}$", ErrorMessage = "Please enter a valid phone number (7–20 digits).")]
    public string? Phone { get; set; }

    [StringLength(80, ErrorMessage = "Company cannot exceed 80 characters.")]
    public string? Company { get; set; }

    [Range(1, 10, ErrorMessage = "You can book between 1 and 10 tickets.")]
    public int Tickets { get; set; } = 1;

    [StringLength(300, ErrorMessage = "Notes cannot exceed 300 characters.")]
    public string? Notes { get; set; }

    [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the terms to register.")]
    public bool AcceptTerms { get; set; }
}

/// <summary>Outcome of a registration attempt (business-rule validation).</summary>
public sealed record RegistrationResult(bool Success, Registration? Registration, string? Field, string? Error)
{
    public static RegistrationResult Ok(Registration r) => new(true, r, null, null);
    public static RegistrationResult Fail(string error, string? field = null) => new(false, null, field, error);
}
