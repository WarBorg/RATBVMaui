using RATBVMaui.ViewModels;

namespace RATBVMaui;

public partial class AppShell : Shell
{
    private readonly BusLinesViewModel _busLinesViewModel;

    public AppShell(BusLinesViewModel viewModel)
    {
        InitializeComponent();

        _busLinesViewModel = viewModel;

        Navigated -= OnNavigated;
        Navigated += OnNavigated;
    }

    private void OnNavigated(object? sender, ShellNavigatedEventArgs e)
    {
        // Get the current route (tab name)
        var currentRoute = e.Current.Location.OriginalString.TrimStart('/');

        // Update ViewModel based on the selected tab
        _busLinesViewModel.UpdateCurrentItems(currentRoute);
    }
}
