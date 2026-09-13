using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using salary.DataLayer.Models;
using salary.DataLayer.Repo;
using salary.SharedLayer;
using System.Text.RegularExpressions;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace salary_WinUI.Views
{
    public sealed partial class salary : Page
    {
        private readonly salaryRepo _salaryRepo;

        private SalaryDto? _lastCalculatedResult;

        public salary()
        {
            this.InitializeComponent();

            IConfiguration configuration =
                new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json")
                    .Build();

            string connectionString =
                configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException(
                        "Missing 'DefaultConnection' in appsettings.json");

            var options =
                new DbContextOptionsBuilder<salaryDbContext>()
                    .UseSqlServer(connectionString)
                    .Options;

            var context = new salaryDbContext(options);

            _salaryRepo = new salaryRepo(context);

            Loaded += Salary_Loaded;
        }


        private async void Salary_Loaded(object sender,RoutedEventArgs e)
        {
            await LoadEmployeesAsync();
        }


        private async Task LoadEmployeesAsync()
        {
            var employees = await _salaryRepo.GetAllEmployeesAsync();

            EmployeeListView.ItemsSource = employees;
        }

        private async void CalculateButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!await ValidateInputsAsync())
            {
                return;
            }

            string name = NameTextBox.Text;

            if (!decimal.TryParse(
                GrossSalaryTextBox.Text,
                out decimal gross))
            {
                return;
            }

            var result =
                await _salaryRepo.CalculateSalaryAsync(
                    name,
                    gross);

            TaxTextBox.Text =
                result.Tax.ToString("0.00");

            PensionTextBox.Text =
                result.Pension.ToString("0.00");

            NetSalaryTextBox.Text =
                result.Salary.ToString("0.00");

            _lastCalculatedResult = result;
        }

        private async void SaveButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!await ValidateInputsAsync())
            {
                return;
            }

            if (_lastCalculatedResult == null)
            {
                await ShowMessageAsync(
                    "Please click Calculate before saving.");
                return;
            }

            await _salaryRepo.SaveEmployeeAsync(_lastCalculatedResult);

            await LoadEmployeesAsync();

            await ShowMessageAsync(
                "Salary saved successfully.");
        }

        private async Task ShowMessageAsync(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Salary",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }

        private async Task<bool> ValidateInputsAsync()
        {
            var errors = new List<string>();

            string name = NameTextBox.Text;

            if (string.IsNullOrWhiteSpace(name))
            {
                SetErrorBorder(NameTextBox);
                errors.Add("Name is required.");
            }
            else if (name.Length > 50)
            {
                SetErrorBorder(NameTextBox);
                errors.Add("Name cannot be more than 50 characters.");
            }
            else if (!Regex.IsMatch(name, "^[a-zA-Z ]+$"))
            {
                SetErrorBorder(NameTextBox);
                errors.Add("Name can only contain alphabets and spaces.");
            }
            else
            {
                ClearErrorBorder(NameTextBox);
            }

            string grossText = GrossSalaryTextBox.Text;

            if (string.IsNullOrWhiteSpace(grossText))
            {
                SetErrorBorder(GrossSalaryTextBox);
                errors.Add("Gross Salary is required.");
            }
            else if (!decimal.TryParse(grossText, out decimal gross))
            {
                SetErrorBorder(GrossSalaryTextBox);
                errors.Add("Gross Salary must be a valid number.");
            }
            else if (gross <= 0)
            {
                SetErrorBorder(GrossSalaryTextBox);
                errors.Add("Gross Salary must be greater than 0.");
            }
            else
            {
                ClearErrorBorder(GrossSalaryTextBox);
            }

            if (errors.Count > 0)
            {
                await ShowMessageAsync(string.Join("\n", errors));
                return false;
            }

            return true;
        }

        private void SetErrorBorder(TextBox textBox)
        {
            textBox.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Red);
            textBox.BorderThickness = new Thickness(2);
        }

        private void ClearErrorBorder(TextBox textBox)
        {
            textBox.ClearValue(Control.BorderBrushProperty);
            textBox.ClearValue(Control.BorderThicknessProperty);
        }
    }
}
