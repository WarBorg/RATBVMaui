using CommunityToolkit.Mvvm.ComponentModel;
using RATBVMaui.Services;

namespace RATBVMaui.ViewModels;

public abstract class BusViewModelBase : ObservableObject, INavigationAware
{
    #region Fields

    private bool _isLoaded;

    #endregion
    #region Properties

    public virtual string Title { get; set; } = string.Empty;

    #endregion

    #region INavigationAware Virtual Methods

    public async ValueTask OnPageAppearing()
    {
        if (_isLoaded)
        {
            return;
        }

        _isLoaded = true;
        await OnPageInitialized();
    }

    public async ValueTask OnPageDisappearing() => await OnCleanUp();

    #endregion

    protected virtual ValueTask OnPageInitialized() => default;

    protected virtual ValueTask OnCleanUp() => default;
}
