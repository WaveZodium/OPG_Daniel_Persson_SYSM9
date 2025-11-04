using System;
using System.Linq;

namespace CookMaster.Services;

public readonly record struct PasswordStrengthResult(int Score, string Label);

public static class PasswordStrengthService {
    // Score: 0..4
    public static PasswordStrengthResult Evaluate(string password) {
        var pwd = password ?? string.Empty;

        bool hasLower = pwd.Any(char.IsLower);
        bool hasUpper = pwd.Any(char.IsUpper);
        bool hasDigit = pwd.Any(char.IsDigit);
        bool hasSymbol = pwd.Any(ch => !char.IsLetterOrDigit(ch));

        int score = 0;
        if (pwd.Length >= 8) score++;
        if (hasLower && hasUpper) score++;
        if (hasDigit) score++;
        if (hasSymbol) score++;

        score = Math.Clamp(score, 0, 4);

        string label = score switch {
            0 or 1 => pwd.Length > 0 && pwd.Length < 8 ? "Too short" : "Weak",
            2 => "Fair",
            3 => "Strong",
            4 => "Very strong",
            _ => "Weak"
        };

        return new PasswordStrengthResult(score, label);
    }
}
