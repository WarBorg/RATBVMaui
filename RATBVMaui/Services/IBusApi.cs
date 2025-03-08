using RATBVData.Models.Models;
using Refit;

namespace RATBVMaui.Services;

public interface IBusApi
{
    [Get("/buslines")]
    Task<List<BusLineModel>> GetBusLines();

    [Get("/buslines/{lineNumber}")]
    Task<BusLineModel> GetBusLine(string lineNumber);

    [Get("/busstations/{lineNumberLink}")]
    Task<List<BusStationModel>> GetBusStations(string lineNumberLink);

    [Get("/bustimetables/{scheduleLink}")]
    Task<List<BusTimeTableModel>> GetBusTimeTables(string scheduleLink);
}
