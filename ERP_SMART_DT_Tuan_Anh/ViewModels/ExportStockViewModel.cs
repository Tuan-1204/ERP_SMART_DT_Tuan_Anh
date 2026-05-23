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
    private readonly ImeiService _imeiService = new();

    public ObservableCollection<Product> Products { get; } = new();
    public ObservableCollection<Partner> Customers { get; } = new();

    private Product? _selectedProduct;
    private Partner? _selectedCustomer;
    private string _unitPriceText = string.Empty;
    private string _paidAmountText = string.Empty;
    private decimal _totalAmount;
    private decimal _changeAmount;
    private decimal _remainingAmount;
    private string _imeiText = string.Empty;
    private string _barcodeText = string.Empty;
    private string _scanMessage = "Quét IMEI để tự nhận diện sản phẩm cần xuất.";

    public Product? SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            if (SetProperty(ref _selectedProduct, value) && value != null)
                UnitPriceText = MoneyHelper.FormatInput(value.ExportPrice);
        }
    }

    public Partner? SelectedCustomer
    {
        get => _selectedCustomer;
        set => SetProperty(ref _selectedCustomer, value);
    }

    public string UnitPriceText
    {
        get => _unitPriceText;
        set
        {
            if (SetProperty(ref _unitPriceText, MoneyHelper.NormalizeInput(value)))
                RecalculatePayment();
        }
    }

    public string PaidAmountText
    {
        get => _paidAmountText;
        set
        {
            if (SetProperty(ref _paidAmountText, MoneyHelper.NormalizeInput(value)))
                RecalculatePayment();
        }
    }

    public decimal TotalAmount
    {
        get => _totalAmount;
        set => SetProperty(ref _totalAmount, value);
    }

    public decimal ChangeAmount
    {
        get => _changeAmount;
        set => SetProperty(ref _changeAmount, value);
    }

    public decimal RemainingAmount
    {
        get => _remainingAmount;
        set => SetProperty(ref _remainingAmount, value);
    }

    public string ImeiText
    {
        get => _imeiText;
        set
        {
            if (SetProperty(ref _imeiText, value))
                RecalculatePayment();
        }
    }

    public string BarcodeText
    {
        get => _barcodeText;
        set => SetProperty(ref _barcodeText, value);
    }

    public string ScanMessage
    {
        get => _scanMessage;
        set => SetProperty(ref _scanMessage, value);
    }

    public ICommand ExportCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ScanBarcodeCommand { get; }

    public ExportStockViewModel()
    {
        ExportCommand = new AsyncRelayCommand(_ => ExportAsync());
        RefreshCommand = new AsyncRelayCommand(_ => LoadAsync());
        ScanBarcodeCommand = new AsyncRelayCommand(_ => ScanBarcodeAsync());
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

    private async Task ScanBarcodeAsync()
    {
        var imeiCode = BarcodeText.Trim();
        if (string.IsNullOrWhiteSpace(imeiCode))
        {
            MessageBoxHelper.Warning("Vui lòng nhập hoặc quét IMEI.");
            return;
        }

        var imei = await _imeiService.GetByImeiAsync(imeiCode);
        if (imei == null)
        {
            ScanMessage = "Không tìm thấy IMEI trong kho.";
            MessageBoxHelper.Warning(ScanMessage);
            return;
        }

        if (imei.StatusId != 1)
        {
            ScanMessage = $"IMEI không ở trạng thái Trong kho. Trạng thái hiện tại: {imei.Status?.StatusName ?? "không xác định"}.";
            MessageBoxHelper.Warning(ScanMessage);
            return;
        }

        if (!imei.ProductId.HasValue)
        {
            MessageBoxHelper.Warning("IMEI chưa gắn với sản phẩm.");
            return;
        }

        SelectProduct(imei.ProductId.Value);
        ImeiText = AppendImei(ImeiText, imei.Imei);
        ScanMessage = $"Đã nhận diện sản phẩm: {imei.Product?.ProductName ?? SelectedProduct?.ProductName}.";
    }

    private async Task ExportAsync()
    {
        if (SelectedProduct == null || SelectedCustomer == null)
        {
            MessageBoxHelper.Warning("Vui lòng chọn sản phẩm và khách hàng.");
            return;
        }

        var unitPrice = MoneyHelper.ParseVnd(UnitPriceText);
        var paidAmount = MoneyHelper.ParseVnd(PaidAmountText);
        if (unitPrice <= 0)
        {
            MessageBoxHelper.Warning("Vui lòng nhập đơn giá bán hợp lệ.");
            return;
        }

        var imeis = ImeiHelper.ParseImeiList(ImeiText);
        if (imeis.Count == 0)
        {
            MessageBoxHelper.Warning("Vui lòng nhập hoặc quét danh sách IMEI cần xuất.");
            return;
        }

        var duplicateInText = imeis.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
        if (duplicateInText.Count > 0)
        {
            MessageBoxHelper.Warning("Danh sách xuất bị trùng IMEI: " + string.Join(", ", duplicateInText));
            return;
        }

        var dbImeis = await _imeiService.GetByImeisAsync(imeis);
        var missingImeis = imeis.Except(dbImeis.Select(x => x.Imei)).ToList();
        if (missingImeis.Count > 0)
        {
            MessageBoxHelper.Warning("Các IMEI không tồn tại trong kho: " + string.Join(", ", missingImeis));
            return;
        }

        var unavailableImeis = dbImeis.Where(x => x.StatusId != 1).Select(x => x.Imei).ToList();
        if (unavailableImeis.Count > 0)
        {
            MessageBoxHelper.Warning("Các IMEI không còn ở trạng thái Trong kho: " + string.Join(", ", unavailableImeis));
            return;
        }

        var wrongProductImeis = dbImeis.Where(x => x.ProductId != SelectedProduct.Id).Select(x => x.Imei).ToList();
        if (wrongProductImeis.Count > 0)
        {
            MessageBoxHelper.Warning("Các IMEI không thuộc sản phẩm đang chọn: " + string.Join(", ", wrongProductImeis));
            return;
        }

        var request = new ExportStockRequestDto
        {
            BillId = BillCodeGenerator.GenerateExportCode(DateTime.Now.Second + 1),
            ObjectId = SelectedCustomer.Id,
            UserId = 1,
            ProductId = SelectedProduct.Id,
            UnitPrice = unitPrice,
            TotalAmount = TotalAmount,
            PaidAmount = paidAmount,
            ImeiList = imeis
        };

        var result = await _stockService.ExportStockAsync(request);
        if (result == "SUCCESS")
        {
            MessageBoxHelper.Info("Xuất kho thành công.");
            ImeiText = string.Empty;
            BarcodeText = string.Empty;
            PaidAmountText = string.Empty;
            RecalculatePayment();
            ScanMessage = "Xuất kho thành công. Có thể tiếp tục quét đơn mới.";
            await LoadAsync();
        }
        else
        {
            MessageBoxHelper.Warning(result);
        }
    }

    private void SelectProduct(int productId)
    {
        SelectedProduct = Products.FirstOrDefault(x => x.Id == productId);
    }

    private static string AppendImei(string currentText, string imei)
    {
        var list = ImeiHelper.ParseImeiList(currentText);
        if (!list.Contains(imei))
            list.Add(imei);

        return string.Join(Environment.NewLine, list);
    }

    private void RecalculatePayment()
    {
        var quantity = ImeiHelper.ParseImeiList(ImeiText).Count;
        var unitPrice = MoneyHelper.ParseVnd(UnitPriceText);
        var paidAmount = MoneyHelper.ParseVnd(PaidAmountText);

        TotalAmount = unitPrice * quantity;
        ChangeAmount = paidAmount > TotalAmount ? paidAmount - TotalAmount : 0;
        RemainingAmount = TotalAmount > paidAmount ? TotalAmount - paidAmount : 0;
    }
}
