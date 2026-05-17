using System.Collections.ObjectModel;
using System.Windows.Input;
using ERP_SMART_DT_Tuan_Anh.Commands;
using ERP_SMART_DT_Tuan_Anh.Enums;
using ERP_SMART_DT_Tuan_Anh.Helpers;
using ERP_SMART_DT_Tuan_Anh.Models;
using ERP_SMART_DT_Tuan_Anh.Services;

namespace ERP_SMART_DT_Tuan_Anh.ViewModels;

public class CustomerViewModel : BaseViewModel
{
    private readonly PartnerService _partnerService = new();

    public ObservableCollection<Partner> Customers { get; } = new();

    private Partner _currentCustomer = new() { ObjectType = PartnerTypeValues.Customer };

    public Partner CurrentCustomer
    {
        get => _currentCustomer;
        set => SetProperty(ref _currentCustomer, value);
    }

    public ICommand LoadCommand { get; }
    public ICommand NewCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }

    public CustomerViewModel()
    {
        LoadCommand = new AsyncRelayCommand(_ => LoadAsync());
        NewCommand = new RelayCommand(_ => CurrentCustomer = new Partner { ObjectType = PartnerTypeValues.Customer });
        SaveCommand = new AsyncRelayCommand(_ => SaveAsync());
        DeleteCommand = new AsyncRelayCommand(_ => DeleteAsync());

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        Customers.Clear();

        foreach (var item in await _partnerService.GetCustomersAsync())
            Customers.Add(item);
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentCustomer.FullName))
        {
            MessageBoxHelper.Warning("Vui lòng nhập tên khách hàng.");
            return;
        }

        CurrentCustomer.ObjectType = PartnerTypeValues.Customer;

        if (CurrentCustomer.Id == 0)
            await _partnerService.AddAsync(CurrentCustomer);
        else
            await _partnerService.UpdateAsync(CurrentCustomer);

        MessageBoxHelper.Info("Lưu khách hàng thành công.");
        CurrentCustomer = new Partner { ObjectType = PartnerTypeValues.Customer };
        await LoadAsync();
    }

    private async Task DeleteAsync()
    {
        if (CurrentCustomer.Id == 0)
            return;

        if (!MessageBoxHelper.Confirm("Bạn có chắc muốn xóa mềm khách hàng này?"))
            return;

        await _partnerService.SoftDeleteAsync(CurrentCustomer.Id);
        CurrentCustomer = new Partner { ObjectType = PartnerTypeValues.Customer };
        await LoadAsync();
    }
}