using System.Collections.ObjectModel;
using System.Windows.Input;
using ERP_SMART_DT_Tuan_Anh.Commands;
using ERP_SMART_DT_Tuan_Anh.Models;
using ERP_SMART_DT_Tuan_Anh.Services;

namespace ERP_SMART_DT_Tuan_Anh.ViewModels;

public class ImeiInventoryViewModel : BaseViewModel
{
    private readonly ImeiService _imeiService = new();

    public ObservableCollection<ImeiInventory> ImeiList { get; } = new();

    private string _keyword = string.Empty;
    private ImeiInventory? _selectedImei;

    public string Keyword
    {
        get => _keyword;
        set => SetProperty(ref _keyword, value);
    }

    public ImeiInventory? SelectedImei
    {
        get => _selectedImei;
        set => SetProperty(ref _selectedImei, value);
    }

    public ICommand LoadCommand { get; }
    public ICommand SearchCommand { get; }

    public ImeiInventoryViewModel()
    {
        LoadCommand = new AsyncRelayCommand(_ => LoadAsync());
        SearchCommand = new AsyncRelayCommand(_ => SearchAsync());
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        ImeiList.Clear();

        foreach (var item in await _imeiService.GetAllAsync())
            ImeiList.Add(item);
    }

    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(Keyword))
        {
            await LoadAsync();
            return;
        }

        ImeiList.Clear();

        var item = await _imeiService.GetByImeiAsync(Keyword.Trim());
        if (item != null)
            ImeiList.Add(item);
    }
}