using Microsoft.EntityFrameworkCore;
using salary.DataLayer.Interface;
using salary.DataLayer.Models;
using salary.SharedLayer;

namespace salary.DataLayer.Repo
{
    public class salaryRepo : ISalaryRepository
    {
        private readonly salaryDbContext _context;

        private readonly List<Pension> _pensions;
        private readonly List<Tax> _taxes;

        public salaryRepo(salaryDbContext context)
        {
            _context = context;

            _pensions = _context.Pensions.ToList();
            _taxes = _context.Taxes.ToList();
        }

        public async Task<SalaryDto> CalculateSalaryAsync(
            string name,
            decimal gross)
        {
            var pension = _pensions
                .FirstOrDefault(x =>
                    gross >= x.FromSalary &&
                    gross <= x.ToSalary);

            var tax = _taxes
                .FirstOrDefault(x =>
                    gross >= x.FromSalary &&
                    gross <= x.ToSalary);

            if (pension == null || tax == null)
            {
                throw new Exception("Salary range not found.");
            }

            decimal pensionAmount =
                gross * pension.Rate / 100;

            decimal taxAmount =
                gross * tax.Rate / 100;

            decimal netSalary =
                gross - taxAmount - pensionAmount;

            return new SalaryDto
            {
                Name = name,
                Gross = gross,
                Tax = taxAmount,
                Pension = pensionAmount,
                Salary = netSalary
            };
        }

        public async Task SaveEmployeeAsync(SalaryDto salaryData)
        {
            var employee = new Employee
            {
                EmpNo = Guid.NewGuid(),
                Name = salaryData.Name,
                Gross = salaryData.Gross,
                Tax = salaryData.Tax,
                Pension = salaryData.Pension,
                Salary = salaryData.Salary
            };

            _context.Employees.Add(employee);

            await _context.SaveChangesAsync();
        }

        public async Task<List<Employee>> GetAllEmployeesAsync()
        {
            return await _context.Employees.OrderByDescending(e => e.Salary).ToListAsync();
        }
    }
}