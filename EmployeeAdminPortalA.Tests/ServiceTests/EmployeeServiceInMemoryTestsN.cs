using AutoMapper;
using EmployeeAdminPortal.Helpers.RequestDTOs;
using EmployeeAdminPortal.Model;
using EmployeeAdminPortal.Model.Models;
using EmployeeAdminPortal.Repository;
using EmployeeAdminPortal.Service;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace EmployeeAdminPortal.Tests
{
    [TestFixture]
    public class EmployeeServiceInMemoryTestsN
    {
        private DbContextOptions<ApplicationDbContext> GetInMemoryOptions()
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) 
                .Options;
        }

        private IMapper GetMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<AddEmployeeDto, Employee1>();
                cfg.CreateMap<ContactInfoDto, ContactInfo1>();
                cfg.CreateMap<EmployeeInfoDto, EmployeeInfo1>();
            });
            return config.CreateMapper();
        }

        [Test]
        public async Task AddOrUpdateEmployeeAsync_ShouldUpdateEmployee_WhenExists()
        {
            // Arrange
            var options = GetInMemoryOptions();
            var mapper = GetMapper();

            using var context = new ApplicationDbContext(options);
            var repository = new EmployeeRepository(context);
            var service = new EmployeeService(repository, mapper);

            var employeeId = Guid.NewGuid();
            var addDto = new AddEmployeeDto
            {
                Id = employeeId,
                Name = "Ali",
                Email = "ali@gmail.com",
                CreatedBy = 1,
                ContactInfo1 = new ContactInfoDto
                {
                    Address = "Lahore",
                    Email = "ali@gmail.com"
                },
                EmployeeInfo1 = new EmployeeInfoDto
                {
                    Department = "IT",
                    Position = "Developer",
                    Gender = "Male",
                    Address = "Old Address"
                }
            };

            await service.AddOrUpdateEmployeeAsync(addDto);
            var updateDto = new AddEmployeeDto
            {
                Id = employeeId,
                Name = "Updated Ali",
                Email = "updated@gmail.com",
                CreatedBy = 1,
                ContactInfo1 = new ContactInfoDto
                {
                    Address = "Karachi",
                    Email = "updated@gmail.com"
                },
                EmployeeInfo1 = new EmployeeInfoDto
                {
                    Department = "HR",
                    Position = "Manager",
                    Gender = "Male",
                    Address = "New Address"
                }
            };

            var resultMessage = await service.AddOrUpdateEmployeeAsync(updateDto);

            // Assert
            var updatedEmployee = await context.Employees1
                .Include(e => e.ContactInfo1)
                .Include(e => e.EmployeeInfo1)
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            Assert.IsNotNull(updatedEmployee);
            Assert.AreEqual("Updated Ali", updatedEmployee.Name);
            Assert.AreEqual("updated@gmail.com", updatedEmployee.Email);
            Assert.AreEqual("Karachi", updatedEmployee.ContactInfo1.Address);
            Assert.AreEqual("HR", updatedEmployee.EmployeeInfo1.Department);
            Assert.AreEqual("Employee record created/updated successfully", resultMessage);
        }

[Test]
        public async Task GetAllEmployeesAsync_ShouldReturnList_WhenExists()
        {
            var options = GetInMemoryOptions();
            var mapper = GetMapper();

            using var context = new ApplicationDbContext(options);
            var repository = new EmployeeRepository(context);
            var service = new EmployeeService(repository, mapper);

            context.Employees1.Add(new Employee1
            {
                Id = Guid.NewGuid(),
                Name = "Ali",
                Email = "ali@test.com",
                CreatedBy = 1,
                CreatedDateTime = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            var result = await service.GetAllEmployeesAsync();

            Assert.NotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public async Task SoftDeleteEmployeeAsync_ShouldMarkAsDeleted()
        {
            var options = GetInMemoryOptions();
            var mapper = GetMapper();

            using var context = new ApplicationDbContext(options);
            var repository = new EmployeeRepository(context);
            var service = new EmployeeService(repository, mapper);

            var empId = Guid.NewGuid();
            context.Employees1.Add(new Employee1
            {
                Id = empId,
                Name = "Delete Test",
                CreatedBy = 1,
                CreatedDateTime = DateTime.UtcNow,
                IsDeleted = false
            });
            await context.SaveChangesAsync();

            var result = await service.SoftDeleteEmployeeAsync(empId, deletedBy: 2);

            Assert.IsTrue(result.IsDeleted);
            Assert.AreEqual(2, result.DeletedBy);
        }

        [Test]
        public async Task GetEmployeeByIdAsync_ShouldReturnEmployee_WhenExists()
        {
            var options = GetInMemoryOptions();
            var mapper = GetMapper();

            using var context = new ApplicationDbContext(options);
            var repository = new EmployeeRepository(context);
            var service = new EmployeeService(repository, mapper);

            var empId = Guid.NewGuid();
            context.Employees1.Add(new Employee1
            {
                Id = empId,
                Name = "Ibtasam Ali",
                Email = "ibtasamaliali@test.com"
            });
            await context.SaveChangesAsync();

            var result = await service.GetEmployeeByIdAsync(empId);

            Assert.NotNull(result);
            Assert.AreEqual(empId, result.Id);
        }

        [Test]
        public async Task SaveFileRecordAsync_ShouldUpdateFileName()
        {
            var options = GetInMemoryOptions();
            var mapper = GetMapper();

            using var context = new ApplicationDbContext(options);
            var repository = new EmployeeRepository(context);
            var service = new EmployeeService(repository, mapper);

            var empId = Guid.NewGuid();
            var employee = new Employee1
            {
                Id = empId,
                Name = "File Test",
                EmployeeInfo1 = new EmployeeInfo1()
            };
            context.Employees1.Add(employee);
            await context.SaveChangesAsync();

            await service.SaveFileRecordAsync(empId, "testfile.pdf");

            var updated = await service.GetEmployeeByIdAsync(empId);
            Assert.AreEqual("testfile.pdf", updated.EmployeeInfo1.FileName);
        }
    }
}
