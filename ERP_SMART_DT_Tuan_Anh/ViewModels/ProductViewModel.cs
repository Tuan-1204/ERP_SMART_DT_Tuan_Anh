using System.Collections.ObjectModel;
using System.Windows.Input;
using ERP_SMART_DT_Tuan_Anh.Commands;
using ERP_SMART_DT_Tuan_Anh.Models;
using ERP_SMART_DT_Tuan_Anh.Services;

namespace ERP_SMART_DT_Tuan_Anh.ViewModels;

public class ProductViewModel : BaseViewModel
{
    private readonly ProductService _productService = new();
    private readonly CategoryService _categoryService = new();

    public ObservableCollection<Product> Products { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();

    private Product _currentProduct = new();

    public Product CurrentProduct
    {
        get => _currentProduct;
        set => SetProperty(ref _currentProduct, value);
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
        NewCommand = new RelayCommand(_ => CurrentProduct = new Product
        {
            MinStock = 5,
            AlertThreshold = 10,
            Unit = "Chiếc"
        });

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        Products.Clear();
        Categories.Clear();

        foreach (var item in await _productService.GetAllAsync())
            Products.Add(item);

        foreach (var item in await _categoryService.GetAllAsync())
            Categories.Add(item);
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentProduct.ProductCode) ||
            string.IsNullOrWhiteSpace(CurrentProduct.ProductName))
            return;

        if (CurrentProduct.Id == 0)
            await _productService.AddAsync(CurrentProduct);
        else
            await _productService.UpdateAsync(CurrentProduct);

        await LoadAsync();
        CurrentProduct = new Product { MinStock = 5, AlertThreshold = 10, Unit = "Chiếc" };
    }

    private async Task DeleteAsync()
    {
        if (CurrentProduct.Id == 0)
            return;

        await _productService.SoftDeleteAsync(CurrentProduct.Id);
        await LoadAsync();
        CurrentProduct = new Product { MinStock = 5, AlertThreshold = 10, Unit = "Chiếc" };
    }
}