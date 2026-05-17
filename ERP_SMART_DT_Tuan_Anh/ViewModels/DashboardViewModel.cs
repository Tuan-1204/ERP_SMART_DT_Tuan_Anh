using ERP_SMART_DT_Tuan_Anh.DTOs;
using ERP_SMART_DT_Tuan_Anh.Services;

namespace ERP_SMART_DT_Tuan_Anh.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    private readonly DashboardService _dashboardService = new();

    private DashboardSummaryDto _summary = new();

    public DashboardSummaryDto Summary
    {
        get => _summary;
        set => SetProperty(ref _summary, value);
    }

    public DashboardViewModel()
    {
        _ = LoadAsync();
    }

    public async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            Summary = await _dashboardService.GetSummaryAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}