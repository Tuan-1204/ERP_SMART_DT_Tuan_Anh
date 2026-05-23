using ERP_SMART_DT_Tuan_Anh.Models;

namespace ERP_SMART_DT_Tuan_Anh.ViewModels;

public class InventoryCheckDetailItemViewModel : BaseViewModel
{
    private int _actualStock;

    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int SystemStock { get; set; }

    public int ActualStock
    {
        get => _actualStock;
        set
        {
            if (SetProperty(ref _actualStock, value))
                OnPropertyChanged(nameof(Difference));
        }
    }

    public int Difference => ActualStock - SystemStock;

    public static InventoryCheckDetailItemViewModel FromProduct(Product product, int actualStock)
    {
        return new InventoryCheckDetailItemViewModel
        {
            ProductId = product.Id,
            ProductCode = product.ProductCode,
            ProductName = product.ProductName,
            SystemStock = product.CurrentStock,
            ActualStock = actualStock
        };
    }

    public static InventoryCheckDetailItemViewModel FromDetail(InventoryCheckDetail detail)
    {
        return new InventoryCheckDetailItemViewModel
        {
            ProductId = detail.ProductId ?? 0,
            ProductCode = detail.Product?.ProductCode ?? string.Empty,
            ProductName = detail.Product?.ProductName ?? string.Empty,
            SystemStock = detail.SystemStock ?? 0,
            ActualStock = detail.ActualStock ?? 0
        };
    }
}
