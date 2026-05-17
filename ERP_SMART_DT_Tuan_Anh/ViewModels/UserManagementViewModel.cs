using System.Collections.ObjectModel;
using System.Windows.Input;
using ERP_SMART_DT_Tuan_Anh.Commands;
using ERP_SMART_DT_Tuan_Anh.Helpers;
using ERP_SMART_DT_Tuan_Anh.Models;
using ERP_SMART_DT_Tuan_Anh.Services;

namespace ERP_SMART_DT_Tuan_Anh.ViewModels;

public class UserManagementViewModel : BaseViewModel
{
    private readonly UserService _userService = new();
    private readonly RoleService _roleService = new();

    public ObservableCollection<User> Users { get; } = new();
    public ObservableCollection<Role> Roles { get; } = new();

    private User _currentUser = new() { IsActive = true };
    private string _rawPassword = string.Empty;

    public User CurrentUser
    {
        get => _currentUser;
        set => SetProperty(ref _currentUser, value);
    }

    public string RawPassword
    {
        get => _rawPassword;
        set => SetProperty(ref _rawPassword, value);
    }

    public ICommand NewCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand LoadCommand { get; }

    public UserManagementViewModel()
    {
        NewCommand = new RelayCommand(_ =>
        {
            CurrentUser = new User { IsActive = true };
            RawPassword = string.Empty;
        });

        SaveCommand = new AsyncRelayCommand(_ => SaveAsync());
        DeleteCommand = new AsyncRelayCommand(_ => DeleteAsync());
        LoadCommand = new AsyncRelayCommand(_ => LoadAsync());

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        Users.Clear();
        Roles.Clear();

        foreach (var item in await _userService.GetAllAsync())
            Users.Add(item);

        foreach (var item in await _roleService.GetAllAsync())
            Roles.Add(item);
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentUser.Username))
        {
            MessageBoxHelper.Warning("Vui lòng nhập tên đăng nhập.");
            return;
        }

        if (CurrentUser.Id == 0)
        {
            if (string.IsNullOrWhiteSpace(RawPassword))
            {
                MessageBoxHelper.Warning("Vui lòng nhập mật khẩu.");
                return;
            }

            await _userService.AddAsync(CurrentUser, RawPassword);
        }
        else
        {
            await _userService.UpdateAsync(CurrentUser);
        }

        MessageBoxHelper.Info("Lưu người dùng thành công.");
        CurrentUser = new User { IsActive = true };
        RawPassword = string.Empty;
        await LoadAsync();
    }

    private async Task DeleteAsync()
    {
        if (CurrentUser.Id == 0)
            return;

        if (!MessageBoxHelper.Confirm("Bạn có chắc muốn xóa mềm người dùng này?"))
            return;

        await _userService.SoftDeleteAsync(CurrentUser.Id);
        CurrentUser = new User { IsActive = true };
        await LoadAsync();
    }
}