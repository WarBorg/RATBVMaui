namespace RATBVMaui.Services;

public interface INavigationAware
{
    /// <summary>
    /// Called every time the page will appear (either navigating to a new page or coming back from one).
    /// </summary>
    public ValueTask OnPageAppearing();

    /// <summary>
    /// Called every time the page will disappear (either navigating to a new page or coming back from one).
    /// </summary>
    public ValueTask OnPageDisappearing();
}
