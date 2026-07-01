using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using ERP_SMART_DT_Tuan_Anh.Helpers;
using ERP_SMART_DT_Tuan_Anh.DTOs;
using ERP_SMART_DT_Tuan_Anh.Models;
using ERP_SMART_DT_Tuan_Anh.Services;

namespace ERP_SMART_DT_Tuan_Anh.ViewModels
{
    public class InventoryCheckViewModel : BaseViewModel
    {
        private readonly ProductService _productService = new();
        private readonly InventoryCheckService _checkService = new();

        public ObservableCollection<InventoryCheckDetailItemViewModel> Checks { get; } = new();
        public ObservableCollection<ProductCheckModel> Details { get; } = new();

        private InventoryCheckDetailItemViewModel? _selectedCheck;
        private string _note = string.Empty;
        private string _formTitle = "Lập biên bản kiểm kê";
        private bool _isReadOnlyGrid = false;

        public InventoryCheckDetailItemViewModel? SelectedCheck
        {
            get => _selectedCheck;
            set
            {
                if (SetProperty(ref _selectedCheck, value) && value != null)
                {
                    IsReadOnlyGrid = true;
                    FormTitle = "Chi tiết biên bản kiểm kê";
                    _ = LoadCheckSheetDetailsAsync(value.ProductId);
                }
            }
        }

        public string Note
        {
            get => _note;
            set => SetProperty(ref _note, value);
        }

        public string FormTitle
        {
            get => _formTitle;
            set => SetProperty(ref _formTitle, value);
        }

        public bool IsReadOnlyGrid
        {
            get => _isReadOnlyGrid;
            set
            {
                if (SetProperty(ref _isReadOnlyGrid, value))
                {
                    OnPropertyChanged(nameof(SystemModeText));
                    OnPropertyChanged(nameof(FormVisibility));
                    OnPropertyChanged(nameof(HistoryMessageVisibility));
                }
            }
        }

        public string SystemModeText => IsReadOnlyGrid ? "CHẾ ĐỘ LỊCH SỬ (READ-ONLY)" : "CHẾ ĐỘ LẬP PHIẾU MỚI (EDITABLE)";
        public Visibility FormVisibility => IsReadOnlyGrid ? Visibility.Collapsed : Visibility.Visible;
        public Visibility HistoryMessageVisibility => IsReadOnlyGrid ? Visibility.Visible : Visibility.Collapsed;

        public ICommand LoadCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand NewCommand { get; }

        public InventoryCheckViewModel()
        {
            LoadCommand = new AsyncRelayCommand(LoadHistorySheetsAsync);
            SaveCommand = new AsyncRelayCommand(SaveCheckSheetAsync);
            NewCommand = new RelayCommand(ExecuteNewForm);

            _ = LoadHistorySheetsAsync();
            ExecuteNewForm();
        }

        private async Task LoadHistorySheetsAsync()
        {
            if (IsBusy) return;
            try
            {
                IsBusy = true;
                Checks.Clear();

                var products = await _productService.GetAllAsync();
                foreach (var p in products)
                {
                    Checks.Add(InventoryCheckDetailItemViewModel.FromProduct(p, p.CurrentStock));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi nạp lịch sử kiểm kê: " + ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadCheckSheetDetailsAsync(int productId)
        {
            Details.Clear();
            try
            {
                var products = await _productService.GetAllAsync();
                var target = products.FirstOrDefault(p => p.Id == productId);
                if (target != null)
                {
                    Details.Add(new ProductCheckModel
                    {
                        ProductId = target.Id,
                        ProductCode = target.ProductCode,
                        ProductName = target.ProductName,
                        SystemStock = target.CurrentStock,
                        ActualStock = target.CurrentStock,
                        Difference = 0
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi tải chi tiết: " + ex.Message);
            }
        }

        private void ExecuteNewForm()
        {
            _selectedCheck = null;
            OnPropertyChanged(nameof(SelectedCheck));

            IsReadOnlyGrid = false;
            Note = string.Empty;
            FormTitle = "Lập biên bản kiểm kê mới";

            _ = InitializeNewCheckSheetAsync();
        }

        private async Task InitializeNewCheckSheetAsync()
        {
            Details.Clear();
            try
            {
                var products = await _productService.GetAllAsync();
                foreach (var p in products)
                {
                    Details.Add(new ProductCheckModel
                    {
                        ProductId = p.Id,
                        ProductCode = p.ProductCode,
                        ProductName = p.ProductName,
                        SystemStock = p.CurrentStock,
                        ActualStock = p.CurrentStock,
                        Difference = 0
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi khởi tạo danh sách máy: " + ex.Message);
            }
        }

        private async Task SaveCheckSheetAsync()
        {
            if (Details.Count == 0 || IsBusy) return;

            try
            {
                IsBusy = true;
                var checkMaster = new InventoryCheckMasterDto
                {
                    UserId = 1,
                    Note = Note,
                    Details = Details.Select(x => new InventoryCheckDetailDto
                    {
                        ProductId = x.ProductId,
                        SystemStock = x.SystemStock,
                        ActualStock = x.ActualStock
                    }).ToList()
                };

                bool isSaved = await _checkService.CreateCheckSheetAsync(checkMaster);
                if (isSaved)
                {
                    MessageBoxHelper.Info("Hệ thống hạch toán thành công! Biên bản kiểm kê đã được lưu.");
                    ExecuteNewForm();
                    await LoadHistorySheetsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBoxHelper.Error("Lỗi lưu biên bản kiểm kê: " + ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    public class ProductCheckModel : BaseViewModel
    {
        private int _actualStock;
        private int _difference;

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
                {
                    Difference = _actualStock - SystemStock;
                }
            }
        }

        public int Difference
        {
            get => _difference;
            set
            {
                if (SetProperty(ref _difference, value))
                {
                    OnPropertyChanged(nameof(IsDeficit));
                    OnPropertyChanged(nameof(IsSurplus));
                }
            }
        }

        public bool IsDeficit => Difference < 0;
        public bool IsSurplus => Difference > 0;
    }
}