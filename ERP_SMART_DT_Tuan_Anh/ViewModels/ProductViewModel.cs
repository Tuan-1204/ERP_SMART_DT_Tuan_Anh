using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ERP_SMART_DT_Tuan_Anh.Helpers;
using ERP_SMART_DT_Tuan_Anh.Models;
using ERP_SMART_DT_Tuan_Anh.Services;

namespace ERP_SMART_DT_Tuan_Anh.ViewModels
{
    public class ProductViewModel : BaseViewModel
    {
        private readonly ProductService _productService = new();
        private readonly CategoryService _categoryService = new();

        private Product? _selectedProduct;
        private Product _currentProduct = new();
        private string _formTitle = "Thêm sản phẩm mới";

        private string _filterMinPriceText = string.Empty;
        private string _filterMaxPriceText = string.Empty;
        private int _selectedSortIndex = 0;

        public ObservableCollection<Product> Products { get; } = new();
        public ObservableCollection<Product> FilteredProducts { get; } = new();
        public ObservableCollection<Category> Categories { get; } = new();

        public Product? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                if (SetProperty(ref _selectedProduct, value) && value != null)
                {
                    // ĐÃ FIX: Khởi tạo giá trị gán thủ công thay thế cho hàm value.Clone() lỗi context
                    CurrentProduct = new Product
                    {
                        Id = value.Id,
                        ProductCode = value.ProductCode,
                        ProductName = value.ProductName,
                        CategoryId = value.CategoryId,
                        ImportPrice = value.ImportPrice,
                        ExportPrice = value.ExportPrice,
                        MinStock = value.MinStock,
                        AlertThreshold = value.AlertThreshold,
                        Unit = value.Unit,
                        CurrentStock = value.CurrentStock,
                        IsDeleted = value.IsDeleted,
                        CreatedDate = value.CreatedDate
                    };
                    FormTitle = "Cập nhật sản phẩm";
                }
            }
        }

        public Product CurrentProduct
        {
            get => _currentProduct;
            set => SetProperty(ref _currentProduct, value);
        }

        public string FormTitle
        {
            get => _formTitle;
            set => SetProperty(ref _formTitle, value);
        }

        public string FilterMinPriceText
        {
            get => _filterMinPriceText;
            set { if (SetProperty(ref _filterMinPriceText, value)) ApplyPriceFilter(); }
        }

        public string FilterMaxPriceText
        {
            get => _filterMaxPriceText;
            set { if (SetProperty(ref _filterMaxPriceText, value)) ApplyPriceFilter(); }
        }

        public int SelectedSortIndex
        {
            get => _selectedSortIndex;
            set { if (SetProperty(ref _selectedSortIndex, value)) ApplyPriceFilter(); }
        }

        public ICommand LoadCommand { get; }
        public ICommand NewCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearFilterCommand { get; }

        public ProductViewModel()
        {
            LoadCommand = new AsyncRelayCommand(LoadAsync);
            NewCommand = new RelayCommand(ExecuteNew);
            SaveCommand = new AsyncRelayCommand(ExecuteSaveAsync);
            DeleteCommand = new AsyncRelayCommand(ExecuteDeleteAsync);
            ClearFilterCommand = new RelayCommand(ExecuteClearFilter);

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            if (IsBusy) return;
            try
            {
                IsBusy = true;
                Products.Clear();
                Categories.Clear();

                var cats = await _categoryService.GetAllAsync();
                foreach (var c in cats) Categories.Add(c);

                var prods = await _productService.GetAllAsync();
                foreach (var p in prods) Products.Add(p);

                ApplyPriceFilter();
            }
            catch (Exception ex)
            {
                MessageBoxHelper.Error("Lỗi hệ thống khi tải danh mục: " + ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ApplyPriceFilter()
        {
            FilteredProducts.Clear();

            string cleanMin = FilterMinPriceText.Replace(",", "").Replace(".", "").Trim();
            string cleanMax = FilterMaxPriceText.Replace(",", "").Replace(".", "").Trim();

            decimal.TryParse(cleanMin, out decimal minPrice);
            if (string.IsNullOrEmpty(cleanMax)) cleanMax = "99999999999";
            decimal.TryParse(cleanMax, out decimal maxPrice);

            var query = Products.Where(p => p.ExportPrice >= minPrice && p.ExportPrice <= maxPrice);

            switch (SelectedSortIndex)
            {
                case 1:
                    query = query.OrderBy(p => p.ExportPrice);
                    break;
                case 2:
                    query = query.OrderByDescending(p => p.ExportPrice);
                    break;
                case 3:
                    query = query.OrderByDescending(p => p.CreatedDate);
                    break;
                case 4:
                    query = query.OrderBy(p => p.ProductName);
                    break;
                default:
                    query = query.OrderBy(p => p.ProductCode);
                    break;
            }

            foreach (var item in query)
            {
                FilteredProducts.Add(item);
            }
        }

        private void ExecuteClearFilter()
        {
            _filterMinPriceText = string.Empty;
            _filterMaxPriceText = string.Empty;
            _selectedSortIndex = 0;

            OnPropertyChanged(nameof(FilterMinPriceText));
            OnPropertyChanged(nameof(FilterMaxPriceText));
            OnPropertyChanged(nameof(SelectedSortIndex));

            ApplyPriceFilter();
        }

        private void ExecuteNew()
        {
            CurrentProduct = new Product();
            SelectedProduct = null;
            FormTitle = "Thêm sản phẩm mới";
        }

        private async Task ExecuteSaveAsync()
        {
            if (string.IsNullOrWhiteSpace(CurrentProduct.ProductCode) || string.IsNullOrWhiteSpace(CurrentProduct.ProductName))
            {
                MessageBoxHelper.Warning("Vui lòng nhập đầy đủ thông tin Mã và Tên mặt hàng thiết bị.");
                return;
            }

            try
            {
                if (CurrentProduct.Id == 0)
                {
                    await _productService.AddAsync(CurrentProduct);
                    MessageBoxHelper.Info("Thêm mới sản phẩm công nghệ thành công!");
                }
                else
                {
                    await _productService.UpdateAsync(CurrentProduct);
                    MessageBoxHelper.Info("Cập nhật thông tin sản phẩm thành công!");
                }

                ExecuteNew();
                await LoadAsync();
            }
            catch (Exception ex)
            {
                MessageBoxHelper.Error("Lỗi ghi sổ dữ liệu: " + ex.Message);
            }
        }

        private async Task ExecuteDeleteAsync()
        {
            if (SelectedProduct == null) return;

            if (MessageBoxHelper.Confirm($"Bạn có chắc chắn muốn xóa dòng máy [{SelectedProduct.ProductName}] khỏi hệ thống?"))
            {
                try
                {
                    await _productService.SoftDeleteAsync(SelectedProduct.Id);
                    MessageBoxHelper.Info("Đã xóa sản phẩm thành công!");
                    ExecuteNew();
                    await LoadAsync();
                }
                catch (Exception ex)
                {
                    MessageBoxHelper.Error("Sự cố xóa dữ liệu: " + ex.Message);
                }
            }
        }
    }
}