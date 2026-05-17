using System.Collections.ObjectModel;
using System.Windows.Input;
using ERP_SMART_DT_Tuan_Anh.Commands;
using ERP_SMART_DT_Tuan_Anh.Helpers;
using ERP_SMART_DT_Tuan_Anh.Models;
using ERP_SMART_DT_Tuan_Anh.Services;

namespace ERP_SMART_DT_Tuan_Anh.ViewModels;

public class WarrantyViewModel : BaseViewModel
{
    private readonly WarrantyService _service = new();

    public ObservableCollection<WarrantyLog> Warranties { get; } = new();

    private WarrantyLog _currentWarranty = new();

    public WarrantyLog CurrentWarranty
    {
        get => _currentWarranty;
        set => SetProperty(ref _currentWarranty, value);
    }

    public ICommand NewCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand LoadCommand { get; }

    public WarrantyViewModel()
    {
        NewCommand = new RelayCommand(_ => CurrentWarranty = new WarrantyLog { Status = "Đang xử lý" });
        SaveCommand = new AsyncRelayCommand(_ => SaveAsync());
        LoadCommand = new AsyncRelayCommand(_ => LoadAsync());
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        Warranties.Clear();
        foreach (var item in await _service.GetAllAsync())
            Warranties.Add(item);
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentWarranty.Imei))
        {
            MessageBoxHelper.Warning("Vui lòng nhập IMEI.");
            return;
        }

        CurrentWarranty.UserId = 1;

        if (CurrentWarranty.Id == 0)
            await _service.AddAsync(CurrentWarranty);
        else
            await _service.UpdateAsync(CurrentWarranty);

        MessageBoxHelper.Info("Lưu phiếu bảo hành thành công.");
        CurrentWarranty = new WarrantyLog { Status = "Đang xử lý" };
        await LoadAsync();
    }
}