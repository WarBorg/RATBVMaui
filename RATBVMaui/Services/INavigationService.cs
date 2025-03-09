using RATBVMaui.ViewModels;

namespace RATBVMaui.Services;

public interface INavigationService
{
    Task NavigateToAsync<TViewModel>() where TViewModel : BusViewModelBase;

    Task NavigateToAsync<TViewModel>(bool isAbsoluteRoute) where TViewModel : BusViewModelBase;

    Task NavigateToAsync<TViewModel>(IDictionary<string, object> parameter) where TViewModel : BusViewModelBase;

    Task NavigateToRouteByNameAsync(string routeName);

    Task GoBackAsync();
}
