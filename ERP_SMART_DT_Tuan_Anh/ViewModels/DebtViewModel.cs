using System.Collections.ObjectModel;
using System.Windows.Input;
using ERP_SMART_DT_Tuan_Anh.Commands;
using ERP_SMART_DT_Tuan_Anh.DTOs;
using ERP_SMART_DT_Tuan_Anh.Helpers;
using ERP_SMART_DT_Tuan_Anh.Models;
using ERP_SMART_DT_Tuan_Anh.Services;

namespace ERP_SMART_DT_Tuan_Anh.ViewModels;

public class DebtViewModel : BaseViewModel
{
    private readonly PartnerService _partnerService = new();
    private readonly DebtService _debtService = new();

    public ObservableCollection<Partner> Partners { get; } = new();
    public ObservableCollection<DebtTransaction> Transactions { get; } = new();

    private Partner? _selectedPartner;
    private decimal _paymentAmount;
    private string _note = string.Empty;

    public Partner? SelectedPartner
    {
        get => _selectedPartner;
        set
        {
            if (SetProperty(ref _selectedPartner, value))
                _ = LoadTransactionsAsync();
        }
    }

    public decimal PaymentAmount
    {
        get => _paymentAmount;
        set => SetProperty(ref _paymentAmount, value);
    }

    public string Note
    {
        get => _note;
        set => SetProperty(ref _note, value);
    }

    public ICommand PayDebtCommand { get; }
    public ICommand LoadCommand { get; }

    public DebtViewModel()
    {
        PayDebtCommand = new AsyncRelayCommand(_ => PayDebtAsync());
        LoadCommand = new AsyncRelayCommand(_ => LoadAsync());
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        Partners.Clear();

        foreach (var item in await _partnerService.GetCustomersAsync())
            Partners.Add(item);

        foreach (var item in await _partnerService.GetSuppliersAsync())
            Partners.Add(item);
    }

    private async Task LoadTransactionsAsync()
    {
        Transactions.Clear();

        if (SelectedPartner == null)
            return;

        foreach (var item in await _debtService.GetDebtTransactionsAsync(SelectedPartner.Id))
            Transactions.Add(item);
    }

    private async Task PayDebtAsync()
    {
        if (SelectedPartner == null)
        {
            MessageBoxHelper.Warning("Vui lòng chọn đối tác.");
            return;
        }

        if (PaymentAmount <= 0)
        {
            MessageBoxHelper.Warning("Số tiền thanh toán phải lớn hơn 0.");
            return;
        }

        var result = await _debtService.PayDebtAsync(new DebtPaymentRequestDto
        {
            ObjectId = SelectedPartner.Id,
            Amount = PaymentAmount,
            Note = Note
        });

        if (result == "SUCCESS")
        {
            MessageBoxHelper.Info("Thanh toán công nợ thành công.");
            PaymentAmount = 0;
            Note = string.Empty;
            await LoadAsync();
            await LoadTransactionsAsync();
        }
        else
        {
            MessageBoxHelper.Warning(result);
        }
    }
}