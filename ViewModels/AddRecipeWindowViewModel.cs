using System;
using System.Windows;
using CookMaster.MVVM;
using CookMaster.Managers;
using CookMaster.Models;

namespace CookMaster.ViewModels;

public class AddRecipeWindowViewModel : ViewModelBase {
    private readonly RecipeManager _recipeManager;
    private readonly UserManager _userManager;
    private readonly IServiceProvider _services;

    // Close event for the view (parameter indicates success)
    public event Action<bool>? RequestClose;

    public RelayCommand PerformAddCommand { get; }
    public RelayCommand PerformCancelCommand { get; }

    public AddRecipeWindowViewModel(RecipeManager recipeManager, UserManager userManager, IServiceProvider services) {
        _recipeManager = recipeManager;
        _userManager = userManager;
        _services = services;

        PerformAddCommand = new RelayCommand(_ => PerformAdd());
        PerformCancelCommand = new RelayCommand(_ => PerformCancel());
    }

    private void PerformAdd() {
        // Request the view to close and indicate success
        RequestClose?.Invoke(true);
    }

    private void PerformCancel() {
        // Request the view to close and indicate cancellation
        RequestClose?.Invoke(false);
    }
}
