using System.Collections.ObjectModel;
using System.Windows.Input;
using ERP_SMART_DT_Tuan_Anh.Commands;
using ERP_SMART_DT_Tuan_Anh.DTOs;
using ERP_SMART_DT_Tuan_Anh.Helpers;
using ERP_SMART_DT_Tuan_Anh.Models;
using ERP_SMART_DT_Tuan_Anh.Services;

namespace ERP_SMART_DT_Tuan_Anh.ViewModels;

public class ExportStockViewModel : BaseViewModel
{
    private readonly StockService _stockService = new();
    private readonly ProductService _productService = new();
    private readonly PartnerService _partnerService = new();

    public ObservableCollection<Product> Products { get; } = new();
    public ObservableCollection<Partner> Customers { get; } = new();

    private Product? _selectedProduct;
    private Partner? _selectedCustomer;
    private decimal _unitPrice;
    private decimal _paidAmount;
    private string _imeiText = string.Empty;

    public Product? SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            if (SetProperty(ref _selectedProduct, value) && value != null)
            {
                UnitPrice = value.ExportPrice;
            }
        }
    }

    public Partner? SelectedCustomer
    {
        get => _selectedCustomer;
        set => SetProperty(ref _selectedCustomer, value);
    }

    public decimal UnitPrice
    {
        get => _unitPrice;
        set => SetProperty(ref _unitPrice, value);
    }

    public decimal PaidAmount
    {
        get => _paidAmount;
        set => SetProperty(ref _paidAmount, value);
    }

    public string ImeiText
    {
        get => _imeiText;
        set => SetProperty(ref _imeiText, value);
    }

    public ICommand ExportCommand { get; }
    public ICommand RefreshCommand { get; }

    public ExportStockViewModel()
    {
        ExportCommand = new AsyncRelayCommand(_ => ExportAsync());
        RefreshCommand = new AsyncRelayCommand(_ => LoadAsync());
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        Products.Clear();
        Customers.Clear();

        foreach (var item in await _productService.GetAllAsync())
            Products.Add(item);

        foreach (var item in await _partnerService.GetCustomersAsync())
            Customers.Add(item);
    }

    private async Task ExportAsync()
    {
        if (SelectedProduct == null || SelectedCustomer == null)
        {
            MessageBoxHelper.Warning("Vui lòng chọn sản phẩm và khách hàng.");
            return;
        }

        var imeis = ImeiHelper.ParseImeiList(ImeiText);
        if (imeis.Count == 0)
        {
            MessageBoxHelper.Warning("Vui lòng nhập danh sách IMEI cần xuất.");
            return;
        }

        var totalAmount = UnitPrice * imeis.Count;

        var request = new ExportStockRequestDto
        {
            BillId = BillCodeGenerator.GenerateExportCode(DateTime.Now.Second + 1),
            ObjectId = SelectedCustomer.Id,
            UserId = 1,
            ProductId = SelectedProduct.Id,
            UnitPrice = UnitPrice,
            TotalAmount = totalAmount,
            PaidAmount = PaidAmount,
            ImeiList = imeis
        };

        var result = await _stockService.ExportStockAsync(request);

        if (result == "SUCCESS")
        {
            MessageBoxHelper.Info("Xuất kho thành công.");
            ImeiText = string.Empty;
            PaidAmount = 0;
            await LoadAsync();
        }
        else
        {
            MessageBoxHelper.Warning(result);
        }
    }
}