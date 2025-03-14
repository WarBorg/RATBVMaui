﻿using RATBVData.Models.Models;

namespace RATBVMaui.Services;

public interface IBusRepository
{
    Task<List<BusLineModel>> GetBusLinesAsync(bool isForcedRefresh);

    Task<List<BusStationModel>> GetBusStationsAsync(string directionLink,
        string direction,
        int busLineId,
        bool isForcedRefresh);

    Task<List<BusTimeTableModel>> GetBusTimetablesAsync(string schedualLink,
        int busStationId,
        bool isForcedRefresh);

    Task DownloadAllStationsTimetablesAsync(string normalDirectionLink,
        string reverseDirectionLink,
        int busLineId);
}