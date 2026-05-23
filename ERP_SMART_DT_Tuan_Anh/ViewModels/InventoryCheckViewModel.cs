using System.Collections.ObjectModel;
using System.Windows.Input;
using ERP_SMART_DT_Tuan_Anh.Commands;
using ERP_SMART_DT_Tuan_Anh.Helpers;
using ERP_SMART_DT_Tuan_Anh.Models;
using ERP_SMART_DT_Tuan_Anh.Services;

namespace ERP_SMART_DT_Tuan_Anh.ViewModels;

public class InventoryCheckViewModel : BaseViewModel
{
    private readonly InventoryCheckService _service = new();
    private readonly ProductService _productService = new();

    private InventoryCheck? _selectedCheck;
    private InventoryCheckDetailItemViewModel? _selectedDetail;
    private Product? _selectedProduct;
    private string _note = string.Empty;
    private int _actualStock;

    public ObservableCollection<InventoryCheck> Checks { get; } = new();
    public ObservableCollection<Product> Products { get; } = new();
    public ObservableCollection<InventoryCheckDetailItemViewModel> Details { get; } = new();

    public InventoryCheck? SelectedCheck
    {
        get => _selectedCheck;
        set
        {
            if (SetProperty(ref _selectedCheck, value))
                LoadSelectedCheck();
        }
    }

    public InventoryCheckDetailItemViewModel? SelectedDetail
    {
        get => _selectedDetail;
        set => SetProperty(ref _selectedDetail, value);
    }

    public Product? SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            if (SetProperty(ref _selectedProduct, value) && value != null)
                ActualStock = value.CurrentStock;
        }
    }

    public string Note
    {
        get => _note;
        set => SetProperty(ref _note, value);
    }

    public int ActualStock
    {
        get => _actualStock;
        set => SetProperty(ref _actualStock, value < 0 ? 0 : value);
    }

    public string FormTitle => SelectedCheck == null ? "Tạo phiếu kiểm kê" : $"Sửa phiếu #{SelectedCheck.Id}";
    public string SaveButtonText => SelectedCheck == null ? "Tạo phiếu" : "Cập nhật phiếu";

    public ICommand LoadCommand { get; }
    public ICommand NewCommand { get; }
    public ICommand AddDetailCommand { get; }
    public ICommand RemoveDetailCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }

    public InventoryCheckViewModel()
    {
        LoadCommand = new AsyncRelayCommand(_ => LoadAsync());
        NewCommand = new RelayCommand(_ => ResetForm());
        AddDetailCommand = new RelayCommand(_ => AddDetail());
        RemoveDetailCommand = new RelayCommand(_ => RemoveDetail());
        SaveCommand = new AsyncRelayCommand(_ => SaveAsync());
        DeleteCommand = new AsyncRelayCommand(_ => DeleteAsync());

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        Products.Clear();
        foreach (var product in await _productService.GetAllAsync())
            Products.Add(product);

        await LoadChecksAsync();
    }

    private async Task LoadChecksAsync()
    {
        Checks.Clear();
        foreach (var item in await _service.GetAllAsync())
            Checks.Add(item);
    }

    private void LoadSelectedCheck()
    {
        Details.Clear();

        if (SelectedCheck == null)
        {
            Note = string.Empty;
        }
        else
        {
            Note = SelectedCheck.Note ?? string.Empty;

            foreach (var detail in SelectedCheck.InventoryCheckDetails)
                Details.Add(InventoryCheckDetailItemViewModel.FromDetail(detail));
        }

        SelectedDetail = null;
        OnPropertyChanged(nameof(FormTitle));
        OnPropertyChanged(nameof(SaveButtonText));
    }

    private void ResetForm()
    {
        SelectedCheck = null;
        SelectedDetail = null;
        SelectedProduct = null;
        ActualStock = 0;
        Note = string.Empty;
        Details.Clear();
        OnPropertyChanged(nameof(FormTitle));
        OnPropertyChanged(nameof(SaveButtonText));
    }

    private void AddDetail()
    {
        if (SelectedProduct == null)
        {
            MessageBoxHelper.Warning("Vui lòng chọn sản phẩm cần kiểm kê.");
            return;
        }

        if (Details.Any(x => x.ProductId == SelectedProduct.Id))
        {
            MessageBoxHelper.Warning("Sản phẩm này đã có trong phiếu kiểm kê.");
            return;
        }

        Details.Add(InventoryCheckDetailItemViewModel.FromProduct(SelectedProduct, ActualStock));
        SelectedProduct = null;
        ActualStock = 0;
    }

    private void RemoveDetail()
    {
        if (SelectedDetail == null)
        {
            MessageBoxHelper.Warning("Vui lòng chọn dòng sản phẩm cần xóa khỏi phiếu.");
            return;
        }

        Details.Remove(SelectedDetail);
        SelectedDetail = null;
    }

    private async Task SaveAsync()
    {
        if (Details.Count == 0)
        {
            MessageBoxHelper.Warning("Phiếu kiểm kê phải có ít nhất một sản phẩm cần kiểm.");
            return;
        }

        var details = Details.Select(x => new InventoryCheckDetail
        {
            ProductId = x.ProductId,
            SystemStock = x.SystemStock,
            ActualStock = x.ActualStock
        }).ToList();

        if (SelectedCheck == null)
        {
            await _service.AddWithDetailsAsync(new InventoryCheck
            {
                UserId = 1,
                Note = Note
            }, details);

            MessageBoxHelper.Info("Tạo phiếu kiểm kê thành công.");
        }
        else
        {
            await _service.UpdateWithDetailsAsync(SelectedCheck.Id, Note, details);
            MessageBoxHelper.Info("Cập nhật phiếu kiểm kê thành công.");
        }

        ResetForm();
        await LoadChecksAsync();
    }

    private async Task DeleteAsync()
    {
        if (SelectedCheck == null)
        {
            MessageBoxHelper.Warning("Vui lòng chọn phiếu kiểm kê cần xóa.");
            return;
        }

        if (!MessageBoxHelper.Confirm($"Bạn có chắc muốn xóa phiếu kiểm kê #{SelectedCheck.Id}?"))
            return;

        await _service.HardDeleteAsync(SelectedCheck.Id);
        MessageBoxHelper.Info("Đã xóa phiếu kiểm kê.");
        ResetForm();
        await LoadChecksAsync();
    }
}
