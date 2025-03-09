using Acr.UserDialogs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RATBVData.Models.Models;
using RATBVMaui.Constants;
using RATBVMaui.Exceptions;
using RATBVMaui.Helpers;
using RATBVMaui.Services;

namespace RATBVMaui.ViewModels;

[QueryProperty(AppNavigation.BusLine, nameof(BusLine))]
public partial class BusStationsViewModel : BusViewModelBase
{
    #region Fields

    private string _directionLink = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BusLineName))]
    private BusLineModel? _busLine;

    #endregion

    #region Properties

    public override string Title =>
        OperatingSystem.IsWindows()
            ? $"Bus Stations - Updated on {LastUpdated}"
            : "Bus Stations";

    public RangeObservableCollection<BusStationsItemViewModel> BusStations { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BusLineName))]
    private string _direction = string.Empty;

    public string BusLineName => BusLine is null
        ? string.Empty
        : $"{BusLine.Name} - {Direction}";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    private string _lastUpdated = "never";

    [ObservableProperty]
    private bool _isBusy;

    #endregion

    #region Constructors and Dependencies

    private readonly IBusRepository _busRepository;
    private readonly IUserDialogs _userDialogsService;
    private readonly IConnectivityService _connectivityService;
    private readonly INavigationService _navigationService;

    public BusStationsViewModel(IBusRepository busRepository,
        IUserDialogs userDialogsService,
        IConnectivityService connectivityService,
        INavigationService navigationService)
    {
        _busRepository = busRepository ?? throw new ArgumentNullException(nameof(busRepository));
        _userDialogsService = userDialogsService ?? throw new ArgumentNullException(nameof(userDialogsService));
        _connectivityService = connectivityService ?? throw new ArgumentNullException(nameof(connectivityService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
    }

    #endregion Constructors

    #region Command Methods

    [RelayCommand]
    private async Task ShowReverseTripStations()
    {
        using (_userDialogsService.Loading("Fetching Data... "))
        {
            await GetBusStationsAsync(
                isRefresh: false,
                shouldReverseWay: true);
        }
    }

    [RelayCommand]
    private async Task RefreshBusStations()
    {
        if (_connectivityService.IsInternetAvailable)
        {
            using (_userDialogsService.Loading("Fetching Data... "))
            {
                await GetBusStationsAsync(
                    isRefresh: true,
                    shouldReverseWay: false);
            }
        }
        else
        {
            _userDialogsService.Toast("No Internet connection detected");
        }

        IsBusy = false;
    }

    [RelayCommand]
    private async Task DownloadAllStationTimetables()
    {
        if (!_connectivityService.IsInternetAvailable)
        {
            _userDialogsService.Toast("Internet connection is necessary to download all bus stations time tables");

            return;
        }

        if (BusLine is null)
        {
            _userDialogsService.Toast("No Stations found for selected bus line");

            return;
        }

        using (_userDialogsService.Loading("Downloading Time Tables... "))
        {
            await _busRepository.DownloadAllStationsTimetablesAsync(
                BusLine.LinkNormalWay,
                BusLine.LinkReverseWay,
                BusLine.Id);
        }

        _userDialogsService.Toast("Download complete for all bus stations time tables");
    }

    #endregion

    #region Navigation Override Methods

    protected override async ValueTask OnPageInitialized()
    {
        using (_userDialogsService.Loading("Fetching Data... "))
        {
            await GetBusStationsAsync(
                isRefresh: false,
                shouldReverseWay: false);
        }
    }

    #endregion

    #region Methods

    private async Task GetBusStationsAsync(bool isRefresh, bool shouldReverseWay)
    {
        if (BusLine is null)
        {
            _userDialogsService.Toast("No Stations found for selected bus line");

            return;
        }

        // If there is a forced user refresh we want to keep the same Direction
        if (!isRefresh)
        {
            // Initial view of the stations list should be normal way
            if (!shouldReverseWay)
            {
                Direction = RouteDirections.Normal;

                _directionLink = BusLine.LinkNormalWay;
            }
            else if (shouldReverseWay && Direction == RouteDirections.Normal)
            {
                Direction = RouteDirections.Reverse;

                _directionLink = BusLine.LinkReverseWay;
            }
            else if (shouldReverseWay && Direction == RouteDirections.Reverse)
            {
                Direction = RouteDirections.Normal;

                _directionLink = BusLine.LinkNormalWay;
            }
        }

        try
        {
            var busStations = await _busRepository
                .GetBusStationsAsync(
                    _directionLink,
                    Direction,
                    BusLine.Id,
                    isRefresh);

            LastUpdated = busStations
                .FirstOrDefault()
                ?.LastUpdateDate
                ?? "never";

            BusStations.ReplaceRange(
                busStations.Select(
                    busStation => new BusStationsItemViewModel(
                        busStation,
                        _navigationService)));
        }
        catch (NoInternetException)
        {
            _userDialogsService.Toast("Internet connection is necessary to get bus stations",
                dismissTimer: TimeSpan.FromSeconds(5));
        }
        catch (Exception)
        {
            _userDialogsService.Toast("Something went wrong when getting bus stations");
        }
    }

    #endregion

    #region BusStationsItemViewModel Class

    public partial class BusStationsItemViewModel : BusViewModelBase
    {
        #region Fields

        private readonly BusStationModel _busStation;

        #endregion

        #region Properties

        public int Id { get; }
        public string Name { get; }
        public string ScheduleLink { get; }

        #endregion

        #region Constructors and Dependencies

        private readonly INavigationService _navigationService;

        public BusStationsItemViewModel(
            BusStationModel busStation,
            INavigationService navigationService)
        {
            _busStation = busStation ?? throw new ArgumentNullException(nameof(busStation));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            Id = _busStation.Id ?? 0;
            Name = _busStation.Name;
            ScheduleLink = _busStation.ScheduleLink;
        }

        #endregion

        #region Command Methods

        [RelayCommand]
        private async Task ShowTimetablesForSelectedBusStation()
        {
            var parameters = new Dictionary<string, object>
            {
                { AppNavigation.BusStation, _busStation }
            };

            await _navigationService.NavigateToAsync<BusTimetablesViewModel>(parameters);
        }

        #endregion
    }

    #endregion
}
