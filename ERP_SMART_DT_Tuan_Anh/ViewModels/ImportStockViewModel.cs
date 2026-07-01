using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using ERP_SMART_DT_Tuan_Anh.DTOs;
using ERP_SMART_DT_Tuan_Anh.Helpers;
using ERP_SMART_DT_Tuan_Anh.Models;
using ERP_SMART_DT_Tuan_Anh.Services;

namespace ERP_SMART_DT_Tuan_Anh.ViewModels
{
    public class ImportStockViewModel : BaseViewModel
    {
        private readonly StockService _stockService = new();
        private readonly ProductService _productService = new();
        private readonly PartnerService _partnerService = new();

        public ObservableCollection<Product> Products { get; } = new();
        public ObservableCollection<Partner> Suppliers { get; } = new();

        private Product? _selectedProduct;
        private Partner? _selectedSupplier;
        private string _unitPriceText = string.Empty;
        private string _paidAmountText = string.Empty;
        private string _imeiText = string.Empty;
        private decimal _totalAmount;
        private decimal _remainingDebtAmount;
        private string _noteText = string.Empty;
        private string _productSearchText = string.Empty;

        public Product? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                if (SetProperty(ref _selectedProduct, value) && value != null)
                {
                    UnitPriceText = value.ImportPrice.ToString("N0");
                    RecalculateImportPayment();
                }
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
                if (string.IsNullOrWhiteSpace(value))
                {
                    if (SetProperty(ref _unitPriceText, string.Empty)) RecalculateImportPayment();
                    return;
                }

