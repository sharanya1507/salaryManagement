using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using salary.DataLayer.Models;
using salary.DataLayer.Repo;
using salary.SharedLayer;

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
        }

        private async void CalculateButton_Click(
            object sender,
            RoutedEventArgs e)
        {
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
            if (_lastCalculatedResult == null)
            {
                await ShowMessageAsync(
                    "Please click Calculate before saving.");
                return;
            }

            await _salaryRepo.SaveEmployeeAsync(_lastCalculatedResult);

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
    }
}
