using System;

namespace CookMaster.Models;

public class User {
    private Guid Id { get; init; } = Guid.NewGuid();
    private DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    private string Username { get; set; } = string.Empty;
    private string Password { get; set; } = string.Empty; // plaintext for now (will upgrade later)
    private UserRole Role { get; set; } = UserRole.User;
    private Country Country { get; set; } = Country.Sweden;

    public bool ValidateLogin(string username, string password) {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) return false;

        // Trim input and compare case-insensitive
        if (!string.Equals(Username, username.Trim(), StringComparison.OrdinalIgnoreCase)) return false;

        return ValidatePassword(password);
    }

    public bool ChangePassword(string oldPassword, string newPassword) {
        if (string.IsNullOrWhiteSpace(newPassword)) return false;
        if (!ValidatePassword(oldPassword)) return false;
        Password = newPassword;
        return true;
    }

    public void UpdateDetails(string? username = null, Country? country = null) {
        if (!string.IsNullOrWhiteSpace(username))
            Username = username!.Trim();

        if (country.HasValue)
            Country = country.Value;
    }

    public bool ValidatePassword(string plainPassword) => Password == (plainPassword ?? string.Empty);

    // Simple setter so later can switch this to hashing in one place
    public void SetPassword(string plainPassword) => Password = plainPassword ?? string.Empty;
}
