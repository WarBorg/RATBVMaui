using System.Collections;
using RATBVData.Models.Models;

namespace RATBVMaui.Services;

public class BusDataService : IBusDataService
{
    #region Fields

    private readonly ISQLiteAsyncConnection _asyncConnection;

    #endregion

    #region Constructors

    public BusDataService(ISQLiteAsyncConnection asyncConnection)
    {
        _asyncConnection = asyncConnection ?? throw new ArgumentNullException(nameof(asyncConnection));
    }

    #endregion

    #region Universal Methods

    public async Task CreateAllTablesAsync()
    {
        await _asyncConnection.CreateTableAsync<BusLineModel>();
        await _asyncConnection.CreateTableAsync<BusStationModel>();
        await _asyncConnection.CreateTableAsync<BusTimeTableModel>();
    }

    public async Task DropAllTablesAsync()
    {
        await _asyncConnection.DropTableAsync<BusLineModel>();
        await _asyncConnection.DropTableAsync<BusStationModel>();
        await _asyncConnection.DropTableAsync<BusTimeTableModel>();
    }

    public async Task DeleteAllTablesAsync()
    {
        await DeleteAllAsync<BusLineModel>();
        await DeleteAllAsync<BusStationModel>();
        await DeleteAllAsync<BusTimeTableModel>();
    }

    #endregion

    #region Bus Lines Methods

    public async Task<int> CountBusLinesAsync() =>
        await _asyncConnection.Table<BusLineModel>()
            .CountAsync();

    public async Task<List<BusLineModel>> GetBusLinesAsync() =>
        await _asyncConnection
            .Table<BusLineModel>()
            .OrderBy(busLineTable => busLineTable.Id)
            .ToListAsync();

    public async Task<int> InsertOrReplaceBusLinesAsync(IEnumerable<BusLineModel> busLines) =>
        await InsertOrReplaceAllAsync(busLines);

    #endregion

    #region Bus Stations Methods

    public async Task<int> CountBusStationsByBusLineIdAndDirectionAsync(
        int busLineId,
        string direction) =>
        await _asyncConnection
            .Table<BusStationModel>()
            .Where(busStationTable => busStationTable.BusLineId == busLineId &&
                                      busStationTable.Direction == direction)
            .CountAsync();

    public async Task<List<BusStationModel>> GetBusStationsByBusLineIdAndDirectionAsync(
        int busId,
        string direction) =>
        await _asyncConnection
            .Table<BusStationModel>()
            .Where(busStationTable => busStationTable.BusLineId == busId &&
                                      busStationTable.Direction == direction)
            .OrderBy(busStationTable => busStationTable.Id)
            .ToListAsync();

    public async Task<int> InsertOrReplaceBusStationsAsync(IEnumerable<BusStationModel> busStations)
    {
        var busLineId = busStations.FirstOrDefault()?.BusLineId ?? 0;
        var busDirection = busStations.FirstOrDefault()?.Direction ?? string.Empty;

        var storedBusStations = await GetBusStationsByBusLineIdAndDirectionAsync(busLineId,
            busDirection);

        if (storedBusStations.Count <= 0)
        {
            return await InsertOrReplaceAllAsync(busStations);
        }

        foreach (var station in busStations)
        {
            var existingStation = storedBusStations.FirstOrDefault(b => b.Name == station.Name);

            if (existingStation != null)
            {
                station.Id = existingStation.Id;
            }
        }

        return await InsertOrReplaceAllAsync(busStations);
    }

    #endregion

    #region Bus Time Table Methods

    public async Task<int> CountBusTimeTableByBusStationIdAsync(int busStationId) =>
        await _asyncConnection
            .Table<BusTimeTableModel>()
            .Where(busStationTable => busStationTable.BusStationId == busStationId)
            .CountAsync();

    public async Task<List<BusTimeTableModel>> GetBusTimeTableByBusStationId(int busStationId) =>
        await _asyncConnection
            .Table<BusTimeTableModel>()
            .Where(busTimeTable => busTimeTable.BusStationId == busStationId)
            .OrderBy(busTimeTable => busTimeTable.Id)
            .ToListAsync();

