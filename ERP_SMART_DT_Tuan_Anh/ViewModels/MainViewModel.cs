using System.Windows.Controls;
using System.Windows.Input;
using ERP_SMART_DT_Tuan_Anh.Commands;
using ERP_SMART_DT_Tuan_Anh.Helpers;
using ERP_SMART_DT_Tuan_Anh.Views.Page;

namespace ERP_SMART_DT_Tuan_Anh.ViewModels;

public class MainViewModel : BaseViewModel
{
    private UserControl _currentView;
    private string _pageTitle;
    private string _breadcrumb;
    private string _displayName;

    public MainViewModel(string? displayName = null)
    {
        _currentView = new DashboardView();
        _pageTitle = "Tổng quan";
        _breadcrumb = "Trang chủ / Tổng quan";
        _displayName = string.IsNullOrWhiteSpace(displayName) ? "Người dùng" : displayName;

        ShowDashboardCommand = new RelayCommand(_ => Navigate(new DashboardView(), "Tổng quan", "Trang chủ / Tổng quan"));
        ShowProductCommand = new RelayCommand(_ => Navigate(new ProductView(), "Quản lý sản phẩm", "Trang chủ / Kho hàng / Sản phẩm"));
        ShowCategoryCommand = new RelayCommand(_ => Navigate(new CategoryView(), "Danh mục sản phẩm", "Trang chủ / Kho hàng / Danh mục"));
        ShowImeiCommand = new RelayCommand(_ => Navigate(new ImeiInventoryView(), "Quản lý IMEI", "Trang chủ / Kho hàng / IMEI"));
        ShowImportCommand = new RelayCommand(_ => Navigate(new ImportStockView(), "Nhập kho", "Trang chủ / Kho hàng / Nhập kho"));
        ShowExportCommand = new RelayCommand(_ => Navigate(new ExportStockView(), "Xuất kho", "Trang chủ / Bán hàng / Xuất kho"));
        ShowCustomerCommand = new RelayCommand(_ => Navigate(new CustomerView(), "Khách hàng", "Trang chủ / Đối tác / Khách hàng"));
        ShowSupplierCommand = new RelayCommand(_ => Navigate(new SupplierView(), "Nhà cung cấp", "Trang chủ / Đối tác / Nhà cung cấp"));
        ShowDebtCommand = new RelayCommand(_ => Navigate(new DebtView(), "Công nợ", "Trang chủ / Tài chính / Công nợ"));
        ShowForecastCommand = new RelayCommand(_ => Navigate(new StockForecastView(), "Dự đoán tồn kho", "Trang chủ / Kho hàng / Dự đoán tồn kho"));
        ShowAuditCommand = new RelayCommand(_ => Navigate(new AuditLogView(), "Nhật ký hệ thống", "Trang chủ / Hệ thống / Nhật ký"));
        ShowSettingCommand = new RelayCommand(_ => Navigate(new SettingView(), "Cài đặt", "Trang chủ / Hệ thống / Cài đặt"));
        ShowNotificationCommand = new RelayCommand(_ => MessageBoxHelper.Info("Hiện chưa có thông báo mới.", "Thông báo"));
        ShowAccountCommand = new RelayCommand(_ => MessageBoxHelper.Info($"Tài khoản đang đăng nhập: {DisplayName}", "Tài khoản"));
    }

    public UserControl CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }

    public string PageTitle
    {
        get => _pageTitle;
        set => SetProperty(ref _pageTitle, value);
    }

    public string Breadcrumb
    {
        get => _breadcrumb;
        set => SetProperty(ref _breadcrumb, value);
    }

    public string DisplayName
    {
        get => _displayName;
        set => SetProperty(ref _displayName, value);
    }

    public ICommand ShowDashboardCommand { get; }
    public ICommand ShowProductCommand { get; }
    public ICommand ShowCategoryCommand { get; }
    public ICommand ShowImeiCommand { get; }
    public ICommand ShowImportCommand { get; }
    public ICommand ShowExportCommand { get; }
    public ICommand ShowCustomerCommand { get; }
    public ICommand ShowSupplierCommand { get; }
    public ICommand ShowDebtCommand { get; }
    public ICommand ShowForecastCommand { get; }
    public ICommand ShowAuditCommand { get; }
    public ICommand ShowSettingCommand { get; }
    public ICommand ShowNotificationCommand { get; }
    public ICommand ShowAccountCommand { get; }

    private void Navigate(UserControl view, string title, string breadcrumb)
    {
        CurrentView = view;
        PageTitle = title;
        Breadcrumb = breadcrumb;
    }
}
