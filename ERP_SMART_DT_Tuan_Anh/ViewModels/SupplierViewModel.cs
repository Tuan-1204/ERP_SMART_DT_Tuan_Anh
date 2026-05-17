using System.Collections.ObjectModel;
using System.Windows.Input;
using ERP_SMART_DT_Tuan_Anh.Commands;
using ERP_SMART_DT_Tuan_Anh.Enums;
using ERP_SMART_DT_Tuan_Anh.Helpers;
using ERP_SMART_DT_Tuan_Anh.Models;
using ERP_SMART_DT_Tuan_Anh.Services;

namespace ERP_SMART_DT_Tuan_Anh.ViewModels;

public class SupplierViewModel : BaseViewModel
{
    private readonly PartnerService _partnerService = new();

    public ObservableCollection<Partner> Suppliers { get; } = new();

    private Partner _currentSupplier = new() { ObjectType = PartnerTypeValues.Supplier };

    public Partner CurrentSupplier
    {
        get => _currentSupplier;
        set => SetProperty(ref _currentSupplier, value);
    }

    public ICommand LoadCommand { get; }
    public ICommand NewCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }

    public SupplierViewModel()
    {
        LoadCommand = new AsyncRelayCommand(_ => LoadAsync());
        NewCommand = new RelayCommand(_ => CurrentSupplier = new Partner { ObjectType = PartnerTypeValues.Supplier });
        SaveCommand = new AsyncRelayCommand(_ => SaveAsync());
        DeleteCommand = new AsyncRelayCommand(_ => DeleteAsync());

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        Suppliers.Clear();

        foreach (var item in await _partnerService.GetSuppliersAsync())
            Suppliers.Add(item);
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentSupplier.FullName))
        {
            MessageBoxHelper.Warning("Vui lòng nhập tên nhà cung cấp.");
            return;
        }

        CurrentSupplier.ObjectType = PartnerTypeValues.Supplier;

        if (CurrentSupplier.Id == 0)
            await _partnerService.AddAsync(CurrentSupplier);
        else
            await _partnerService.UpdateAsync(CurrentSupplier);

        MessageBoxHelper.Info("Lưu nhà cung cấp thành công.");
        CurrentSupplier = new Partner { ObjectType = PartnerTypeValues.Supplier };
        await LoadAsync();
    }

    private async Task DeleteAsync()
    {
        if (CurrentSupplier.Id == 0)
            return;

        if (!MessageBoxHelper.Confirm("Bạn có chắc muốn xóa mềm nhà cung cấp này?"))
            return;

        await _partnerService.SoftDeleteAsync(CurrentSupplier.Id);
        CurrentSupplier = new Partner { ObjectType = PartnerTypeValues.Supplier };
        await LoadAsync();
    }
}