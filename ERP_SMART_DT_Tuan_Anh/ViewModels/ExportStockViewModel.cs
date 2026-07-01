using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
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

        private string _unitPriceText = string.Empty;
        private string _paidAmountText = string.Empty;

        [ObservableProperty]
        private decimal totalAmount;

        [ObservableProperty]
        private decimal changeAmount;

        [ObservableProperty]
        private decimal remainingAmount;

        [ObservableProperty]
        private string barcodeText = string.Empty;

        [ObservableProperty]
        private string scanMessage = "Đang chờ quét Barcode / Mã IMEI bán hàng...";

        [ObservableProperty]
        private int scannedCount;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string? errorMessage;

        // --- PROPERTY ĐƠN GIÁ XUẤT TỰ ĐỘNG PHÂN CHIA TIỀN TỆ ---
        public string UnitPriceText
        {
            get => _unitPriceText;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    if (SetProperty(ref _unitPriceText, string.Empty)) RecalculatePayment();
                    return;
                }
                string clean = value.Replace(",", "").Replace(".", "").Trim();
                if (decimal.TryParse(clean, out decimal parsed))
                {
                    if (SetProperty(ref _unitPriceText, parsed.ToString("N0"))) RecalculatePayment();
                }
            }
        }

        // --- PROPERTY KHÁCH TRẢ TỰ ĐỘNG PHÂN CHIA TIỀN TỆ ---
        public string PaidAmountText
        {
            get => _paidAmountText;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    if (SetProperty(ref _paidAmountText, string.Empty)) RecalculatePayment();
                    return;
                }
                string clean = value.Replace(",", "").Replace(".", "").Trim();
                if (decimal.TryParse(clean, out decimal parsed))
                {
                    if (SetProperty(ref _paidAmountText, parsed.ToString("N0"))) RecalculatePayment();
                }
            }
        }

        // --- HIỂN THỊ SỐ TỒN KHO TRỰC QUAN ---
        public string ProductStockAvailableText => SelectedProduct != null
            ? $"[ Số kho khả dụng: {SelectedProduct.CurrentStock} máy còn lại ]"
            : "[ Vui lòng chọn dòng máy để đối soát số lượng tồn kho ]";

        public ExportStockViewModel()
        {
            _ = LoadAsync();
        }

        partial void OnSelectedProductChanged(Product? value)
        {
            if (value != null)
            {
                UnitPriceText = value.ExportPrice.ToString("N0");
            }
            else
            {
                UnitPriceText = string.Empty;
            }
            OnPropertyChanged(nameof(ProductStockAvailableText));
            RecalculatePayment();
        }

        private async Task LoadAsync()
        {
            try
            {
                Products.Clear();
                Customers.Clear();

                var allProducts = await _productService.GetAllAsync();
                foreach (var item in allProducts) Products.Add(item);

                var allCustomers = await _partnerService.GetCustomersAsync();
                foreach (var item in allCustomers) Customers.Add(item);
            }
            catch (Exception ex)
            {
                ErrorMessage = "Lỗi tải danh mục cấu hình: " + ex.Message;
            }
        }

        [RelayCommand]
        private async Task ScanBarcodeAsync()
        {
            var imeiCode = BarcodeText.Trim();
            if (string.IsNullOrWhiteSpace(imeiCode)) return;

            BarcodeText = string.Empty;

            if (SelectedProduct == null)
            {
                ScanMessage = "⚠️ Hãy chọn dòng sản phẩm trên thiết bị trước khi quét mã nhận diện IMEI.";
                return;
            }

            // NGHIỆP VỤ MỚI: Bẫy kiểm soát độ dài IMEI ngay tại Client để chống nổ lỗi Check Constraint DB
            if (imeiCode.Length < 10)
            {
                ScanMessage = $"❌ LỖI ĐỘ DÀI: Mã IMEI [{imeiCode}] quá ngắn ({imeiCode.Length} ký tự). Yêu cầu tối thiểu phải đạt 10 ký tự!";
                return;
            }

            if (ImeiList.Contains(imeiCode))
            {
                ScanMessage = $"Mã máy IMEI {imeiCode} đã được quét cho vào giỏ xuất kho!";
                return;
            }

            var imei = await _imeiService.GetByImeiAsync(imeiCode);

            if (imei == null)
            {
                // --- LUỒNG BYPASS SỐ LIỆU CHO PHIÊN BẢN DEMO ĐỒ ÁN ---
                if (ImeiList.Count >= SelectedProduct.CurrentStock)
                {
                    ScanMessage = $"❌ TỒN KHO KHÔNG ĐỦ: Kho hàng chỉ còn {SelectedProduct.CurrentStock} máy, không thể quét thêm thiết bị thứ {ImeiList.Count + 1}.";
                    return;
                }

                ImeiList.Add(imeiCode);
                ScannedCount = ImeiList.Count;
                RecalculatePayment();
                ScanMessage = $"📥 Đã nhận diện và thêm IMEI thử nghiệm: {imeiCode} (Theo dòng {SelectedProduct.ProductName}).";
                return;
            }

            // --- LUỒNG KIỂM TRA CHẶT CHẼ TRÊN DỮ LIỆU CSDL GỐC ---
            if (!imei.ProductId.HasValue || imei.ProductId.Value != SelectedProduct.Id)
            {
                ScanMessage = "❌ LỖI NGHIỆP VỤ: Thiết bị định danh vừa quét không thuộc về dòng sản phẩm đang chọn.";
                return;
            }

            if (imei.StatusId == 2)
            {
                ScanMessage = $"❌ LỖI GIAO DỊCH: Thiết bị IMEI [{imeiCode}] đã được xuất bán từ trước!";
                return;
            }

            if (ImeiList.Count >= SelectedProduct.CurrentStock)
            {
                ScanMessage = $"❌ TỒN KHO KHÔNG ĐỦ: Kho hàng chỉ còn {SelectedProduct.CurrentStock} máy.";
                return;
            }

            ImeiList.Add(imeiCode);
            ScannedCount = ImeiList.Count;
            RecalculatePayment();
            ScanMessage = $"📥 Đã quét thành công thiết bị máy IMEI: {imeiCode}.";
        }

        [RelayCommand]
        private void RemoveImei(string imei)
        {
            if (ImeiList.Contains(imei))
            {
                ImeiList.Remove(imei);
                ScannedCount = ImeiList.Count;
                RecalculatePayment();
                ScanMessage = $"Đã gỡ máy {imei} ra khỏi giỏ hàng hóa xuất kho.";
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

            string cleanPrice = _unitPriceText.Replace(",", "").Replace(".", "").Trim();
            string cleanPaid = _paidAmountText.Replace(",", "").Replace(".", "").Trim();

            decimal.TryParse(string.IsNullOrEmpty(cleanPrice) ? "0" : cleanPrice, out decimal unitPrice);
            decimal.TryParse(string.IsNullOrEmpty(cleanPaid) ? "0" : cleanPaid, out decimal paidAmount);

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
                MessageBoxHelper.Warning("Vui lòng chỉ định rõ Khách hàng đối tác và Sản phẩm thiết bị.");
                return;
            }

            if (ImeiList.Count == 0)
            {
                MessageBoxHelper.Warning("Giỏ hàng hóa xuất kho đang trống! Vui lòng thực hiện thao tác quét mã vạch.");
                return;
            }

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                var cleanedImei = ImeiList.Select(x => x.Trim()).Distinct().ToList();

                string cleanPrice = _unitPriceText.Replace(",", "").Replace(".", "").Trim();
                string cleanPaid = _paidAmountText.Replace(",", "").Replace(".", "").Trim();

                decimal.TryParse(cleanPrice, out decimal cleanUnitPrice);
                decimal.TryParse(cleanPaid, out decimal cleanPaidAmount);

                TotalAmount = cleanUnitPrice * cleanedImei.Count;
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
                    MessageBoxHelper.Info("Hệ thống đã phê duyệt hạch toán chứng từ và lập phiếu xuất bán hàng thành công!");

                    _unitPriceText = string.Empty;
                    _paidAmountText = string.Empty;
                    OnPropertyChanged(nameof(UnitPriceText));
                    OnPropertyChanged(nameof(PaidAmountText));

                    ImeiList.Clear();
                    ScannedCount = 0;
                    SelectedProduct = null;

                    RecalculatePayment();
                    await LoadAsync();
                }
                else
                {
                    MessageBoxHelper.Warning($"Lỗi nghiệp vụ từ mã Database: {result}");
                }
            }
            catch (Exception ex)
            {
                MessageBoxHelper.Error("Lỗi kết nối hạch toán luồng xuất kho: " + ex.Message);
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