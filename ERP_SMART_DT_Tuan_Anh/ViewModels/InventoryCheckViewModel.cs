using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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

        public ObservableCollection<ProductCheckModel> AuditItemsList { get; } = new();

        private string _checkNote = string.Empty;

        public string CheckNote
        {
            get => _checkNote;
            set => SetProperty(ref _checkNote, value);
        }

        public ICommand SaveCommand { get; }

        public InventoryCheckViewModel()
        {
            SaveCommand = new AsyncRelayCommand(SaveCheckSheetAsync);
            _ = InitializeCheckSheetAsync();
        }

        private async Task InitializeCheckSheetAsync()
        {
            AuditItemsList.Clear();
            var products = await _productService.GetAllAsync();
            foreach (var p in products)
            {
                AuditItemsList.Add(new ProductCheckModel(CalculateDifference)
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

        private void CalculateDifference()
        {
            // Hàm callback rỗng phục vụ kích hoạt tính toán đổi số liệu từ Model con
        }

        private async Task SaveCheckSheetAsync()
        {
            if (AuditItemsList.Count == 0 || IsBusy) return;

            try
            {
                IsBusy = true;
                var checkMaster = new InventoryCheckMasterDto
                {
                    UserId = 1,
                    Note = CheckNote,
                    Details = AuditItemsList.Select(x => new InventoryCheckDetailDto
                    {
                        ProductId = x.ProductId,
                        SystemStock = x.SystemStock,
                        ActualStock = x.ActualStock
                    }).ToList()
                };

                bool isSaved = await _checkService.CreateCheckSheetAsync(checkMaster);
                if (isSaved)
                {
                    MessageBoxHelper.Info("Lưu biên bản kiểm kê kho thành công!");
                    CheckNote = string.Empty;
                    await InitializeCheckSheetAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBoxHelper.Error("Lỗi hệ thống: " + ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    public class ProductCheckModel : BaseViewModel
    {
        private readonly Action _onChangedCallback;
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
                    _onChangedCallback?.Invoke();
                }
            }
        }

        public int Difference
        {
            get => _difference;
            set => SetProperty(ref _difference, value);
        }

        public ProductCheckModel(Action onChangedCallback)
        {
            _onChangedCallback = onChangedCallback;
        }
    }
}