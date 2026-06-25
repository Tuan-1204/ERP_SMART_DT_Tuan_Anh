using System;
using System.Collections.ObjectModel;
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

        public ObservableCollection<Partner> DebtorsList { get; } = new();

        private Partner? _selectedPartner;
        private string _paymentAmountText = string.Empty;
        private string _transactionNote = string.Empty;

        public Partner? SelectedPartner
        {
            get => _selectedPartner;
            set => SetProperty(ref _selectedPartner, value);
        }

        public string PaymentAmountText
        {
            get => _paymentAmountText;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    SetProperty(ref _paymentAmountText, value);
                    return;
                }
                string clean = value.Replace(",", "").Replace(".", "").Trim();
                if (decimal.TryParse(clean, out decimal parsed))
                {
                    SetProperty(ref _paymentAmountText, parsed.ToString("N0"));
                }
            }
        }

        public string TransactionNote
        {
            get => _transactionNote;
            set => SetProperty(ref _transactionNote, value);
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
                DebtorsList.Clear();
                var list = await _partnerService.GetAllPartnersWithDebtAsync();
                foreach (var partner in list)
                {
                    DebtorsList.Add(partner);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Lỗi tải dữ liệu công nợ: " + ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ProcessDebtPaymentAsync()
        {
            if (SelectedPartner == null)
            {
                MessageBoxHelper.Warning("Vui lòng chọn đối tác cần thu/trả nợ.");
                return;
            }

            decimal amount = decimal.Parse(string.IsNullOrWhiteSpace(PaymentAmountText) ? "0" : PaymentAmountText.Replace(",", "").Replace(".", ""));
            if (amount <= 0 || amount > SelectedPartner.TotalDebt)
            {
                MessageBoxHelper.Warning("Số tiền thanh toán phải lớn hơn 0 và nhỏ hơn tổng dư nợ hiện tại.");
                return;
            }

            try
            {
                var result = await _debtService.ExecutePayDebtAsync(SelectedPartner.Id, amount, TransactionNote);
                if (result == "SUCCESS")
                {
                    MessageBoxHelper.Info("Đã lập chứng từ và tất toán số dư nợ thành công!");
                    PaymentAmountText = string.Empty;
                    TransactionNote = string.Empty;
                    SelectedPartner = null;
                    await LoadDebtorsAsync();
                }
                else
                {
                    MessageBoxHelper.Warning(result);
                }
            }
            catch (Exception ex)
            {
                MessageBoxHelper.Error("Sự cố hệ thống: " + ex.Message);
            }
        }
    }
}