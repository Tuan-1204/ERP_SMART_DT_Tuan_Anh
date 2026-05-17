using System.Collections.ObjectModel;
using System.Windows.Input;
using ERP_SMART_DT_Tuan_Anh.Commands;
using ERP_SMART_DT_Tuan_Anh.Helpers;
using ERP_SMART_DT_Tuan_Anh.Models;
using ERP_SMART_DT_Tuan_Anh.Services;

namespace ERP_SMART_DT_Tuan_Anh.ViewModels;

public class CategoryViewModel : BaseViewModel
{
    private readonly CategoryService _service = new();

    public ObservableCollection<Category> Categories { get; } = new();

    private Category _currentCategory = new();

    public Category CurrentCategory
    {
        get => _currentCategory;
        set => SetProperty(ref _currentCategory, value);
    }

    public ICommand NewCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand LoadCommand { get; }

    public CategoryViewModel()
    {
        NewCommand = new RelayCommand(_ => CurrentCategory = new Category());
        SaveCommand = new AsyncRelayCommand(_ => SaveAsync());
        DeleteCommand = new AsyncRelayCommand(_ => DeleteAsync());
        LoadCommand = new AsyncRelayCommand(_ => LoadAsync());

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        Categories.Clear();
        foreach (var item in await _service.GetAllAsync())
            Categories.Add(item);
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentCategory.CategoryName))
        {
            MessageBoxHelper.Warning("Vui lòng nhập tên danh mục.");
            return;
        }

        if (CurrentCategory.Id == 0)
            await _service.AddAsync(CurrentCategory);
        else
            await _service.UpdateAsync(CurrentCategory);

        MessageBoxHelper.Info("Lưu danh mục thành công.");
        CurrentCategory = new Category();
        await LoadAsync();
    }

    private async Task DeleteAsync()
    {
        if (CurrentCategory.Id == 0)
            return;

        if (!MessageBoxHelper.Confirm("Bạn có chắc muốn xóa mềm danh mục này?"))
            return;

        await _service.SoftDeleteAsync(CurrentCategory.Id);
        CurrentCategory = new Category();
        await LoadAsync();
    }
}