using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using EventEase.Services;

namespace EventEase.Models;

/// <summary>The signed-in user's session, persisted to localStorage.</summary>
public class UserSession
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime ExpiresAt { get; set; }

    [JsonIgnore] public string FirstName => Text.FirstName(Name);
    [JsonIgnore] public string Initials => Text.Initials(Name);
}

/// <summary>Sign-in form model.</summary>
public class SignInModel
{
    [Required(ErrorMessage = "Please enter your name.")]
    [StringLength(60, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 60 characters.")]
    [RegularExpression(@"^[\p{L}][\p{L}\s'.\-]*$", ErrorMessage = "Name may only contain letters, spaces, apostrophes, dots and hyphens.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your email address.")]
    [StringLength(120)]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$", ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = string.Empty;
}
