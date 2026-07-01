using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using ERP_SMART_DT_Tuan_Anh.DTOs;
using ERP_SMART_DT_Tuan_Anh.Helpers;
using ERP_SMART_DT_Tuan_Anh.Services;

namespace ERP_SMART_DT_Tuan_Anh.ViewModels
{
    public class StockForecastViewModel : BaseViewModel
    {
        private readonly StockForecastService _service = new();
        private int _analysisDays = 30;
        private int _leadTimeDays = 5;
        private int _safetyDays = 3;
        private bool _isLoading;

        public ObservableCollection<StockForecastResultDto> Results { get; } = new();

        public int AnalysisDays
        {
            get => _analysisDays;
            set
            {
                if (SetProperty(ref _analysisDays, value))
                    OnPropertyChanged(nameof(StatusMessage));
            }
        }

        public int LeadTimeDays
        {
            get => _leadTimeDays;
            set => SetProperty(ref _leadTimeDays, value);
        }

        public int SafetyDays
        {
            get => _safetyDays;
            set => SetProperty(ref _safetyDays, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                    OnPropertyChanged(nameof(StatusMessage));
            }
        }

        public string StatusMessage => IsLoading ? "Hệ thống đang chạy phân tích dữ liệu..." : $"Dự báo dựa trên lịch sử {AnalysisDays} ngày gần nhất";

        public ICommand ForecastCommand { get; }

        public StockForecastViewModel()
        {
            ForecastCommand = new AsyncRelayCommand(ForecastAsync);
            _ = ForecastAsync(); // Tự động load dữ liệu dự báo ngay khi bật màn hình
        }

        private async Task ForecastAsync()
        {
            if (AnalysisDays <= 0 || LeadTimeDays < 0 || SafetyDays < 0)
            {
                MessageBoxHelper.Warning("Vui lòng cấu hình các thông số ngày lớn hơn hoặc bằng 0!");
                return;
            }

            try
            {
                IsLoading = true;
                Results.Clear();

                var request = new StockForecastRequestDto
                {
                    AnalysisDays = AnalysisDays,
                    LeadTimeDays = LeadTimeDays,
                    SafetyDays = SafetyDays
                };

                // Gọi xuống lớp Service để thực thi Stored Procedure sp_GetStockForecast
                var data = await _service.ForecastAsync(request);

                foreach (var item in data)
                {
                    Results.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBoxHelper.Error($"Lỗi hệ thống khi dự đoán: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}