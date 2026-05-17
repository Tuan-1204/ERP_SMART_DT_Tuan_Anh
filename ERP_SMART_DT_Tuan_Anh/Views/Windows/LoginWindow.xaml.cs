using System.Windows;
using System.Windows.Input;
using ERP_SMART_DT_Tuan_Anh.ViewModels;

namespace ERP_SMART_DT_Tuan_Anh.Views.Windows;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _viewModel = new();

    public LoginWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    private void Login_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.Password = PasswordBox.Password;

        if (!_viewModel.Login())
            return;

        var displayName = string.IsNullOrWhiteSpace(_viewModel.Username)
            ? "Người dùng"
            : _viewModel.Username.Trim();

        var mainWindow = new MainWindow(displayName);
        mainWindow.Show();
        Close();
    }

    private void Minimize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void Maximize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            Maximize_Click(sender, e);
            return;
        }

        DragMove();
    }
}
