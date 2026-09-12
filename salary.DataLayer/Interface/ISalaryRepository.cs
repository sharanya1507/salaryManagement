using salary.SharedLayer;

namespace salary.DataLayer.Interface
{
    public interface ISalaryRepository
    {
        Task<SalaryDto> CalculateSalaryAsync(
            string name,
            decimal gross);

        Task SaveEmployeeAsync(SalaryDto salaryData);
    }
}