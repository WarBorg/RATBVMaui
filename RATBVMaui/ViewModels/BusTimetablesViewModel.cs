using System.Windows.Input;
using Acr.UserDialogs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RATBVData.Models.Enums;
using RATBVData.Models.Models;
using RATBVMaui.Constants;
using RATBVMaui.Exceptions;
using RATBVMaui.Helpers;
using RATBVMaui.Services;

namespace RATBVMaui.ViewModels;

[QueryProperty(AppNavigation.BusStation, nameof(BusStation))]
public partial class BusTimetablesViewModel : BusViewModelBase
{
    #region Fields

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BusLineAndStation))]
    private BusStationModel? _busStation;

    #endregion

    #region Properties

    public RangeObservableCollection<BusTimeTableModel> BusTimetableWeekdays { get; } = [];

    public RangeObservableCollection<BusTimeTableModel> BusTimetableSaturday { get; } = [];

    public RangeObservableCollection<BusTimeTableModel> BusTimetableSunday { get; } = [];

    public RangeObservableCollection<BusTimeTableModel> BusTimetableHolidayWeekdays { get; } = [];

    // TODO Add bus line number
    public string BusLineAndStation => BusStation is null
        ? string.Empty
        : BusStation.Name;

    public override string Title =>
        OperatingSystem.IsWindows()
            ? $"{BusLineAndStation} - Updated on {LastUpdated}"
            : BusLineAndStation;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    private string _lastUpdated = "never";

    [ObservableProperty]
    private bool _isBusy;

    #endregion

    #region Constructors and Dependencies

    private readonly IBusRepository _busRepository;
    private readonly IUserDialogs _userDilaogsService;
    private readonly IConnectivityService _connectivityService;

    public BusTimetablesViewModel(IBusRepository busRepository,
        IUserDialogs userDialogsService,
        IConnectivityService connectivityService)
    {
        _busRepository = busRepository ?? throw new ArgumentException(nameof(busRepository));
        _userDilaogsService = userDialogsService ?? throw new ArgumentException(nameof(userDialogsService));
        _connectivityService = connectivityService ?? throw new ArgumentException(nameof(connectivityService));
    }

    #endregion

    #region Command Methods

    [RelayCommand]
    private async Task RefreshTimetables()
    {
        if (_connectivityService.IsInternetAvailable)
        {
            using (_userDilaogsService.Loading("Fetching Data... "))
            {
                await GetBusTimeTableAsync(isForcedRefresh: true);
            }
        }
        else
        {
            _userDilaogsService.Toast("No Internet connection detected");
        }

        IsBusy = false;
    }

    #endregion

    #region Navigation Override Methods

    protected override async ValueTask OnPageInitialized()
    {
        using (_userDilaogsService.Loading("Fetching Data... "))
        {
            await GetBusTimeTableAsync(isForcedRefresh: false);
        }
    }

    #endregion

    #region Methods

    private async Task GetBusTimeTableAsync(bool isForcedRefresh)
    {
        if (BusStation is null)
        {
            _userDilaogsService.Toast("No Timetables found for selected bus station");

            return;
        }

        if (BusStation.Id is null)
        {
            _userDilaogsService.Toast($"Timetable database error for {BusStation.Name} bus station");

            return;
        }

        try
        {
            var busTimetables = await _busRepository.GetBusTimetablesAsync(BusStation.ScheduleLink,
                BusStation.Id.Value,
                isForcedRefresh);

            LastUpdated = busTimetables
                .FirstOrDefault()
                ?.LastUpdateDate
                ?? "never";

            GetTimeTableByTimeOfWeek(busTimetables);
        }
        catch (NoInternetException)
        {
            _userDilaogsService.Toast("Internet connection is necessary to get bus timetables",
                dismissTimer: TimeSpan.FromSeconds(5));
        }
        catch (Exception)
        {
            _userDilaogsService.Toast("Something went wrong when getting bus timetables");
        }
    }

    private void GetTimeTableByTimeOfWeek(List<BusTimeTableModel> busTimetable)
    {
        BusTimetableWeekdays
            .ReplaceRange(busTimetable.Where(btt => btt.TimeOfWeek == TimeOfTheWeek.WeekDays.ToString()));

        BusTimetableSaturday
            .ReplaceRange(busTimetable.Where(btt => btt.TimeOfWeek == TimeOfTheWeek.Saturday.ToString()));

        BusTimetableSunday
            .ReplaceRange(busTimetable.Where(btt => btt.TimeOfWeek == TimeOfTheWeek.Sunday.ToString()));
    }

    #endregion
}
