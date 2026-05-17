using System.Windows.Input;
using ERP_SMART_DT_Tuan_Anh.Commands;
using ERP_SMART_DT_Tuan_Anh.Helpers;
using ERP_SMART_DT_Tuan_Anh.Models;
using ERP_SMART_DT_Tuan_Anh.Services;

namespace ERP_SMART_DT_Tuan_Anh.ViewModels;

public class SettingViewModel : BaseViewModel
{
    private readonly SettingService _service = new();

    private Setting _setting = new();

    public Setting Setting
    {
        get => _setting;
        set => SetProperty(ref _setting, value);
    }

    public ICommand SaveCommand { get; }
    public ICommand LoadCommand { get; }

    public SettingViewModel()
    {
        SaveCommand = new AsyncRelayCommand(_ => SaveAsync());
        LoadCommand = new AsyncRelayCommand(_ => LoadAsync());

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        Setting = await _service.GetAsync() ?? new Setting
        {
            ShopName = "ERP SMART DT Tuấn Anh",
            Theme = "Light"
        };
    }

    private async Task SaveAsync()
    {
        await _service.SaveAsync(Setting);
        MessageBoxHelper.Info("Lưu cấu hình thành công.");
    }
}