using RATBVMaui.Views;

namespace RATBVMaui;

public partial class App : Application
{
    private readonly AppShell _shell;

    public App(AppShell shell)
    {
        InitializeComponent();

        _shell = shell;

        Routing.RegisterRoute(typeof(BusLinesPage).FullName, typeof(BusLinesPage));
    }

    protected override Window CreateWindow(IActivationState? activationState) => new(_shell);
}
