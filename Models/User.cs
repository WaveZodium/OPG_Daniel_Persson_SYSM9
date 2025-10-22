using System;

namespace CookMaster.Models;

public class User {
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty; // plaintext for now (will upgrade later)
    public UserRole Role { get; set; } = UserRole.User;
    public Country Country { get; set; } = Country.Sweden;

    public User(string username, string password,  UserRole role, Country country) {
        Username = username;
        SetPassword(password);
        Role = role;
        Country = country;
    }

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
