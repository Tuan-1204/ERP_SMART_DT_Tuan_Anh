using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using ERP_SMART_DT_Tuan_Anh.Models;
using ERP_SMART_DT_Tuan_Anh.Services;

namespace ERP_SMART_DT_Tuan_Anh.ViewModels
{
    public class ImeiInventoryViewModel : BaseViewModel
    {
        private readonly ImeiService _imeiService = new();

        public ObservableCollection<ImeiInventory> ImeiList { get; } = new();

        private string _keyword = string.Empty;
        private ImeiInventory? _selectedImei;

        // Bổ sung cơ chế trễ Delay để gõ tìm kiếm mượt mà
        public string Keyword
        {
            get => _keyword;
            set
            {
                if (SetProperty(ref _keyword, value) && string.IsNullOrWhiteSpace(value))
                {
                    _ = LoadAsync(); // Xóa trắng ô tìm kiếm thì tự động nạp lại toàn bộ
                }
            }
        }

        public ImeiInventory? SelectedImei
        {
            get => _selectedImei;
            set => SetProperty(ref _selectedImei, value);
        }

        public ICommand LoadCommand { get; }
        public ICommand SearchCommand { get; }

        public ImeiInventoryViewModel()
        {
            LoadCommand = new AsyncRelayCommand(LoadAsync);
            SearchCommand = new AsyncRelayCommand(SearchAsync);
            _ = LoadAsync();
        }

        public async Task LoadAsync()
        {
            if (IsBusy) return;
            try
            {
                IsBusy = true;
                ImeiList.Clear();

                // LƯU Ý: Tầng Service hàm GetAllAsync() bắt buộc phải .Include(p => p.Product)
                var list = await _imeiService.GetAllAsync();
                foreach (var item in list)
                {
                    ImeiList.Add(item);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Không thể nạp danh sách thiết bị: " + ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SearchAsync()
        {
            string searchKey = Keyword.Trim();
            if (string.IsNullOrWhiteSpace(searchKey))
            {
                await LoadAsync();
                return;
            }

            try
            {
                IsBusy = true;
                ImeiList.Clear();

                //Lấy toàn bộ, sau đó lọc tương đối (Contains) theo cả mã IMEI, tên sản phẩm hoặc mã máy
                var allImeis = await _imeiService.GetAllAsync();
                var filtered = allImeis.Where(x =>
                    x.Imei.Contains(searchKey, StringComparison.OrdinalIgnoreCase) ||
                    (x.Product != null && x.Product.ProductName.Contains(searchKey, StringComparison.OrdinalIgnoreCase)) ||
                    (x.Product != null && x.Product.ProductCode.Contains(searchKey, StringComparison.OrdinalIgnoreCase))
                );

                foreach (var item in filtered)
                {
                    ImeiList.Add(item);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Lỗi truy vấn tìm kiếm: " + ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}