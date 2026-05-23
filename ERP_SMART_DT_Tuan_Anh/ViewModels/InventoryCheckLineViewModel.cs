using ERP_SMART_DT_Tuan_Anh.Models;

namespace ERP_SMART_DT_Tuan_Anh.ViewModels;

public class InventoryCheckLineViewModel : BaseViewModel
{
    private Product? _product;
    private int _systemStock;
    private int _actualStock;

    public Product? Product
    {
        get => _product;
        set
        {
            if (!SetProperty(ref _product, value))
                return;

            SystemStock = value?.CurrentStock ?? 0;
            ActualStock = SystemStock;
            OnPropertyChanged(nameof(ProductCode));
            OnPropertyChanged(nameof(ProductName));
        }
    }

    public int SystemStock
    {
        get => _systemStock;
        set
        {
            if (SetProperty(ref _systemStock, value))
                OnPropertyChanged(nameof(Difference));
        }
    }

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

    public string ProductCode => Product?.ProductCode ?? string.Empty;

    public string ProductName => Product?.ProductName ?? string.Empty;
}
