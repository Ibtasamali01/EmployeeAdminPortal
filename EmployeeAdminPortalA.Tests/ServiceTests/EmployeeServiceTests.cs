using AutoMapper;
using EmployeeAdminPortal.Helpers.RequestDTOs;
using EmployeeAdminPortal.Model.Models;
using EmployeeAdminPortal.Repository;
using EmployeeAdminPortal.Service;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EmployeeAdminPortal.Tests
{
    [TestFixture]
    public class EmployeeServiceTest
    {
        private Mock<IEmployeeRepository> _repositoryMock;
        private Mock<IMapper> _mapperMock;
        private EmployeeService _service;

        [SetUp]
        public void Setup()
        {
            _repositoryMock = new Mock<IEmployeeRepository>();
            _mapperMock = new Mock<IMapper>();
            _service = new EmployeeService(_repositoryMock.Object, _mapperMock.Object);
        }

        [Test]
        public async Task GetAllEmployeesAsync_ShouldReturnListOfEmployees_WhenEmployeesExist()
        {
            // Arrange
            var employees = new List<Employee1>
            {
                new Employee1 { Id = Guid.NewGuid(), Name = "Ibtasam", Email = "ibtasamAli@gmail.com" },
                new Employee1 { Id = Guid.NewGuid(), Name = "Ali", Email = "ali@gmail.com" }
            };
            _repositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(employees);

            // Act
            var result = await _service.GetAllEmployeesAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<List<Employee1>>(result);
            Assert.AreEqual(employees.Count, result.Count);
        }

        [Test]
        public async Task AddOrUpdateEmployeeAsync_ShouldAddOrUpdateEmployee_BasedOnExistence()
        {
            var employeeId = Guid.NewGuid();
            var dto = new AddEmployeeDto
            {
                Id = employeeId,
                Name = "ali",
                Email = "ali@gmail.com",
                Phone = "1234567890",
                Salary = 60000,
                CreatedBy = 1,
                ContactInfo1 = new ContactInfoDto
                {
                    Address = "ali",
                    PhoneNumber = "1234567890",
                    Email = "abc@gmail.com"
                },
                EmployeeInfo1 = new EmployeeInfoDto
                {
                    Department = "HR",
                    Position = "Manager",
                    Address = "Office",
                    Gender = "Male",
                    FileName = "cv.pdf"
                }
            };

            var employeeEntity = new Employee1 { Id = employeeId };

            _repositoryMock.Setup(repo => repo.GetByIdWithChildrenAsync(employeeId)).ReturnsAsync((Employee1)null);  
            _mapperMock.Setup(map => map.Map<Employee1>(dto)).Returns(employeeEntity);
            _repositoryMock.Setup(repo => repo.AddAsync(employeeEntity)).Returns(Task.CompletedTask);
            _repositoryMock.Setup(repo => repo.SaveAsync()).ReturnsAsync(1);
            var addResult = await _service.AddOrUpdateEmployeeAsync(dto);
            Assert.AreEqual("Employee record created/updated successfully", addResult);
            _repositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Employee1>()), Times.Once);
            _repositoryMock.Verify(repo => repo.SaveAsync(), Times.Once);
            _repositoryMock.Invocations.Clear(); 

            var existingEmployee = new Employee1
            {
                Id = employeeId,
                Name = "Old Name",
                Email = "old@example.com",
                ContactInfo1 = new ContactInfo1(),
                EmployeeInfo1 = new EmployeeInfo1()
            };

            _repositoryMock.Setup(repo => repo.GetByIdWithChildrenAsync(employeeId)).ReturnsAsync(existingEmployee);
            _repositoryMock.Setup(repo => repo.SaveAsync()).ReturnsAsync(1);
            var updateResult = await _service.AddOrUpdateEmployeeAsync(dto);
            Assert.AreEqual("Employee record created/updated successfully", updateResult);
            _repositoryMock.Verify(repo => repo.SaveAsync(), Times.Exactly(1));
        }


        [Test]
        public async Task GetEmployeeByIdAsync_ShouldReturnEmployee_WhenFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            var expectedEmployee = new Employee1 { Id = id };
            _repositoryMock.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync(expectedEmployee);

            // Act
            var result = await _service.GetEmployeeByIdAsync(id);

            // Assert
            Assert.AreEqual(expectedEmployee, result);
        }

        [Test]
        public async Task SaveFileRecordAsync_ShouldSaveFileName_WhenEmployeeExists()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var fileName = "document.pdf";
            var employee = new Employee1
            {
                Id = employeeId,
                EmployeeInfo1 = new EmployeeInfo1() 
            };

            _repositoryMock.Setup(r => r.GetByIdAsync(employeeId)).ReturnsAsync(employee);
            _repositoryMock.Setup(r => r.SaveAsync()).ReturnsAsync(1);

            var service = new EmployeeService(_repositoryMock.Object, _mapperMock.Object);

            // Act
            await service.SaveFileRecordAsync(employeeId, fileName);

            // Assert
            Assert.AreEqual(fileName, employee.EmployeeInfo1.FileName);
        }

        [Test]
        public async Task SoftDeleteEmployeeAsync_ShouldMarkEmployeeAsDeleted_WhenExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var deletedBy = 1;
            var employee = new Employee1 { Id = id, IsDeleted = false };

            _repositoryMock.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync(employee);
            _repositoryMock.Setup(repo => repo.SaveAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.SoftDeleteEmployeeAsync(id, deletedBy);

            // Assert
            Assert.IsTrue(result.IsDeleted);
            Assert.AreEqual(deletedBy, result.DeletedBy);
        }
    }
}
