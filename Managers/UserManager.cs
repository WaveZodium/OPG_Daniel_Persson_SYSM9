using System;
using System.Collections.Generic;
using System.Linq;
using CookMaster.Models;

namespace CookMaster.Managers;

public class UserManager {
    public List<User> Users { get; set; } = new List<User>();
    private User? _loggedInUser { get; set; } = null;

    public bool UserExists(string username) {
        if (string.IsNullOrWhiteSpace(username)) return false;
        return Users.Any(u => string.Equals(u.Username, username.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public bool CreateUser(string username, string password, UserRole role = UserRole.User, Country country = Country.Sweden) {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) return false;
        if (UserExists(username)) return false;

        User user;

        if (role == UserRole.Admin) {
            user = new AdminUser(username.Trim(), password, country);
        }
        else {
            user = new User(username.Trim(), password, role, country);
        }
        Users.Add(user);

        return true;
    }

    public User? FindUser(string username) {
        return Users.Find(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
    }

    public bool SignIn(string username, string password) {
        var user = FindUser(username);
        if (user == null) return false;
        if (!user.ValidatePassword(password)) return false;
        _loggedInUser = user;
        return true;
    }

    public void SignOut() => _loggedInUser = null;

    public User? GetLoggedIn() => _loggedInUser;

    // ===== seeding =====
    // Idempotent seeding of required default accounts (users only)
    public void SeedDefaults() {
        // admin
        if (!UserExists("admin")) {
            CreateUser("admin", "password", UserRole.Admin, Country.Sweden);
        }

        // normal user
        if (!UserExists("user")) {
            CreateUser("user", "password", UserRole.User, Country.Sweden);
        }
    }

    // placeholder methods left in place for assignment
    public void Login() { }
    public void Logout() { }
    public void Register() { }
    public void ChangePassword() { }
}
