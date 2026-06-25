using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ERP_SMART_DT_Tuan_Anh.DTOs;
using ERP_SMART_DT_Tuan_Anh.Services;

namespace ERP_SMART_DT_Tuan_Anh.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly DashboardService _dashboardService = new();

        // Tự động sinh ra thuộc tính public 'Summary' chuẩn MVVM
        [ObservableProperty]
        private DashboardSummaryDto _summary = new();

        // Trạng thái hiển thị vòng xoay Loading màn hình ngầm
        [ObservableProperty]
        private bool _isBusy;

        // Lưu thông báo lỗi nếu gặp sự cố kết nối
        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public DashboardViewModel()
        {
            // Nạp dữ liệu tự động ngay khi màn hình Dashboard được khởi tạo
            _ = LoadDataAsync();
        }

        // Tạo ra LoadDataCommand liên kết thẳng tới nút "Làm mới" trên giao diện XAML
        [RelayCommand]
        public async Task LoadDataAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                // Gọi tầng dịch vụ để nạp dữ liệu Multi-Result Set mới nhất
                Summary = await _dashboardService.GetSummaryAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Không thể làm mới dữ liệu tổng quan: " + ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}