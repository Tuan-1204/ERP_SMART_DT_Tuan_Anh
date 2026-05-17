using System.Collections.ObjectModel;
using System.Windows.Input;
using ERP_SMART_DT_Tuan_Anh.Commands;
using ERP_SMART_DT_Tuan_Anh.Helpers;
using ERP_SMART_DT_Tuan_Anh.Models;
using ERP_SMART_DT_Tuan_Anh.Services;

namespace ERP_SMART_DT_Tuan_Anh.ViewModels;

public class RoleManagementViewModel : BaseViewModel
{
    private readonly RoleService _service = new();

    public ObservableCollection<Role> Roles { get; } = new();

    private Role _currentRole = new();

    public Role CurrentRole
    {
        get => _currentRole;
        set => SetProperty(ref _currentRole, value);
    }

    public ICommand NewCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand LoadCommand { get; }

    public RoleManagementViewModel()
    {
        NewCommand = new RelayCommand(_ => CurrentRole = new Role());
        SaveCommand = new AsyncRelayCommand(_ => SaveAsync());
        LoadCommand = new AsyncRelayCommand(_ => LoadAsync());

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        Roles.Clear();
        foreach (var item in await _service.GetAllAsync())
            Roles.Add(item);
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentRole.RoleName))
        {
            MessageBoxHelper.Warning("Vui lòng nhập tên vai trò.");
            return;
        }

        if (CurrentRole.Id == 0)
            await _service.AddAsync(CurrentRole);
        else
            await _service.UpdateAsync(CurrentRole);

        MessageBoxHelper.Info("Lưu vai trò thành công.");
        CurrentRole = new Role();
        await LoadAsync();
    }
}