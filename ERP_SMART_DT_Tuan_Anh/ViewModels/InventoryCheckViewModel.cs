using System.Collections.ObjectModel;
using System.Windows.Input;
using ERP_SMART_DT_Tuan_Anh.Commands;
using ERP_SMART_DT_Tuan_Anh.Helpers;
using ERP_SMART_DT_Tuan_Anh.Models;
using ERP_SMART_DT_Tuan_Anh.Services;

namespace ERP_SMART_DT_Tuan_Anh.ViewModels;

public class InventoryCheckViewModel : BaseViewModel
{
    private readonly InventoryCheckService _service = new();

    public ObservableCollection<InventoryCheck> Checks { get; } = new();

    private string _note = string.Empty;

    public string Note
    {
        get => _note;
        set => SetProperty(ref _note, value);
    }

    public ICommand CreateCommand { get; }
    public ICommand LoadCommand { get; }

    public InventoryCheckViewModel()
    {
        CreateCommand = new AsyncRelayCommand(_ => CreateAsync());
        LoadCommand = new AsyncRelayCommand(_ => LoadAsync());
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        Checks.Clear();
        foreach (var item in await _service.GetAllAsync())
            Checks.Add(item);
    }

    private async Task CreateAsync()
    {
        await _service.AddAsync(new InventoryCheck
        {
            UserId = 1,
            Note = Note
        });

        MessageBoxHelper.Info("Tạo phiếu kiểm kê thành công.");
        Note = string.Empty;
        await LoadAsync();
    }
}