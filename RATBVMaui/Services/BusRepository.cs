using RATBVData.Models.Models;
using RATBVMaui.Constants;
using RATBVMaui.Exceptions;

namespace RATBVMaui.Services;

public class BusRepository : IBusRepository
{
    #region Constructors and Dependencies

    private readonly IBusApi _busApi;
    private readonly IBusDataService _busDataService;
    private readonly IConnectivityService _connectivityService;

    public BusRepository(IBusApi busApi,
        IBusDataService busDataService,
        IConnectivityService connectivityService)
    {
        _busApi = busApi ?? throw new ArgumentNullException(nameof(busApi));
        _busDataService = busDataService ?? throw new ArgumentNullException(nameof(busDataService));
        _connectivityService = connectivityService ?? throw new ArgumentNullException(nameof(connectivityService));
    }

    #endregion

    #region IBusWebService Methods

    public async Task<List<BusLineModel>> GetBusLinesAsync(bool isForcedRefresh)
    {
        var busLinesCount = await _busDataService.CountBusLinesAsync();

        if (!isForcedRefresh && busLinesCount is not 0)
        {
            return await _busDataService.GetBusLinesAsync();
        }

        if (!_connectivityService.IsInternetAvailable)
        {
            throw new NoInternetException();
        }

        var busLines = await _busApi.GetBusLines();

        var lastUpdated = $"{DateTime.Now.Date:d} {DateTime.Now:HH:mm}";

        await InsertBusLinesInDatabaseAsync(busLines, lastUpdated);

        return await _busDataService.GetBusLinesAsync();
    }

    public async Task<List<BusStationModel>> GetBusStationsAsync(
        string directionLink,
        string direction,
        int busLineId,
        bool isForcedRefresh)
    {
        var busStationsCount = await _busDataService.CountBusStationsByBusLineIdAndDirectionAsync(
            busLineId,
            direction);

        if (!isForcedRefresh && busStationsCount is not 0)
        {
            return await _busDataService.GetBusStationsByBusLineIdAndDirectionAsync(busLineId,
                direction);
        }

        if (!_connectivityService.IsInternetAvailable)
        {
            throw new NoInternetException();
        }

        var busStations = await _busApi.GetBusStations(directionLink);

        var lastUpdated = $"{DateTime.Now.Date:d} {DateTime.Now:HH:mm}";

        await InsertBusStationsInDatabaseAsync(busStations,
            busLineId,
            lastUpdated,
            direction);

        return await _busDataService.GetBusStationsByBusLineIdAndDirectionAsync(busLineId,
            direction);
    }

    public async Task<List<BusTimeTableModel>> GetBusTimetablesAsync(
        string scheduleLink,
        int busStationId,
        bool isForcedRefresh)
    {
        var busTimeTableCount = await _busDataService.CountBusTimeTableByBusStationIdAsync(busStationId);

        if (!isForcedRefresh && busTimeTableCount != 0)
        {
            return await _busDataService.GetBusTimeTableByBusStationId(busStationId);
        }

        if (!_connectivityService.IsInternetAvailable)
        {
            throw new NoInternetException();
        }

        var busTimetables = await _busApi.GetBusTimeTables(scheduleLink);

        var lastUpdated = $"{DateTime.Now.Date:d} {DateTime.Now:HH:mm}";

        await InsertBusTimetablesInDatabaseAsync(
            busTimetables,
            busStationId,
            lastUpdated);

        return await _busDataService.GetBusTimeTableByBusStationId(busStationId);
    }

    public async Task DownloadAllStationsTimetablesAsync(
        string normalDirectionLink,
        string reverseDirectionLink,
        int busLineId)
    {
        var lastUpdated = $"{DateTime.Now.Date:d} {DateTime.Now:HH:mm}";

        // Get bus stations for normal direction
        var busStationsNormalDirection = await _busApi.GetBusStations(normalDirectionLink);

        busStationsNormalDirection.ForEach(b => b.Direction = RouteDirections.Normal);

        // Get bus stations for reverse direction
        var busStationsReverseDirection = await _busApi.GetBusStations(reverseDirectionLink);

        busStationsReverseDirection.ForEach(b => b.Direction = RouteDirections.Reverse);

        // Concatenate bus stations for both directions
        var busStations = busStationsNormalDirection
            .Concat(busStationsReverseDirection)
            .ToList();

        await InsertBusStationsInDatabaseAsync(
            busStations,
            busLineId,
            lastUpdated);

        foreach (var busStation in busStations)
        {
            var busTimetables = await _busApi.GetBusTimeTables(busStation.ScheduleLink);

            await InsertBusTimetablesInDatabaseAsync(busTimetables,
                busStation.Id ?? 0,
                lastUpdated);
        }
    }

    #endregion

    #region Methods

    private async Task InsertBusLinesInDatabaseAsync(
        List<BusLineModel> busLines,
        string lastUpdated)
    {
        foreach (var busLine in busLines)
        {
            busLine.LastUpdateDate = lastUpdated;
        }

        await _busDataService.InsertOrReplaceBusLinesAsync(busLines);
    }

    private async Task InsertBusStationsInDatabaseAsync(
        List<BusStationModel> busStations,
        int busLineId,
        string lastUpdated,
        string? direction = null)
    {
        foreach (var busStation in busStations)
        {
            // Add foreign key and direction before inserting in database
            busStation.BusLineId = busLineId;
            busStation.LastUpdateDate = lastUpdated;

            if (direction is not null)
            {
                busStation.Direction = direction;
            }
        }

        await _busDataService.InsertOrReplaceBusStationsAsync(busStations);
    }

    private async Task InsertBusTimetablesInDatabaseAsync(
        List<BusTimeTableModel> busTimeTables,
        int busStationId,
        string lastUpdated)
    {
        foreach (var busTimetableHour in busTimeTables)
        {
            // Add foreign key before inserting in database
            busTimetableHour.BusStationId = busStationId;
            busTimetableHour.LastUpdateDate = lastUpdated;
        }

        await _busDataService.InsertOrReplaceBusTimeTablesAsync(busTimeTables);
    }

    #endregion
}
