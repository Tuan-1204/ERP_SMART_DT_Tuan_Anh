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

    public ObservableCollection<Product> Products { get; } = new();
    public ObservableCollection<Partner> Suppliers { get; } = new();

    public Product? SelectedProduct { get; set; }
    public Partner? SelectedSupplier { get; set; }

    public decimal UnitPrice { get; set; }
    public decimal PaidAmount { get; set; }
    public string Note { get; set; } = string.Empty;
    public string ImeiText { get; set; } = string.Empty;

    public ICommand ImportCommand { get; }

    public ImportStockViewModel()
    {
        ImportCommand = new AsyncRelayCommand(_ => ImportAsync());
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

    private async Task ImportAsync()
    {
        if (SelectedProduct == null || SelectedSupplier == null)
        {
            MessageBoxHelper.Warning("Vui lòng chọn sản phẩm và nhà cung cấp.");
            return;
        }

        var imeis = ImeiHelper.ParseImeiList(ImeiText);
        if (imeis.Count == 0)
        {
            MessageBoxHelper.Warning("Vui lòng nhập danh sách IMEI.");
            return;
        }

        var request = new ImportStockRequestDto
        {
            BillId = BillCodeGenerator.GenerateImportCode(DateTime.Now.Second + 1),
            ObjectId = SelectedSupplier.Id,
            UserId = 1,
            ProductId = SelectedProduct.Id,
            UnitPrice = UnitPrice,
            TotalAmount = UnitPrice * imeis.Count,
            PaidAmount = PaidAmount,
            Note = Note,
            ImeiList = imeis
        };

        var result = await _stockService.ImportStockAsync(request);

        if (result == "SUCCESS")
            MessageBoxHelper.Info("Nhập kho thành công.");
        else
            MessageBoxHelper.Warning(result);
    }
}