using System.Collections.ObjectModel;
using System.Windows.Input;
using ERP_SMART_DT_Tuan_Anh.Commands;
using ERP_SMART_DT_Tuan_Anh.Models;
using ERP_SMART_DT_Tuan_Anh.Services;

namespace ERP_SMART_DT_Tuan_Anh.ViewModels;

public class AuditLogViewModel : BaseViewModel
{
    private readonly AuditLogService _service = new();

    public ObservableCollection<AuditLog> Logs { get; } = new();

    public ICommand LoadCommand { get; }

    public AuditLogViewModel()
    {
        LoadCommand = new AsyncRelayCommand(_ => LoadAsync());
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        Logs.Clear();
        foreach (var item in await _service.GetAllAsync())
            Logs.Add(item);
    }
}