using System.Windows.Controls;
using ERP_SMART_DT_Tuan_Anh.ViewModels;

namespace ERP_SMART_DT_Tuan_Anh.Views.Page;

public partial class WarrantyView : UserControl
{
    public WarrantyView()
    {
        InitializeComponent();
        DataContext = new WarrantyViewModel();
    }
}