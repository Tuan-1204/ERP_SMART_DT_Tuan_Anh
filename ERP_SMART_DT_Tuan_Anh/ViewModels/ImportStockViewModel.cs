using System.Collections.ObjectModel;
using System.Windows.Input;
using ERP_SMART_DT_Tuan_Anh.Commands;
using ERP_SMART_DT_Tuan_Anh.DTOs;
using ERP_SMART_DT_Tuan_Anh.Helpers;
using ERP_SMART_DT_Tuan_Anh.Models;
using ERP_SMART_DT_Tuan_Anh.Services;

namespace ERP_SMART_DT_Tuan_Anh.ViewModels;

public class ImportStockViewModel : BaseViewModel
{
    private readonly StockService _stockService = new();
    private readonly ProductService _productService = new();
    private readonly PartnerService _partnerService = new();
    private readonly ImeiService _imeiService = new();

    private Product? _selectedProduct;
    private Partner? _selectedSupplier;
    private string _unitPriceText = string.Empty;
    private string _paidAmountText = string.Empty;
    private string _note = string.Empty;
    private string _imeiText = string.Empty;
    private string _barcodeText = string.Empty;
    private string _productSearchText = string.Empty;
    private string _scanMessage = "Có thể nhập tay tên/mã sản phẩm hoặc quét barcode sản phẩm/IMEI.";

    public ObservableCollection<Product> Products { get; } = new();
    public ObservableCollection<Partner> Suppliers { get; } = new();

