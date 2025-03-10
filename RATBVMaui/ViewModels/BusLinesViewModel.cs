using System.Collections.ObjectModel;
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

public partial class BusLinesViewModel : BusViewModelBase
{
    #region Properties

    public override string Title =>
        OperatingSystem.IsWindows()
            ? $"Bus Lines - Updated on {LastUpdated}"
            : "Bus Lines";

    public RangeObservableCollection<BusLinesItemViewModel> BusLines =>
    [
        new(new BusLineModel
            {
                Name = "Bus 1",
                Route = "Route 1"
            },
            _navigationService),

        new(new BusLineModel
            {
                Name = "Bus 2",
                Route = "Route 2"
            },
            _navigationService)
    ];

    // Generate some fake data for the collection
    public RangeObservableCollection<BusLinesItemViewModel> MidiBusLines =>
    [
        new(new BusLineModel
            {
                Name = "MidiBus 1",
                Route = "Route 1"
            },
            _navigationService),

        new(new BusLineModel
            {
                Name = "MidiBus 2",
                Route = "Route 2"
            },
            _navigationService),
        new(new BusLineModel
            {
                Name = "MidiBus 3",
                Route = "Route 3"
            },
            _navigationService),
        new(new BusLineModel
            {
                Name = "MidiBus 4",
                Route = "Route 4"
            },
            _navigationService)
    ];

    public RangeObservableCollection<BusLinesItemViewModel> TrolleybusLines =>
    [
        new(new BusLineModel
            {
                Name = "Trolleybus 1",
                Route = "Route 1"
            },
            _navigationService),

        new(new BusLineModel
            {
                Name = "Trolleybus 2",
                Route = "Route 2"
            },
            _navigationService),
        new(new BusLineModel
            {
                Name = "Trolleybus 3",
                Route = "Route 3"
            },
            _navigationService),
        new(new BusLineModel
            {
                Name = "Trolleybus 4",
                Route = "Route 4"
            },
            _navigationService)
    ];

    [ObservableProperty]
    private ObservableCollection<BusLinesItemViewModel> _currentItems = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    private string _lastUpdated = "never";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private Color _testColor = Colors.Red;

    #endregion

    #region Constructor and Dependencies

    private readonly IBusDataService _busDataService;
    private readonly IBusRepository _busRepository;
    private readonly IUserDialogs _userDialogsService;
    private readonly IConnectivityService _connectivityService;
    private readonly INavigationService _navigationService;

    public BusLinesViewModel(IBusDataService busDataService,
        IBusRepository busRepository,
        IUserDialogs userDialogsService,
        IConnectivityService connectivityService,
        INavigationService navigationService)
    {
        _busDataService = busDataService ?? throw new ArgumentNullException(nameof(busDataService));
        _busRepository = busRepository ?? throw new ArgumentNullException(nameof(busRepository));
        _userDialogsService = userDialogsService ?? throw new ArgumentNullException(nameof(userDialogsService));
        _connectivityService = connectivityService ?? throw new ArgumentNullException(nameof(connectivityService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
    }

    #endregion

    #region Command Methods

    [RelayCommand]
    private async Task RefreshBusLines()
    {
        if (_connectivityService.IsInternetAvailable)
        {
            using (_userDialogsService.Loading("Fetching Data... "))
            {
                await GetBusLinesAsync(isForcedRefresh: true);
            }
        }
        else
        {
            _userDialogsService.Toast("No Internet connection detected");
        }

        IsBusy = false;
    }

    #endregion

    #region Navigation Override Methods

    protected override async ValueTask OnPageInitialized()
    {
        // TODO improve this and move the create tables in the Repository
        // ERROR Object not set to an instance of an object :|
        //using (_userDilaogsService.Loading("Fetching Data... "))
        //{
        // Create tables, if they already exist nothing will happen
        //await _busDataService.CreateAllTablesAsync();

        //await GetBusLinesAsync(isForcedRefresh: false);
        //}
    }

    #endregion

    #region Methods

    public void UpdateCurrentItems(string category)
    {
        CurrentItems = category switch
        {
            "Bus" => BusLines,
            "MidiBus" => MidiBusLines,
            "Trolleybus" => TrolleybusLines,
            _ => CurrentItems
        };

        TestColor = category switch
        {
            "Bus" => Colors.Blue,
            "MidiBus" => Colors.Orange,
            "Trolleybus" => Colors.Green,
            _ => Colors.Aquamarine
        };
    }

    private async Task GetBusLinesAsync(bool isForcedRefresh)
    {
        try
        {
            var busLines = await _busRepository.GetBusLinesAsync(isForcedRefresh);

            LastUpdated = busLines.FirstOrDefault()
                ?.LastUpdateDate
                ?? "never";

            GetBusLinesByType(busLines);
        }
        catch (NoInternetException)
        {
            _userDialogsService.Toast("Internet connection is necessary to get bus lines",
                dismissTimer: TimeSpan.FromSeconds(5));
        }
        catch (Exception)
        {
            _userDialogsService.Toast("Something went wrong when getting bus lines");
        }
    }

    private void GetBusLinesByType(List<BusLineModel> busLines)
    {
        BusLines
            .ReplaceRange(busLines.Where(bl => bl.Type == BusTypes.Bus.ToString())
            .Select(busLine => new BusLinesItemViewModel(
                busLine,
                _navigationService)));

        MidiBusLines
            .ReplaceRange(busLines.Where(bl => bl.Type == BusTypes.Midibus.ToString())
            .Select(busLine => new BusLinesItemViewModel(
                busLine,
                _navigationService)));

        TrolleybusLines
            .ReplaceRange(busLines.Where(bl => bl.Type == BusTypes.Trolleybus.ToString())
            .Select(busLine => new BusLinesItemViewModel(
                busLine,
                _navigationService)));
    }

    #endregion

    #region BusLinesItemViewModel Class

    public partial class BusLinesItemViewModel : BusViewModelBase
    {
        #region Fields

        private readonly BusLineModel _busLine;

        #endregion

        #region Properties

        public string Name { get; }
        public string Route { get; }

        #endregion

        #region Constructor and Dependencies

        private readonly INavigationService _navigationService;

        public BusLinesItemViewModel(
            BusLineModel busLine,
            INavigationService navigationService)
        {
            _busLine = busLine ?? throw new ArgumentNullException(nameof(busLine));
            //_navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            Name = _busLine.Name;
            Route = _busLine.Route;
        }

        #endregion

        #region Command Methods

        [RelayCommand]
        private async Task ShowStationsForSelectedBusLine()
        {
            var parameters = new Dictionary<string, object>
            {
                { AppNavigation.BusLine, _busLine }
            };

            //await _navigationService.Navigate($"{nameof(RATBVNavigation)}/{nameof(BusLines)}/{nameof(BusStations)}", parameters);
            await _navigationService.NavigateToAsync<BusStationsViewModel>(parameters);
        }

        #endregion
    }

    #endregion
}
