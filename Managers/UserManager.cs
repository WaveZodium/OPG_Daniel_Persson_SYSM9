using CookMaster.Models;

namespace CookMaster.Managers;

public class UserManager {
    public List<User> Users { get; set; } = new List<User>();
    private User? _loggedInUser { get; set; } = null;

    public UserManager() {
        SeedDefaults();
    }

    public List<User> GetAllUsers() => Users;

    public bool UserExists(string username) {
        // Validate input
        if (string.IsNullOrWhiteSpace(username)) return false;

        // Case-insensitive search
        return Users.Any(u => string.Equals(u.Username, username.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public bool CreateUser(string username,
                           string password,
                           UserRole role = UserRole.User,
                           Country country = Country.Sweden,
                           string email = "",
                           string securityQuestion = "",
                           string securityAnswer = "") {
        // Validate input
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) return false;

        // Check for existing user
        if (UserExists(username)) return false;

        User user;

        // Create user based on role
        if (role == UserRole.Admin) {
            user = new AdminUser(username.Trim(), password, country, email, securityQuestion, securityAnswer);
        }
        else {
            user = new User(username.Trim(), password, role, country, email, securityQuestion, securityAnswer);
        }

        // Add user
        Users.Add(user);

        // Successful creation
        return true;
    }

    public bool CreateUser(User user) {
        // Validate input
        if (user == null) return false;

        // Check for existing user
        if (UserExists(user.Username)) return false;

        // Add user
        Users.Add(user);

        // Successful creation
        return true;
    }

    public bool DeleteUser(string username) {
        var user = FindUser(username);
        if (user == null) return false;
        Users.Remove(user);
        // If the deleted user was logged in, log them out
        if (_loggedInUser == user) {
            SignOut();
        }
        return true;
    }

    public User? FindUser(string username) {
        // Validate input
        if (string.IsNullOrWhiteSpace(username)) return null;

        // Case-insensitive search
        return Users.Find(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
    }

    public bool SignIn(string username, string password) {
        // Validate input
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) return false;

        // Find user. Case-insensitive search. Return false if not found.
        User? user;
        if ((user = FindUser(username)) is null) return false;

        // Validate password. Return false if invalid.
        if (!user.ValidatePassword(password)) return false;

        // Set logged-in user.
        _loggedInUser = user;

        // Successful login
        return true;
    }

    public void SignOut() => _loggedInUser = null;

    public User? GetLoggedIn() => _loggedInUser;
    public bool IsAdmin => _loggedInUser != null && _loggedInUser.Role == UserRole.Admin;
    public bool IsLoggedIn => _loggedInUser != null;

    // convenience alias if you prefer a property instead of GetLoggedIn()
    public User? CurrentUser => _loggedInUser;

    // Creates a detached copy of a user (preserves concrete type)
    public User CopyUser(User u) {
        if (u is AdminUser admin) {
            var copyAdmin = new AdminUser(admin.Username, admin.Password, admin.Country, admin.Email, admin.SecurityQuestion, admin.SecurityAnswer) {
                Id = admin.Id,
                CreatedAt = admin.CreatedAt
            };
            return copyAdmin;
        }

        var copy = new User(u.Username, u.Password, u.Role, u.Country, u.Email, u.SecurityQuestion, u.SecurityAnswer) {
            Id = u.Id,
            CreatedAt = u.CreatedAt
        };
        return copy;
    }

    // ===== seeding =====
    // Idempotent seeding of required default accounts (users only)
    public void SeedDefaults() {
        // admin
        if (!UserExists("admin")) {
            CreateUser("admin", "password", UserRole.Admin, Country.Sweden, "admin@cookmaster.wavezodium.dev", "What is your favorite color?", "Purple");
        }

        // normal user
        if (!UserExists("user")) {
            CreateUser("user", "password", UserRole.User, Country.Sweden, "user@cookmaster.wavezodium.dev", "What is your pet's name?", "Zitha");
        }

        if (!UserExists("user2")) {
            CreateUser("user2", "password", UserRole.User, Country.Sweden, "user2@cookmaster.wavezodium.dev", "What is your favorite food?", "Carbonara");
        }
    }

    public void Login(string username, string password) => SignIn(username, password);
    public void Logout() => SignOut();
    public void Register(string username, string password, UserRole role, Country country, string email, string securityQuestion, string securityAnswer) {
        CreateUser(username, password, role, country, email, securityQuestion, securityAnswer);
    }
    public void ChangePassword(string oldPassword, string newPassword) {
        if (IsLoggedIn) {
            CurrentUser?.ChangePassword(oldPassword, newPassword);
        }
    }
}
