using RATBVMaui.ViewModels;

namespace RATBVMaui.Services;

public class NavigationService : INavigationService
{
    public async Task NavigateToAsync<TViewModel>()
        where TViewModel : BusViewModelBase =>
        await InternalNavigateToAsync(typeof(TViewModel), false);

    public async Task NavigateToAsync<TViewModel>(bool isAbsoluteRoute)
        where TViewModel : BusViewModelBase =>
        await InternalNavigateToAsync(typeof(TViewModel), isAbsoluteRoute);

    public async Task NavigateToAsync<TViewModel>(IDictionary<string, object> parameter)
        where TViewModel : BusViewModelBase =>
        await InternalNavigateToAsync(typeof(TViewModel), parameter, false);

    public async Task NavigateToRouteByNameAsync(string routeName) =>
        await Shell.Current.GoToAsync($"//{routeName}");

    public async Task GoBackAsync() =>
        await Shell.Current.GoToAsync("..");

    private static async Task InternalNavigateToAsync(Type viewModelType, bool isAbsoluteRoute)
    {
        var navigationRoute = GetViewNavigationRoute(viewModelType, isAbsoluteRoute);

        await Shell.Current.GoToAsync(navigationRoute);
    }

    private static async Task InternalNavigateToAsync(
        Type viewModelType,
        IDictionary<string, object> parameter,
        bool isAbsoluteRoute)
    {
        var navigationRoute = GetViewNavigationRoute(viewModelType, isAbsoluteRoute);

        await Shell.Current.GoToAsync(navigationRoute, parameter);
    }

    private static string GetViewNavigationRoute(Type viewModelType, bool isAbsoluteRoute)
    {
        var viewName = viewModelType
            .FullName
            ?.Replace("ViewModels", "Views")
            .Replace("ViewModel", "Page");

        var absolutePrefix = isAbsoluteRoute ? "///" : string.Empty;

        return $"{absolutePrefix}{viewName}";
    }
}