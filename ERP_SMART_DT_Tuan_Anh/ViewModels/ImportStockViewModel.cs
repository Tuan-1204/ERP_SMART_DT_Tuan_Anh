using System;
using System.Collections.ObjectModel;
using System.Linq;
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
        private string _rawImeiText = string.Empty;
        private decimal _totalAmount;
        private decimal _remainingDebtAmount;
        private string _noteText = string.Empty;

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
                    SetProperty(ref _unitPriceText, value);
                    RecalculateImportPayment();
                    return;
                }
                string clean = value.Replace(",", "").Replace(".", "").Trim();
                if (decimal.TryParse(clean, out decimal parsed))
                {
                    if (SetProperty(ref _unitPriceText, parsed.ToString("N0")))
                        RecalculateImportPayment();
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
                    SetProperty(ref _paidAmountText, value);
                    RecalculateImportPayment();
                    return;
                }
                string clean = value.Replace(",", "").Replace(".", "").Trim();
                if (decimal.TryParse(clean, out decimal parsed))
                {
                    if (SetProperty(ref _paidAmountText, parsed.ToString("N0")))
                        RecalculateImportPayment();
                }
            }
        }

        public string RawImeiText
        {
            get => _rawImeiText;
            set
            {
                if (SetProperty(ref _rawImeiText, value))
                    RecalculateImportPayment();
            }
        }

        public decimal TotalAmount
        {
            get => _totalAmount;
            set => SetProperty(ref _totalAmount, value);
        }

        public decimal RemainingDebtAmount
        {
            get => _remainingDebtAmount;
            set => SetProperty(ref _remainingDebtAmount, value);
        }

        public string NoteText
        {
            get => _noteText;
            set => SetProperty(ref _noteText, value);
        }

        public ICommand ImportCommand { get; }
        public ICommand RefreshCommand { get; }

        public ImportStockViewModel()
        {
            ImportCommand = new AsyncRelayCommand(ExecuteImportAsync);
            RefreshCommand = new AsyncRelayCommand(LoadDataAsync);
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

        private void RecalculateImportPayment()
        {
            var imeis = RawImeiText.Split(new[] { ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                  .Select(x => x.Trim())
                                  .Where(x => x.Length >= 10)
                                  .ToList();

            int quantity = imeis.Count;
            if (quantity == 0)
            {
                TotalAmount = 0; RemainingDebtAmount = 0;
                return;
            }

            decimal unitPrice = decimal.Parse(string.IsNullOrWhiteSpace(UnitPriceText) ? "0" : UnitPriceText.Replace(",", "").Replace(".", ""));
            decimal paidAmount = decimal.Parse(string.IsNullOrWhiteSpace(PaidAmountText) ? "0" : PaidAmountText.Replace(",", "").Replace(".", ""));

            TotalAmount = unitPrice * quantity;
            RemainingDebtAmount = TotalAmount > paidAmount ? TotalAmount - paidAmount : 0;
        }

        private async Task ExecuteImportAsync()
        {
            if (SelectedProduct == null || SelectedSupplier == null)
            {
                MessageBoxHelper.Warning("Vui lòng chọn đầy đủ Nhà cung cấp đối tác và Sản phẩm cần nhập.");
                return;
            }

            var imeis = RawImeiText.Split(new[] { ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                  .Select(x => x.Trim())
                                  .Where(x => x.Length >= 10)
                                  .ToList();

            if (imeis.Count == 0)
            {
                MessageBoxHelper.Warning("Danh sách nhập trống hoặc các mã IMEI độ dài nhỏ hơn 10 ký tự!");
                return;
            }

            decimal cleanUnitPrice = decimal.Parse(string.IsNullOrWhiteSpace(UnitPriceText) ? "0" : UnitPriceText.Replace(",", "").Replace(".", ""));
            decimal cleanPaidAmount = decimal.Parse(string.IsNullOrWhiteSpace(PaidAmountText) ? "0" : PaidAmountText.Replace(",", "").Replace(".", ""));

            var request = new ImportStockRequestDto
            {
                BillId = "HDN" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                ObjectId = SelectedSupplier.Id,
                UserId = 1,
                ProductId = SelectedProduct.Id,
                UnitPrice = cleanUnitPrice,
                TotalAmount = TotalAmount,
                PaidAmount = cleanPaidAmount,
                Note = NoteText,
                ImeiList = imeis 
            };

            var result = await _stockService.ImportStockAsync(request);
            if (result == "SUCCESS")
            {
                MessageBoxHelper.Info("Nhập hàng thành công! Tồn kho và công nợ nhà cung cấp đã được tự động hạch toán.");
                RawImeiText = string.Empty;
                PaidAmountText = string.Empty;
                NoteText = string.Empty;
                RecalculateImportPayment();
                await LoadDataAsync();
            }
            else
            {
                MessageBoxHelper.Warning($"Lỗi nghiệp vụ hệ thống: {result}");
            }
        }
    }
}