using CookMaster.Managers;
using CookMaster.MVVM;

namespace CookMaster.ViewModels;

public class RecipeDetailWindowViewModel : ViewModelBase {
    private readonly RecipeManager _recipeManager;
    private readonly IServiceProvider _services;

    // Close event for the view (parameter indicates success)
    public event Action<bool>? RequestClose;

    public RelayCommand PerformSaveCommand { get; }
    public RelayCommand PerformDeleteCommand { get; }
    public RelayCommand PerformCloseCommand { get; }

    public RecipeDetailWindowViewModel(RecipeManager recipeManager, IServiceProvider services) {
        _recipeManager = recipeManager;
        _services = services;

        PerformSaveCommand = new RelayCommand(_ => PerformSave());
        PerformCloseCommand = new RelayCommand(_ => PerformClose());
        PerformDeleteCommand = new RelayCommand(_ => PerformDelete());
    }

    private void PerformSave() {
        // Request the view to close and indicate success
        RequestClose?.Invoke(true);
    }

    private void PerformClose() {
        // Request the view to close and indicate cancellation
        // which in this case is that no changes were saved.
        RequestClose?.Invoke(false);
    }

    private void PerformDelete() {
        RequestClose?.Invoke(true);
    }
}