    public Product? SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            if (SetProperty(ref _selectedProduct, value) && value != null)
                UnitPriceText = MoneyHelper.FormatInput(value.ImportPrice);
        }
    }

    public Partner? SelectedSupplier
    {
        get => _selectedSupplier;
        set => SetProperty(ref _selectedSupplier, value);
    }

    public string UnitPriceText
    {
        get => _unitPriceText;
        set
        {
            if (SetProperty(ref _unitPriceText, MoneyHelper.NormalizeInput(value)))
                RefreshMoneySummary();
        }
    }

    public string PaidAmountText
    {
        get => _paidAmountText;
        set
        {
            if (SetProperty(ref _paidAmountText, MoneyHelper.NormalizeInput(value)))
                RefreshMoneySummary();
        }
    }

    public string Note
    {
        get => _note;
        set => SetProperty(ref _note, value);
    }

    public string ImeiText
    {
        get => _imeiText;
        set
        {
            if (SetProperty(ref _imeiText, value))
                RefreshMoneySummary();
        }
    }

    public string BarcodeText
    {
        get => _barcodeText;
        set => SetProperty(ref _barcodeText, value);
    }

    public string ProductSearchText
    {
        get => _productSearchText;
        set => SetProperty(ref _productSearchText, value);
    }

    public string ScanMessage
    {
        get => _scanMessage;
        set => SetProperty(ref _scanMessage, value);
    }

    public int ImeiQuantity => ImeiHelper.ParseImeiList(ImeiText).Count;

    public decimal TotalAmount => MoneyHelper.ParseVnd(UnitPriceText) * ImeiQuantity;

    public decimal RemainingSupplierDebt
    {
        get
        {
            var remaining = TotalAmount - MoneyHelper.ParseVnd(PaidAmountText);
            return remaining > 0 ? remaining : 0;
        }
    }

    public string QuantityText => $"{ImeiQuantity:N0} IMEI";

    public string TotalAmountText => $"{TotalAmount:N0} VND";

    public string RemainingSupplierDebtText => $"{RemainingSupplierDebt:N0} VND";

    public string PaymentStatusText
    {
        get
        {
            if (ImeiQuantity == 0 || TotalAmount <= 0)
                return "Chưa có dữ liệu tính tiền";

            return RemainingSupplierDebt > 0
                ? "Phiếu nhập còn công nợ nhà cung cấp"
                : "Phiếu nhập đã thanh toán đủ cho nhà cung cấp";
        }
    }

    public ICommand ImportCommand { get; }
    public ICommand ScanBarcodeCommand { get; }
    public ICommand FindProductCommand { get; }

    public ImportStockViewModel()
    {
        ImportCommand = new AsyncRelayCommand(_ => ImportAsync());
        ScanBarcodeCommand = new AsyncRelayCommand(_ => ScanBarcodeAsync());
        FindProductCommand = new RelayCommand(_ => FindProductByText());
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        Products.Clear();
        Suppliers.Clear();

        foreach (var item in await _productService.GetAllAsync())
            Products.Add(item);

        foreach (var item in await _partnerService.GetSuppliersAsync())
            Suppliers.Add(item);
    }

    private async Task ScanBarcodeAsync()
    {
        var code = BarcodeText.Trim();
        if (string.IsNullOrWhiteSpace(code))
        {
            MessageBoxHelper.Warning("Vui lòng nhập hoặc quét mã barcode.");
            return;
        }

        var productByCode = await _productService.GetByCodeAsync(code);
        if (productByCode != null)
        {
            SelectProduct(productByCode.Id);
            ScanMessage = $"Đã chọn sản phẩm: {productByCode.ProductName}.";
            return;
        }

        var imei = await _imeiService.GetByImeiAsync(code);
        if (imei != null)
        {
            if (imei.ProductId.HasValue)
                SelectProduct(imei.ProductId.Value);

            ScanMessage = $"IMEI đã tồn tại, thuộc sản phẩm: {imei.Product?.ProductName ?? "không xác định"}. Không được nhập trùng.";
            MessageBoxHelper.Warning(ScanMessage);
            return;
        }

        ImeiText = AppendImei(ImeiText, code);
        ScanMessage = "IMEI mới đã được thêm vào danh sách nhập kho.";
    }

    private void FindProductByText()
    {
        var keyword = ProductSearchText.Trim();
        if (string.IsNullOrWhiteSpace(keyword))
        {
            MessageBoxHelper.Warning("Vui lòng nhập mã hoặc tên sản phẩm cần tìm.");
            return;
        }

        var product = Products.FirstOrDefault(x =>
            string.Equals(x.ProductCode, keyword, StringComparison.OrdinalIgnoreCase) ||
            x.ProductName.Contains(keyword, StringComparison.OrdinalIgnoreCase));

        if (product == null)
        {
            MessageBoxHelper.Warning("Không tìm thấy sản phẩm phù hợp.");
            return;
        }

        SelectedProduct = product;
        ScanMessage = $"Đã chọn sản phẩm: {product.ProductName}. Hãy quét IMEI để nhập đúng sản phẩm này.";
    }

    private async Task ImportAsync()
    {
        if (SelectedProduct == null || SelectedSupplier == null)
        {
            MessageBoxHelper.Warning("Vui lòng chọn sản phẩm và nhà cung cấp.");
            return;
        }

        var unitPrice = MoneyHelper.ParseVnd(UnitPriceText);
        var paidAmount = MoneyHelper.ParseVnd(PaidAmountText);
        if (unitPrice <= 0)
        {
            MessageBoxHelper.Warning("Vui lòng nhập đơn giá nhập hợp lệ.");
            return;
        }

        var imeis = ImeiHelper.ParseImeiList(ImeiText);
        if (imeis.Count == 0)
        {
            MessageBoxHelper.Warning("Vui lòng nhập hoặc quét danh sách IMEI.");
            return;
        }

        var invalidImeis = imeis.Where(x => !ImeiHelper.IsValid(x)).ToList();
        if (invalidImeis.Count > 0)
        {
            MessageBoxHelper.Warning("Có IMEI không hợp lệ: " + string.Join(", ", invalidImeis));
            return;
        }

        var duplicateInText = imeis.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
        if (duplicateInText.Count > 0)
        {
            MessageBoxHelper.Warning("Danh sách nhập bị trùng IMEI: " + string.Join(", ", duplicateInText));
            return;
        }

        var existingImeis = await _imeiService.GetExistingImeisAsync(imeis);
        if (existingImeis.Count > 0)
        {
            MessageBoxHelper.Warning("Các IMEI đã tồn tại, không thể nhập trùng: " + string.Join(", ", existingImeis));
            return;
        }

        var request = new ImportStockRequestDto
        {
            BillId = BillCodeGenerator.GenerateImportCode(DateTime.Now.Second + 1),
            ObjectId = SelectedSupplier.Id,
            UserId = 1,
            ProductId = SelectedProduct.Id,
            UnitPrice = unitPrice,
            TotalAmount = unitPrice * imeis.Count,
            PaidAmount = paidAmount,
            Note = Note,
            ImeiList = imeis
        };

        var result = await _stockService.ImportStockAsync(request);
        if (result == "SUCCESS")
        {
            MessageBoxHelper.Info("Nhập kho thành công.");
            ImeiText = string.Empty;
            BarcodeText = string.Empty;
            PaidAmountText = string.Empty;
            ScanMessage = "Nhập kho thành công. Có thể tiếp tục quét lô mới.";
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

    private void RefreshMoneySummary()
    {
        OnPropertyChanged(nameof(ImeiQuantity));
        OnPropertyChanged(nameof(TotalAmount));
        OnPropertyChanged(nameof(RemainingSupplierDebt));
        OnPropertyChanged(nameof(QuantityText));
        OnPropertyChanged(nameof(TotalAmountText));
        OnPropertyChanged(nameof(RemainingSupplierDebtText));
        OnPropertyChanged(nameof(PaymentStatusText));
    }
}
