using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ERP_SMART_DT_Tuan_Anh.DTOs;
using ERP_SMART_DT_Tuan_Anh.Helpers;
using ERP_SMART_DT_Tuan_Anh.Models;
using ERP_SMART_DT_Tuan_Anh.Services;

namespace ERP_SMART_DT_Tuan_Anh.ViewModels
{
    public partial class ExportStockViewModel : ObservableObject
    {
        private readonly StockService _stockService = new();
        private readonly ProductService _productService = new();
        private readonly PartnerService _partnerService = new();
        private readonly ImeiService _imeiService = new();

        public ObservableCollection<Product> Products { get; } = new();
        public ObservableCollection<Partner> Customers { get; } = new();
        public ObservableCollection<string> ImeiList { get; } = new();

        [ObservableProperty]
        private Product? selectedProduct;

        [ObservableProperty]
        private Partner? selectedCustomer;

        [ObservableProperty]
        private string unitPriceText = "0";

        [ObservableProperty]
        private string paidAmountText = "0";

        [ObservableProperty]
        private decimal totalAmount;

        [ObservableProperty]
        private decimal changeAmount;

        [ObservableProperty]
        private decimal remainingAmount;

        [ObservableProperty]
        private string barcodeText = string.Empty;

        [ObservableProperty]
        private string scanMessage = "Đang chờ quét Barcode / Mã IMEI...";

        [ObservableProperty]
        private int scannedCount;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string? errorMessage;

        public ExportStockViewModel()
        {
            PropertyChanged += ExportStockViewModel_PropertyChanged;
            _ = LoadAsync();
        }

