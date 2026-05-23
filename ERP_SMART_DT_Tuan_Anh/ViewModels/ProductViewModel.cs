using System.Collections.ObjectModel;
using System.Windows.Input;
using ERP_SMART_DT_Tuan_Anh.Commands;
using ERP_SMART_DT_Tuan_Anh.Helpers;
using ERP_SMART_DT_Tuan_Anh.Models;
using ERP_SMART_DT_Tuan_Anh.Services;

namespace ERP_SMART_DT_Tuan_Anh.ViewModels;

public class ProductViewModel : BaseViewModel
{
    private readonly ProductService _productService = new();
    private readonly CategoryService _categoryService = new();

    private Product _currentProduct = CreateDefaultProduct();
    private Product? _selectedProduct;
    private string _formTitle = "Thêm sản phẩm mới";

    public ObservableCollection<Product> Products { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();

    public Product CurrentProduct
    {
        get => _currentProduct;
        set => SetProperty(ref _currentProduct, value);
    }

    public Product? SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            if (SetProperty(ref _selectedProduct, value) && value != null)
            {
                CurrentProduct = CloneProduct(value);
                FormTitle = "Sửa sản phẩm";
            }
        }
    }

    public string FormTitle
    {
        get => _formTitle;
        set => SetProperty(ref _formTitle, value);
    }

    public ICommand LoadCommand { get; }
    public ICommand NewCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }

    public ProductViewModel()
    {
        LoadCommand = new AsyncRelayCommand(_ => LoadAsync());
        SaveCommand = new AsyncRelayCommand(_ => SaveAsync());
        DeleteCommand = new AsyncRelayCommand(_ => DeleteAsync());
        NewCommand = new RelayCommand(_ => ResetForm());

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        Products.Clear();
        Categories.Clear();

        foreach (var item in await _categoryService.GetAllAsync())
            Categories.Add(item);

        foreach (var item in await _productService.GetAllAsync())
            Products.Add(item);
    }

    private async Task SaveAsync()
    {
        NormalizeCurrentProduct();

        var validationMessage = ValidateProduct(CurrentProduct);
        if (!string.IsNullOrWhiteSpace(validationMessage))
        {
            MessageBoxHelper.Warning(validationMessage);
            return;
        }

        if (await _productService.IsProductCodeExistsAsync(CurrentProduct.ProductCode, CurrentProduct.Id))
        {
            MessageBoxHelper.Warning("Mã sản phẩm đã tồn tại. Vui lòng nhập mã khác.");
            return;
        }

        if (CurrentProduct.Id == 0)
        {
            CurrentProduct.CreatedBy = "SYSTEM";
            await _productService.AddAsync(CurrentProduct);
            MessageBoxHelper.Info("Thêm sản phẩm thành công.");
        }
        else
        {
            CurrentProduct.UpdatedBy = "SYSTEM";
            await _productService.UpdateAsync(CurrentProduct);
            MessageBoxHelper.Info("Cập nhật sản phẩm thành công.");
        }

        await LoadAsync();
        ResetForm();
    }

    private async Task DeleteAsync()
    {
        if (CurrentProduct.Id == 0)
        {
            MessageBoxHelper.Warning("Vui lòng chọn sản phẩm cần xóa.");
            return;
        }

        if (!MessageBoxHelper.Confirm($"Bạn có chắc muốn xóa hẳn sản phẩm \"{CurrentProduct.ProductName}\"? Thao tác này không thể hoàn tác."))
            return;

        var result = await _productService.HardDeleteAsync(CurrentProduct.Id);
        if (result != "SUCCESS")
        {
            MessageBoxHelper.Warning(result);
            return;
        }

        MessageBoxHelper.Info("Xóa hẳn sản phẩm thành công.");

        await LoadAsync();
        ResetForm();
    }

    private void ResetForm()
    {
        SelectedProduct = null;
        CurrentProduct = CreateDefaultProduct();
        FormTitle = "Thêm sản phẩm mới";
    }

    private void NormalizeCurrentProduct()
    {
        CurrentProduct.ProductCode = CurrentProduct.ProductCode.Trim();
        CurrentProduct.ProductName = CurrentProduct.ProductName.Trim();
        CurrentProduct.Unit = string.IsNullOrWhiteSpace(CurrentProduct.Unit)
            ? "Chiếc"
            : CurrentProduct.Unit.Trim();
    }

    private static string ValidateProduct(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.ProductCode))
            return "Vui lòng nhập mã sản phẩm.";

        if (string.IsNullOrWhiteSpace(product.ProductName))
            return "Vui lòng nhập tên sản phẩm.";

        if (product.CategoryId == null || product.CategoryId <= 0)
            return "Vui lòng chọn danh mục sản phẩm.";

        if (product.ImportPrice < 0)
            return "Giá nhập không được âm.";

        if (product.ExportPrice < 0)
            return "Giá bán không được âm.";

        if (product.ExportPrice > 0 && product.ImportPrice > product.ExportPrice)
            return "Giá nhập đang lớn hơn giá bán. Vui lòng kiểm tra lại.";

        if (product.MinStock < 0)
            return "Tồn tối thiểu không được âm.";

        if (product.AlertThreshold < 0)
            return "Ngưỡng cảnh báo không được âm.";

        return string.Empty;
    }

    private static Product CreateDefaultProduct()
    {
        return new Product
        {
            MinStock = 5,
            AlertThreshold = 10,
            Unit = "Chiếc",
            ImportPrice = 0,
            ExportPrice = 0
        };
    }

    private static Product CloneProduct(Product source)
    {
        return new Product
        {
            Id = source.Id,
            CategoryId = source.CategoryId,
            ProductCode = source.ProductCode,
            ProductName = source.ProductName,
            ImportPrice = source.ImportPrice,
            ExportPrice = source.ExportPrice,
            CurrentStock = source.CurrentStock,
            MinStock = source.MinStock,
            AlertThreshold = source.AlertThreshold,
            Unit = source.Unit,
            ProductImage = source.ProductImage,
            CreatedDate = source.CreatedDate,
            UpdatedDate = source.UpdatedDate,
            CreatedBy = source.CreatedBy,
            UpdatedBy = source.UpdatedBy,
            IsDeleted = source.IsDeleted
        };
    }
}
