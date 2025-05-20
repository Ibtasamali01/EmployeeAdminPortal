using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAdminPortal.API.Controllers;
using EmployeeAdminPortal.Helpers.RequestDTOs;
using EmployeeAdminPortal.Model.Models;
using EmployeeAdminPortal.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace EmployeeAdminPortal.NUnitTests
{
    [TestFixture]
    public class EmployeeControllerTests
    {
        private Mock<IEmployeeService> _serviceMock;
        private EmployeeController _controller;
        // priavte Mock<IemployeeService> -serviceMock;
        //prviate EmployeeController _contyroller;

        [SetUp]
        public void Setup()
        {
            _serviceMock = new Mock<IEmployeeService>();
            //_serviceMock= new Mock<IemployeeService>();
            _controller = new EmployeeController(_serviceMock.Object);
            //_controllerr= new EmployeeController(_serviceMock.Object);
        }

        [Test]
        public async Task GetAllEmployees_ShouldReturnOkWithData()
        {
            _serviceMock.Setup(x => x.GetAllEmployeesAsync()).ReturnsAsync(new List<Employee1>());
  
            var result = await _controller.GetAllEmployees();

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.IsInstanceOf<List<Employee1>>(okResult.Value);
        }

        [Test]
        public async Task GetEmployee_ShouldReturnNotFound_WhenNotExists()
        {
            var id = Guid.NewGuid();
            _serviceMock.Setup(x => x.GetEmployeeByIdAsync(id)).ReturnsAsync((Employee1)null);

            var result = await _controller.GetEmployee(id);

            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        [Test]
        public async Task AddOrUpdateEmployee_ShouldReturnBadRequest_WhenDtoIsNull()
        {
            var result = await _controller.AddOrUpdateEmployee(null);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task AddOrUpdateEmployee_ShouldReturnOk_WhenValid()
        {
            var dto = new AddEmployeeDto { Name = "Ali" };
            _serviceMock.Setup(x => x.AddOrUpdateEmployeeAsync(dto)).ReturnsAsync("Saved");

         var result = await _controller.AddOrUpdateEmployee(dto);

            var ok = result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreEqual("Saved", ok.Value);
        }

        [Test]
        public async Task SoftDeleteEmployee_ShouldReturnNotFound_WhenNotExists()
        {
            var id = Guid.NewGuid();
            _serviceMock.Setup(x => x.SoftDeleteEmployeeAsync(id, 1)).ReturnsAsync((Employee1)null);

            var result = await _controller.SoftDeleteEmployee(id, 1);

            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        [Test]
        public async Task SoftDeleteEmployee_ShouldReturnOk_WhenFound()
        {
            var id = Guid.NewGuid();
            var mockEmployee = new Employee1 { Id = id, Name = "Test" };
            _serviceMock.Setup(x => x.SoftDeleteEmployeeAsync(id, 1)).ReturnsAsync(mockEmployee);

            var result = await _controller.SoftDeleteEmployee(id, 1);

            var ok = result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.IsInstanceOf<Employee1>(ok.Value);
        }
    }
}
