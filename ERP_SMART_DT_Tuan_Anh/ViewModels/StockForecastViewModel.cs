using System.Collections.ObjectModel;
using System.Windows.Input;
using ERP_SMART_DT_Tuan_Anh.Commands;
using ERP_SMART_DT_Tuan_Anh.DTOs;
using ERP_SMART_DT_Tuan_Anh.Services;

namespace ERP_SMART_DT_Tuan_Anh.ViewModels;

public class StockForecastViewModel : BaseViewModel
{
    private readonly StockForecastService _service = new();

    public ObservableCollection<StockForecastResultDto> Results { get; } = new();

    public int AnalysisDays { get; set; } = 30;
    public int LeadTimeDays { get; set; } = 5;
    public int SafetyDays { get; set; } = 3;

    public ICommand ForecastCommand { get; }

    public StockForecastViewModel()
    {
        ForecastCommand = new AsyncRelayCommand(_ => ForecastAsync());
        _ = ForecastAsync();
    }

    private async Task ForecastAsync()
    {
        Results.Clear();

        var data = await _service.ForecastAsync(new StockForecastRequestDto
        {
            AnalysisDays = AnalysisDays,
            LeadTimeDays = LeadTimeDays,
            SafetyDays = SafetyDays
        });

        foreach (var item in data)
            Results.Add(item);
    }
}