using System.Collections.ObjectModel;
using System.Windows.Input;
using ERP_SMART_DT_Tuan_Anh.Commands;
using ERP_SMART_DT_Tuan_Anh.Helpers;
using ERP_SMART_DT_Tuan_Anh.Models;
using ERP_SMART_DT_Tuan_Anh.Services;
using ReturnModel = ERP_SMART_DT_Tuan_Anh.Models.Return;

namespace ERP_SMART_DT_Tuan_Anh.ViewModels;

public class ReturnViewModel : BaseViewModel
{
    private readonly ReturnService _returnService = new();
    private readonly BillService _billService = new();

    public ObservableCollection<ReturnModel> Returns { get; } = new();
    public ObservableCollection<Bill> Bills { get; } = new();

    private ReturnModel _currentReturn = new() { ReturnType = "CUSTOMER_RETURN" };

    public ReturnModel CurrentReturn
    {
        get => _currentReturn;
        set => SetProperty(ref _currentReturn, value);
    }

    public ICommand NewCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand LoadCommand { get; }

    public ReturnViewModel()
    {
        NewCommand = new RelayCommand(_ => CurrentReturn = new ReturnModel { ReturnType = "CUSTOMER_RETURN" });
        SaveCommand = new AsyncRelayCommand(_ => SaveAsync());
        LoadCommand = new AsyncRelayCommand(_ => LoadAsync());
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        Returns.Clear();
        Bills.Clear();

        foreach (var item in await _returnService.GetAllAsync())
            Returns.Add(item);

        foreach (var item in await _billService.GetAllAsync())
            Bills.Add(item);
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentReturn.BillId))
        {
            MessageBoxHelper.Warning("Vui lòng chọn hoặc nhập mã hóa đơn.");
            return;
        }

        await _returnService.AddAsync(CurrentReturn);
        MessageBoxHelper.Info("Tạo phiếu trả hàng thành công.");

        CurrentReturn = new ReturnModel { ReturnType = "CUSTOMER_RETURN" };
        await LoadAsync();
    }
}