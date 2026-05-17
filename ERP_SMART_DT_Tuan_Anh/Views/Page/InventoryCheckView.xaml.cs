using System.Windows.Controls;
using ERP_SMART_DT_Tuan_Anh.ViewModels;

namespace ERP_SMART_DT_Tuan_Anh.Views.Page;

public partial class InventoryCheckView : UserControl
{
    public InventoryCheckView()
    {
        InitializeComponent();
        DataContext = new InventoryCheckViewModel();
    }
}