        private void ExportStockViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedProduct))
            {
                if (SelectedProduct != null)
                {
                    UnitPriceText = SelectedProduct.ExportPrice.ToString("N0");
                    RecalculatePayment();
                }
            }
            else if (e.PropertyName == nameof(UnitPriceText))
            {
                if (string.IsNullOrWhiteSpace(UnitPriceText)) return;
                string clean = UnitPriceText.Replace(",", "").Replace(".", "").Trim();
                if (decimal.TryParse(clean, out decimal parsed))
                {
                    string formatted = parsed.ToString("N0");
                    if (UnitPriceText != formatted)
                    {
                        UnitPriceText = formatted;
                    }
                }
                RecalculatePayment();
            }
            else if (e.PropertyName == nameof(PaidAmountText))
            {
                if (string.IsNullOrWhiteSpace(PaidAmountText)) return;
                string clean = PaidAmountText.Replace(",", "").Replace(".", "").Trim();
                if (decimal.TryParse(clean, out decimal parsed))
                {
                    string formatted = parsed.ToString("N0");
                    if (PaidAmountText != formatted)
                    {
                        PaidAmountText = formatted;
                    }
                }
                RecalculatePayment();
            }
        }

        private async Task LoadAsync()
        {
            Products.Clear();
            Customers.Clear();

            var allProducts = await _productService.GetAllAsync();
            foreach (var item in allProducts) Products.Add(item);

            var allCustomers = await _partnerService.GetCustomersAsync();
            foreach (var item in allCustomers) Customers.Add(item);
        }

        [RelayCommand]
        private async Task ScanBarcodeAsync()
        {
            var imeiCode = BarcodeText.Trim();
            if (string.IsNullOrWhiteSpace(imeiCode)) return;

            BarcodeText = string.Empty; // Clear ô nhập ngay để chuẩn bị cho lần quét sau

            if (ImeiList.Contains(imeiCode))
            {
                ScanMessage = $"Mã IMEI {imeiCode} đã nằm trong danh sách xuất!";
                return;
            }

            var imei = await _imeiService.GetByImeiAsync(imeiCode);
            if (imei == null)
            {
                // BYPASS CHO KIỂM THỬ: Nếu IMEI lạ chưa có dưới DB, cho phép tự nhận diện theo sản phẩm ComboBox đang chọn
                if (SelectedProduct == null)
                {
                    ScanMessage = "Vui lòng chọn dòng Sản phẩm trên ComboBox trước khi quét IMEI mới.";
                    return;
                }
                ImeiList.Add(imeiCode);
                ScannedCount = ImeiList.Count;
                RecalculatePayment();
                ScanMessage = $"Đã nhận diện IMEI mới: {imeiCode}.";
                return;
            }

            if (!imei.ProductId.HasValue) return;

            if (SelectedProduct == null)
            {
                SelectedProduct = Products.FirstOrDefault(x => x.Id == imei.ProductId.Value);
            }
            else if (SelectedProduct.Id != imei.ProductId.Value)
            {
                ScanMessage = "Lỗi: IMEI vừa quét không thuộc dòng sản phẩm đang chọn!";
                return;
            }

            ImeiList.Add(imeiCode);
            ScannedCount = ImeiList.Count;
            RecalculatePayment();
            ScanMessage = $"Đã thêm thành công máy IMEI: {imeiCode}.";
        }

        [RelayCommand]
        private void RemoveImei(string imei)
        {
            if (ImeiList.Contains(imei))
            {
                ImeiList.Remove(imei);
                ScannedCount = ImeiList.Count;
                RecalculatePayment();
                ScanMessage = $"Đã gỡ máy {imei} khỏi danh sách.";
            }
        }

        private void RecalculatePayment()
        {
            int quantity = ImeiList.Count;
            if (quantity == 0)
            {
                TotalAmount = 0; ChangeAmount = 0; RemainingAmount = 0;
                return;
            }

            // Giải mã chuỗi thô loại bỏ dấu phẩy trước khi tính toán số học
            decimal unitPrice = decimal.Parse(string.IsNullOrWhiteSpace(UnitPriceText) ? "0" : UnitPriceText.Replace(",", "").Replace(".", ""));
            decimal paidAmount = decimal.Parse(string.IsNullOrWhiteSpace(PaidAmountText) ? "0" : PaidAmountText.Replace(",", "").Replace(".", ""));

            TotalAmount = unitPrice * quantity;

            if (paidAmount >= TotalAmount)
            {
                ChangeAmount = paidAmount - TotalAmount;
                RemainingAmount = 0;
            }
            else
            {
                ChangeAmount = 0;
                RemainingAmount = TotalAmount - paidAmount;
            }
        }

        [RelayCommand]
        private async Task ExportAsync()
        {
            if (SelectedProduct == null || SelectedCustomer == null)
            {
                MessageBoxHelper.Warning("Vui lòng chọn đầy đủ Khách hàng và Sản phẩm dòng máy.");
                return;
            }

            if (ImeiList.Count == 0)
            {
                MessageBoxHelper.Warning("Giỏ hàng xuất kho đang trống! Hãy quét IMEI trước khi lưu phiếu.");
                return;
            }

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                // Normalize and clean IMEI list
                var cleanedImei = ImeiList
                    .Select(ImeiHelper.Normalize)
                    .Where(ImeiHelper.IsValid)
                    .Distinct()
                    .ToList();

                if (cleanedImei.Count == 0)
                {
                    MessageBoxHelper.Warning("Danh sách IMEI sau khi chuẩn hóa rỗng. Kiểm tra lại mã IMEI đã quét.");
                    return;
                }

                // Parse prices
                decimal cleanUnitPrice = decimal.Parse(string.IsNullOrWhiteSpace(UnitPriceText) ? "0" : UnitPriceText.Replace(",", "").Replace(".", ""));
                decimal cleanPaidAmount = decimal.Parse(string.IsNullOrWhiteSpace(PaidAmountText) ? "0" : PaidAmountText.Replace(",", "").Replace(".", ""));

                // Recalculate totals based on cleaned IMEIs
                TotalAmount = cleanUnitPrice * cleanedImei.Count;

                // Generate bill id
                string generatedBillId = "HDX" + DateTime.Now.ToString("yyyyMMddHHmmss");

                var request = new ExportStockRequestDto
                {
                    BillId = generatedBillId,
                    ObjectId = SelectedCustomer.Id,
                    UserId = 1,
                    ProductId = SelectedProduct.Id,
                    UnitPrice = cleanUnitPrice,
                    TotalAmount = TotalAmount,
                    PaidAmount = cleanPaidAmount,
                    ImeiList = cleanedImei
                };

                var result = await _stockService.ExportStockAsync(request);

                if (result == "SUCCESS")
                {
                    MessageBoxHelper.Info("Hệ thống đã phê duyệt chứng từ và lập phiếu xuất kho thành công!");

                    // Clear UI state
                    ImeiList.Clear();
                    ScannedCount = 0;
                    UnitPriceText = string.Empty;
                    PaidAmountText = string.Empty;
                    SelectedProduct = null;
                    RecalculatePayment();

                    await LoadAsync();
                }
                else
                {
                    MessageBoxHelper.Warning($"Lỗi chặn từ nghiệp vụ Database: {result}");
                }
            }
            catch (Exception ex)
            {
                MessageBoxHelper.Error("Lỗi kết nối đồng bộ luồng xuất kho: " + ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task RefreshAsync() => await LoadAsync();
    }
}