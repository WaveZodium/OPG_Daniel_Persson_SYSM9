using System.Windows.Media;
using CookMaster.Managers;
using CookMaster.Models;
using CookMaster.MVVM;
using CookMaster.Services;

namespace CookMaster.ViewModels
{
    public class ForgotPasswordViewModel : ViewModelBase {
        private readonly UserManager _userManager;

        // Event used to ask the view to close. Parameter indicates success (true) or cancel/fail (false).
        public event Action<bool>? RequestClose;

        public RelayCommand PerformCancelCommand { get; }
        public RelayCommand LoadSecurityQuestionCommand { get; }
        public RelayCommand CheckSecurityAnswerCommand { get; }
        public RelayCommand PerformUpdatePasswordCommand { get; }

        private User? _user;
        public User? User {
            get => _user;
            private set => Set(ref _user, value);
        }

        private string _username = string.Empty;
        public string Username {
            get => _username;
            set {
                if (Set(ref _username, value)) {
                    // Reset when username changes
                    CanEditFields = false;
                    CanEditPassword = false;
                }
            }
        }

        // New MVVM-friendly focus request property (bind TwoWay to attached behavior)
        private bool _focusUsername = true;
        public bool FocusUsername {
            get => _focusUsername;
            set => Set(ref _focusUsername, value);
        }

        private string _newPassword = string.Empty;
        public string NewPassword {
            get => _newPassword;
            set {
                if (Set(ref _newPassword, value)) {
                    UpdatePasswordStrength(value);
                    UpdatePasswordMatch();
                    PerformUpdatePasswordCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private string _confirmNewPassword = string.Empty;
        public string ConfirmNewPassword {
            get => _confirmNewPassword;
            set {
                if (Set(ref _confirmNewPassword, value)) {
                    UpdatePasswordMatch();
                    PerformUpdatePasswordCommand.RaiseCanExecuteChanged();
                }
            }
        }

        // ===== Password match bindables (computed) =====
        public bool PasswordsMatch =>
            !string.IsNullOrEmpty(ConfirmNewPassword) &&
            string.Equals(NewPassword ?? string.Empty, ConfirmNewPassword ?? string.Empty, StringComparison.Ordinal);

        public string PasswordMatchMessage =>
        PasswordsMatch ? "Passwords match" : "Passwords do not match";

        public Brush PasswordMatchBrush =>
            PasswordsMatch ? Brushes.ForestGreen : Brushes.IndianRed;

        // Show the message only after user starts typing in Confirm Password
        public bool ShowPasswordMatchMessage => !string.IsNullOrEmpty(ConfirmNewPassword);

        private void UpdatePasswordMatch() {
            OnPropertyChanged(nameof(PasswordsMatch));
            OnPropertyChanged(nameof(PasswordMatchMessage));
            OnPropertyChanged(nameof(PasswordMatchBrush));
            OnPropertyChanged(nameof(ShowPasswordMatchMessage));
        }

        private string _securityQuestion = string.Empty;
        public string SecurityQuestion {
            get => _securityQuestion;
            set => Set(ref _securityQuestion, value);
        }

        private string _securityAnswer = string.Empty;
        public string SecurityAnswer {
            get => _securityAnswer;
            set => Set(ref _securityAnswer, value);
        }

        // Controls whether answer field is enabled (user found)
        private bool _canEditFields;
        public bool CanEditFields {
            get => _canEditFields;
            private set => Set(ref _canEditFields, value);
        }

        // Controls whether password fields are enabled (answer verified)
        private bool _canEditPassword;
        public bool CanEditPassword {
            get => _canEditPassword;
            private set => Set(ref _canEditPassword, value);
        }

        // ===== Password strength bindables (same shape as Register VM) =====
        private int _passwordStrengthScore; // 0..4
        public int PasswordStrengthScore {
            get => _passwordStrengthScore;
            private set {
                if (Set(ref _passwordStrengthScore, value)) {
                    OnPropertyChanged(nameof(PasswordStrengthBar1));
                    OnPropertyChanged(nameof(PasswordStrengthBar2));
                    OnPropertyChanged(nameof(PasswordStrengthBar3));
                    OnPropertyChanged(nameof(PasswordStrengthBar4));
                }
            }
        }

        private string _passwordStrengthText = string.Empty;
        public string PasswordStrengthText {
            get => _passwordStrengthText;
            private set => Set(ref _passwordStrengthText, value);
        }

        private Brush _passwordStrengthBrush = Brushes.Gray;
        public Brush PasswordStrengthBrush {
            get => _passwordStrengthBrush;
            private set => Set(ref _passwordStrengthBrush, value);
        }

        public bool PasswordStrengthBar1 => PasswordStrengthScore >= 1;
        public bool PasswordStrengthBar2 => PasswordStrengthScore >= 2;
        public bool PasswordStrengthBar3 => PasswordStrengthScore >= 3;
        public bool PasswordStrengthBar4 => PasswordStrengthScore >= 4;

        public ForgotPasswordViewModel(UserManager userManager) {
            _userManager = userManager;

            PerformCancelCommand = new RelayCommand(_ => PerformCancel());
            LoadSecurityQuestionCommand = new RelayCommand(_ => LoadSecurityQuestion());
            CheckSecurityAnswerCommand = new RelayCommand(_ => CheckSecurityAnswer());
            PerformUpdatePasswordCommand = new RelayCommand(_ => PerformUpdatePassword(), _ => PasswordsMatch);

            CanEditFields = false;
            CanEditPassword = false;

            // Request initial focus on the username box (bound to the attached behavior)
            FocusUsername = true;
        }

        private void UpdatePasswordStrength(string? value) {
            var result = PasswordStrengthService.Evaluate(value ?? string.Empty);
            PasswordStrengthScore = result.Score;
            PasswordStrengthText = result.Label;
            PasswordStrengthBrush = result.Score switch {
                0 or 1 => Brushes.IndianRed,
                2 => Brushes.Orange,
                3 => Brushes.YellowGreen,
                4 => Brushes.ForestGreen,
                _ => Brushes.Gray
            };
        }

        private void CheckSecurityAnswer() {
            if (User == null) return;

            if (string.Equals(User.SecurityAnswer, SecurityAnswer, StringComparison.Ordinal)) {
                CanEditPassword = true;
            } else {
                CanEditPassword = false;
                // Optionally expose an error message property for the view
            }
        }

        private void LoadSecurityQuestion() {
            var user = _userManager.FindUser(Username?.Trim() ?? string.Empty);

            if (user != null) {
                User = user;
                SecurityQuestion = user.SecurityQuestion;
                CanEditFields = true;
            } else {
                User = null;
                SecurityQuestion = "User not found.";
                CanEditFields = false;
                CanEditPassword = false;
                // Optionally clear entered fields
                // SecurityAnswer = string.Empty;
                // NewPassword = string.Empty;
                // ConfirmPassword = string.Empty;
            }
        }

        private void PerformUpdatePassword() {
            if (User == null || !PasswordsMatch) return;

            // Update the user's password and close dialog as success
            User.SetPassword(NewPassword);
            RequestClose?.Invoke(true);
        }

        private void PerformCancel() {
            RequestClose?.Invoke(false);
        }
    }
}