    public async Task<int> InsertOrReplaceBusTimeTablesAsync(IEnumerable<BusTimeTableModel> busTimeTables)
    {
        var busStationId = busTimeTables.FirstOrDefault()?.BusStationId ?? 0;

        var storedBusTimeTables = await GetBusTimeTableByBusStationId(busStationId);

        if (storedBusTimeTables.Count <= 0)
        {
            return await InsertOrReplaceAllAsync(busTimeTables);
        }

        foreach (var timeTable in busTimeTables)
        {
            var existingTimeTable = storedBusTimeTables.FirstOrDefault(b => b.BusStationId == timeTable.BusStationId
                                                                            && b.Hour == timeTable.Hour
                                                                            && b.TimeOfWeek == timeTable.TimeOfWeek);

            if (existingTimeTable is not null)
            {
                timeTable.Id = existingTimeTable.Id;
            }
        }

        return await InsertOrReplaceAllAsync(busTimeTables);
    }

    #endregion

    #region Insert / Delete Methods

    private async Task<int> InsertOrReplaceAllAsync(IEnumerable items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var returnItems = 0;

        foreach (var item in items)
        {
            await _asyncConnection.InsertOrReplaceAsync(item);

            returnItems++;
        }

        return returnItems;
    }

    private Task<int> DeleteAllAsync<T>(CancellationToken cancellationToken = default) =>
        Task.Factory.StartNew(() =>
            {
                var conn = _asyncConnection.GetConnection();

                using (conn?.Lock())
                {
                    return conn?.DeleteAll<T>() ?? 0;
                }
            },
            cancellationToken);

    #endregion

    #region NOT USED METHODS

    #region Bus Lines Methods

    private async Task<List<BusLineModel>> GetBusLinesByNameAsync(string? nameFilter = null)
    {
        nameFilter ??= string.Empty;

        return await _asyncConnection
            .Table<BusLineModel>()
            .Where(busLineTable => busLineTable.Name.Contains(nameFilter))
            .OrderBy(busLineTable => busLineTable.Id)
            .ToListAsync();
    }

    private async Task<BusLineModel> GetBusLineByIdAsync(int id) =>
        await _asyncConnection.GetAsync<BusLineModel>(id);

    private async Task<int> InsertBusLineAsync(BusLineModel busLine) =>
        await _asyncConnection.InsertAsync(busLine);

    private async Task<int> UpdateBusLineAsync(BusLineModel busLine) =>
        await _asyncConnection.UpdateAsync(busLine);

    private async Task<int> DeleteBusLineAsync(BusLineModel busLine) =>
        await _asyncConnection.DeleteAsync(busLine);

    #endregion

    #region Bus Stations Methods

    private async Task<List<BusStationModel>> GetBusStationsByNameAsync(
        int busId,
        string direction,
        string? nameFilter = null)
    {
        nameFilter ??= string.Empty;

        return await (from busStationTable in _asyncConnection.Table<BusStationModel>()
            where busStationTable.BusLineId == busId
                  && busStationTable.Direction == direction
                  && busStationTable.Name
                      .Contains(nameFilter)
            orderby busStationTable.Id
            select busStationTable).ToListAsync();
    }

    private async Task<BusStationModel> GetBusStationByIdAsync(int id) =>
        await _asyncConnection.GetAsync<BusStationModel>(id);

    private async Task<int> InsertBusStationAsync(BusStationModel busStation) =>
        await _asyncConnection.InsertAsync(busStation);

    private async Task<int> UpdateBusStationAsync(BusStationModel busStation) =>
        await _asyncConnection.UpdateAsync(busStation);

    private async Task<int> DeleteBusStationAsync(BusStationModel busStation) =>
        await _asyncConnection.DeleteAsync(busStation);

    #endregion

    #region Bus Time Table Methods

    private async Task<BusTimeTableModel> GetBusTimeTableById(int id) =>
        await _asyncConnection.GetAsync<BusTimeTableModel>(id);

    private async Task<int> InsertBusTimeTableAsync(BusTimeTableModel busTimeTable) =>
        await _asyncConnection.InsertAsync(busTimeTable);

    private async Task<int> UpdateBusTimeTableAsync(BusTimeTableModel busTimeTable) =>
        await _asyncConnection.UpdateAsync(busTimeTable);

    private async Task<int> DeleteBusTimeTableAsync(BusTimeTableModel busTimeTable) =>
        await _asyncConnection.DeleteAsync(busTimeTable);

    #endregion

    #endregion
}
