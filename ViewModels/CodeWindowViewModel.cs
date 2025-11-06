using System;
using System.Security.Cryptography;
using System.Windows;

using CookMaster.MVVM;

namespace CookMaster.ViewModels;

public class CodeWindowViewModel : ViewModelBase {
    // Ask the view to close (true indicates success)
    public event Action<bool>? RequestClose;

    private string _code = GenerateSixDigitCode();
    public string Code {
        get => _code;
        private set => Set(ref _code, value);
    }

    public RelayCommand CopyAndCloseCommand { get; }
    public RelayCommand RegenerateCodeCommand { get; }

    public CodeWindowViewModel() {
        _code = GenerateSixDigitCode();

        CopyAndCloseCommand = new RelayCommand(_ => CopyAndClose(), _ => !string.IsNullOrEmpty(Code));
        RegenerateCodeCommand = new RelayCommand(_ => Code = GenerateSixDigitCode());
    }

    private void CopyAndClose() {
        try {
            Clipboard.SetText(Code);
        }
        catch {
            // ignore (clipboard may be busy); still close
        }
        RequestClose?.Invoke(true);
    }

    private static string GenerateSixDigitCode() {
        var value = RandomNumberGenerator.GetInt32(0, 1_000_000); // 0..999999
        return value.ToString("D6"); // pad with leading zeros
    }
}
