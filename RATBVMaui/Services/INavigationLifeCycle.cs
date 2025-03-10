namespace RATBVMaui.Services;

public interface INavigationLifeCycle
{
    /// <summary>
    /// Should be called in the view's constructor after the call to <c>InitializeComponent</c>
    /// method.
    /// <example>
    /// Boilerplate code that will usually look like:
    /// <code>
    /// ((INavigationLifeCycle)this).Register(this);
    /// </code>
    /// </example>
    /// </summary>
    /// <typeparam name="T">
    /// Indicates that only classes of type <see cref="Page"/> that
    /// implement <see cref="INavigationLifeCycle"/> ca be used as arguments
    /// </typeparam>
    /// <param name="page">
    /// The view that will be used for subscription on lifecycle events
    /// </param>
    public void Register<T>(T page) where T : Page, INavigationLifeCycle
    {
        _ = page ?? throw new ArgumentNullException(nameof(page));

        page.Appearing += Page_Appearing;
        page.Disappearing += Page_Disappearing;
        page.ParentChanging += Page_ParentChanging;
    }

    private void Page_ParentChanging(object? sender, ParentChangingEventArgs e)
    {
        if (sender is Page { BindingContext: INavigationAware lifeCycleAware })
        {
            //lifeCycleAware.OnParentChanging(e);
        }
    }

    private async void Page_Appearing(object? sender, EventArgs e)
    {
        if (sender is Page { BindingContext: INavigationAware lifeCycleAware })
        {
            await lifeCycleAware.OnPageAppearing();
        }
    }

    private async void Page_Disappearing(object? sender, EventArgs e)
    {
        if (sender is Page { BindingContext: INavigationAware lifeCycleAware })
        {
            await lifeCycleAware.OnPageDisappearing();
        }
    }
}
