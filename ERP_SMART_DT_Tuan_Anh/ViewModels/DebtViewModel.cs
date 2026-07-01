using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using ERP_SMART_DT_Tuan_Anh.Helpers;
using ERP_SMART_DT_Tuan_Anh.Models;
using ERP_SMART_DT_Tuan_Anh.Services;

namespace ERP_SMART_DT_Tuan_Anh.ViewModels
{
    public class DebtViewModel : BaseViewModel
    {
        private readonly PartnerService _partnerService = new();
        private readonly DebtService _debtService = new();

        // Đã đồng bộ chính xác với ItemsSource và các Binding trong XAML
        public ObservableCollection<Partner> Partners { get; } = new();
        public ObservableCollection<DebtTransactionDto> Transactions { get; } = new();

        private Partner? _selectedPartner;
        private string _paymentAmount = string.Empty;
        private string _note = string.Empty;

        public Partner? SelectedPartner
        {
            get => _selectedPartner;
            set
            {
                if (SetProperty(ref _selectedPartner, value))
                {
                    // Khi thay đổi đối tác, lập tức cập nhật lại các trường text hiển thị và tải lịch sử
                    OnPropertyChanged(nameof(PartnerTypeText));
                    OnPropertyChanged(nameof(CurrentDebtText));
                    OnPropertyChanged(nameof(RemainingDebtText));
                    _ = LoadPartnerTransactionsAsync();
                }
            }
        }

        public string PaymentAmount
        {
            get => _paymentAmount;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    if (SetProperty(ref _paymentAmount, string.Empty))
                    {
                        OnPropertyChanged(nameof(RemainingDebtText));
                    }
                    return;
                }

                // Làm sạch các dấu phẩy phân cách cũ trước khi ép định dạng số học mới
                string clean = value.Replace(",", "").Replace(".", "").Trim();
                if (decimal.TryParse(clean, out decimal parsed))
                {
                    if (SetProperty(ref _paymentAmount, parsed.ToString("N0")))
                    {
                        OnPropertyChanged(nameof(RemainingDebtText));
                    }
                }
            }
        }

        public string Note
        {
            get => _note;
            set => SetProperty(ref _note, value);
        }

        // --- CÁC THUỘC TÍNH BINDING TÓM TẮT ĐỐI SOÁT TRỰC QUAN ---
        public string PartnerTypeText
        {
            get
            {
                if (SelectedPartner == null) return "Chưa chọn đối tác";
                return SelectedPartner.ObjectType == "SUPPLIER" ? "📥 Nhà cung cấp phân phối" : "📤 Khách hàng / Đại lý buôn";
            }
        }

        public string CurrentDebtText => SelectedPartner != null
            ? SelectedPartner.TotalDebt.ToString("N0") + " đ"
            : "0 đ";

        public string RemainingDebtText
        {
            get
            {
                if (SelectedPartner == null) return "0 đ";
                string clean = _paymentAmount.Replace(",", "").Replace(".", "").Trim();
                decimal.TryParse(clean, out decimal payment);

                decimal rem = SelectedPartner.TotalDebt - payment;
                return (rem > 0 ? rem : 0).ToString("N0") + " đ";
            }
        }

        public ICommand LoadCommand { get; }
        public ICommand PayDebtCommand { get; }

        public DebtViewModel()
        {
            LoadCommand = new AsyncRelayCommand(LoadDebtorsAsync);
            PayDebtCommand = new AsyncRelayCommand(ProcessDebtPaymentAsync);
            _ = LoadDebtorsAsync();
        }

        private async Task LoadDebtorsAsync()
        {
            if (IsBusy) return;
            try
            {
                IsBusy = true;
                Partners.Clear();

                // Gọi dịch vụ lấy danh sách tất cả đối tác đang có phát sinh công nợ
                var list = await _partnerService.GetAllPartnersWithDebtAsync();
                foreach (var partner in list)
                {
                    Partners.Add(partner);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Lỗi tải dữ liệu công nợ hệ thống: " + ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }


        private async Task LoadPartnerTransactionsAsync()
        {
            Transactions.Clear();
            if (SelectedPartner == null) return;

            try
            {
                // Gọi hàm nạp lịch sử chứng từ nợ của riêng đối tác đang chọn
                var history = await _debtService.GetTransactionsByPartnerAsync(SelectedPartner.Id);
                foreach (var tx in history.OrderByDescending(t => t.TransactionDate))
                {
                    Transactions.Add(new DebtTransactionDto
                    {
                        TransactionDate = tx.TransactionDate,
                        BillId = tx.BillId ?? "CT-TTOAN",
                        Type = tx.Type ?? "DEBT",

                 
                        Amount = tx.Amount ?? 0,

                        Note = tx.Note ?? string.Empty
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi nạp nhật ký công nợ: " + ex.Message);
            }
        }

        private async Task ProcessDebtPaymentAsync()
        {
            if (SelectedPartner == null)
            {
                MessageBoxHelper.Warning("Vui lòng chọn đối tác cần thực hiện thu/trả nợ gối đầu.");
                return;
            }

            string clean = _paymentAmount.Replace(",", "").Replace(".", "").Trim();
            if (!decimal.TryParse(clean, out decimal amount) || amount <= 0)
            {
                MessageBoxHelper.Warning("Số tiền thanh toán ghi sổ bắt buộc phải lớn hơn 0 VND!");
                return;
            }

            try
            {
                IsBusy = true;
                var result = await _debtService.ExecutePayDebtAsync(SelectedPartner.Id, amount, Note);

                if (result == "SUCCESS")
                {
                    MessageBoxHelper.Info($"Đã hạch toán chứng từ công nợ thành công! Số dư nợ của đối tác [{SelectedPartner.FullName}] đã được giảm trừ tự động.");

                    // Reset sạch form nhập liệu để sẵn sàng lập chứng từ tiếp theo
                    PaymentAmount = string.Empty;
                    Note = string.Empty;
                    SelectedPartner = null;
                    Transactions.Clear();

                    await LoadDebtorsAsync();
                }
                else
                {
                    MessageBoxHelper.Warning($"Lỗi chặn nghiệp vụ Database: {result}");
                }
            }
            catch (Exception ex)
            {
                MessageBoxHelper.Error("Sự cố đồng bộ luồng ghi sổ: " + ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }


    public class DebtTransactionDto
    {
        public DateTime TransactionDate { get; set; }
        public string BillId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Note { get; set; } = string.Empty;
        public string TransactionTypeText => Type == "DEBT" ? "🚨 Phát sinh nợ mới" : "💳 Tất toán trả nợ";
    }
}