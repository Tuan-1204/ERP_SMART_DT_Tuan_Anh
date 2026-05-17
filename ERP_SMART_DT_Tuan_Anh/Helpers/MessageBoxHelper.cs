using System.Windows;

namespace ERP_SMART_DT_Tuan_Anh.Helpers;

public static class MessageBoxHelper
{
    public static void Info(string message, string title = "Thông báo")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public static void Warning(string message, string title = "Cảnh báo")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    public static void Error(string message, string title = "Lỗi")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public static bool Confirm(string message, string title = "Xác nhận")
    {
        return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }
}