                string clean = value.Replace(",", "").Replace(".", "").Trim();
                if (decimal.TryParse(clean, out decimal parsed))
                {
                    if (SetProperty(ref _unitPriceText, parsed.ToString("N0"))) RecalculateImportPayment();
                }
            }
        }

        public string PaidAmountText
        {
            get => _paidAmountText;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    if (SetProperty(ref _paidAmountText, string.Empty)) RecalculateImportPayment();
                    return;
                }

                string clean = value.Replace(",", "").Replace(".", "").Trim();
                if (decimal.TryParse(clean, out decimal parsed))
                {
                    if (SetProperty(ref _paidAmountText, parsed.ToString("N0"))) RecalculateImportPayment();
                }
            }
        }

        public string ImeiText
        {
            get => _imeiText;
            set
            {
                if (SetProperty(ref _imeiText, value))
                {
                    RecalculateImportPayment();
                }
            }
        }

        public string NoteText
        {
            get => _noteText;
            set => SetProperty(ref _noteText, value);
        }

        public string ProductSearchText
        {
            get => _productSearchText;
            set => SetProperty(ref _productSearchText, value);
        }

        public string QuantityText
        {
            get
            {
                var imeis = ParseValidImeiList();
                return $"{imeis.Count} Máy";
            }
        }

        public string TotalAmountText => _totalAmount.ToString("N0") + " VND";
        public string RemainingSupplierDebtText => _remainingDebtAmount.ToString("N0") + " VND";

        public string PaymentStatusText
        {
            get
            {
                if (_totalAmount == 0) return "Chưa có thông tin sản phẩm và số lượng nhập.";
                if (_remainingDebtAmount == 0) return "Cửa hàng đã thanh toán đủ 100% tiền mặt cho Nhà cung cấp.";
                return $"Cửa hàng trả trước một phần tiền mặt, hạch toán ghi nợ gối đầu NCC số tiền: {_remainingDebtAmount:N0} VND";
            }
        }

        public ICommand ImportCommand { get; }
        public ICommand ResetFormCommand { get; }
        public ICommand FilterProductCommand { get; }

        public ImportStockViewModel()
        {
            ImportCommand = new AsyncRelayCommand(ExecuteImportAsync);
            ResetFormCommand = new RelayCommand(ExecuteResetForm);
            FilterProductCommand = new AsyncRelayCommand(ExecuteFilterProductAsync);
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            Products.Clear();
            Suppliers.Clear();

            var allProducts = await _productService.GetAllAsync();
            foreach (var item in allProducts) Products.Add(item);

            var allSuppliers = await _partnerService.GetSuppliersAsync();
            foreach (var item in allSuppliers) Suppliers.Add(item);
        }

        private async Task ExecuteFilterProductAsync()
        {
            var allProducts = await _productService.GetAllAsync();
            Products.Clear();

            var filtered = allProducts.Where(p => string.IsNullOrEmpty(ProductSearchText) ||
                                                 p.ProductName.Contains(ProductSearchText, StringComparison.OrdinalIgnoreCase) ||
                                                 p.ProductCode.Contains(ProductSearchText, StringComparison.OrdinalIgnoreCase));

            foreach (var item in filtered) Products.Add(item);
        }

        private void ExecuteResetForm()
        {
            _imeiText = string.Empty;
            _unitPriceText = string.Empty;
            _paidAmountText = string.Empty;
            _noteText = string.Empty;
            _productSearchText = string.Empty;
            _selectedProduct = null;
            _selectedSupplier = null;

            OnPropertyChanged(nameof(ImeiText));
            OnPropertyChanged(nameof(UnitPriceText));
            OnPropertyChanged(nameof(PaidAmountText));
            OnPropertyChanged(nameof(NoteText));
            OnPropertyChanged(nameof(ProductSearchText));
            OnPropertyChanged(nameof(SelectedProduct));
            OnPropertyChanged(nameof(SelectedSupplier));

            RecalculateImportPayment();
        }

    
        private List<string> ParseValidImeiList()
        {
            if (string.IsNullOrWhiteSpace(ImeiText)) return new List<string>();

            // Tách chuỗi theo dấu phẩy, dấu chấm phẩy hoặc ký tự xuống dòng
            string[] lines = ImeiText.Split(new[] { ',', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var list = new List<string>();
            foreach (var line in lines)
            {
                // Làm sạch khoảng trắng rác ở hai đầu đoạn mã IMEI
                string cleanImei = line.Trim();

                // Tiêu chuẩn độ dài >= 10 ký tự 
                if (!string.IsNullOrEmpty(cleanImei) && cleanImei.Length >= 10)
                {
                    list.Add(cleanImei);
                }
            }
            return list.Distinct().ToList(); // Chống nhập trùng lặp ngay tại Client
        }

        private void RecalculateImportPayment()
        {
            var imeis = ParseValidImeiList();
            int quantity = imeis.Count;

            if (quantity == 0)
            {
                _totalAmount = 0;
                _remainingDebtAmount = 0;

                OnPropertyChanged(nameof(QuantityText));
                OnPropertyChanged(nameof(TotalAmountText));
                OnPropertyChanged(nameof(RemainingSupplierDebtText));
                OnPropertyChanged(nameof(PaymentStatusText));
                return;
            }

            string cleanPrice = _unitPriceText.Replace(",", "").Replace(".", "").Trim();
            string cleanPaid = _paidAmountText.Replace(",", "").Replace(".", "").Trim();

            decimal.TryParse(string.IsNullOrEmpty(cleanPrice) ? "0" : cleanPrice, out decimal unitPrice);
            decimal.TryParse(string.IsNullOrEmpty(cleanPaid) ? "0" : cleanPaid, out decimal paidAmount);

            _totalAmount = unitPrice * quantity;
            _remainingDebtAmount = _totalAmount > paidAmount ? _totalAmount - paidAmount : 0;

            OnPropertyChanged(nameof(QuantityText));
            OnPropertyChanged(nameof(TotalAmountText));
            OnPropertyChanged(nameof(RemainingSupplierDebtText));
            OnPropertyChanged(nameof(PaymentStatusText));
        }

        private async Task ExecuteImportAsync()
        {
            if (SelectedProduct == null || SelectedSupplier == null)
            {
                MessageBoxHelper.Warning("Vui lòng chọn đầy đủ Nhà cung cấp đối tác và Sản phẩm cần nhập kho.");
                return;
            }

            var imeis = ParseValidImeiList();
            if (imeis.Count == 0)
            {
                MessageBoxHelper.Warning("Danh sách nhập hàng trống hoặc các mã số IMEI tự gõ chưa đạt độ dài tiêu chuẩn (tối thiểu 10 ký tự)!");
                return;
            }

            string cleanPrice = _unitPriceText.Replace(",", "").Replace(".", "").Trim();
            decimal.TryParse(cleanPrice, out decimal cleanUnitPrice);
            if (cleanUnitPrice <= 0)
            {
                MessageBoxHelper.Warning("Đơn giá nhập hàng bắt buộc phải lớn hơn 0 VND!");
                return;
            }

            string cleanPaid = _paidAmountText.Replace(",", "").Replace(".", "").Trim();
            decimal.TryParse(cleanPaid, out decimal cleanPaidAmount);

            var request = new ImportStockRequestDto
            {
                BillId = "HDN" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                ObjectId = SelectedSupplier.Id,
                UserId = 1,
                ProductId = SelectedProduct.Id,
                UnitPrice = cleanUnitPrice,
                TotalAmount = _totalAmount,
                PaidAmount = cleanPaidAmount,
                Note = NoteText,
                ImeiList = imeis
            };

            var result = await _stockService.ImportStockAsync(request);
            if (result == "SUCCESS")
            {
                MessageBoxHelper.Info("Nhập kho hàng thành công! Số lượng tồn kho và công nợ đối tác đã được tự động hạch toán.");
                ExecuteResetForm();
                await LoadDataAsync();
            }
            else
            {
                MessageBoxHelper.Warning($"Lỗi nghiệp vụ hệ thống từ Database: {result}");
            }
        }
    }
}