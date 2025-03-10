using RATBVMaui.Services;
using RATBVMaui.ViewModels;

namespace RATBVMaui.Views;

public partial class BusLinesPage : INavigationLifeCycle
{
    public BusLinesPage(BusLinesViewModel viewModel)
    {
        InitializeComponent();

        ((INavigationLifeCycle)this).Register(this);

        BindingContext = viewModel;
    }
}
