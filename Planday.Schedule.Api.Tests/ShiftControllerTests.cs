using Moq;
using Xunit;
using Planday.Schedule.Controllers;
using Planday.Schedule.Infrastructure.Providers.Interfaces;
using Planday.Schedule.Clients;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Planday.Schedule.Api.Tests
{
	public class ShiftControllerTests
	{
		private readonly Mock<IConnectionStringProvider> _mockConnectionStringProvider;
		private readonly Mock<IEmployeeApiClient> _mockEmployeeApiClient;

		public ShiftControllerTests()
		{
			_mockConnectionStringProvider = new Mock<IConnectionStringProvider>();
			_mockEmployeeApiClient = new Mock<IEmployeeApiClient>();
		}

		[Fact]
		public async Task Get_ShouldReturnNotFound_WhenShiftDoesNotExist()
		{
			long id = 3;
			_mockConnectionStringProvider.Setup(p => p.GetConnectionString()).Returns("Data Source=planday-schedule.db;");

			var controller = new ShiftController(_mockConnectionStringProvider.Object, _mockEmployeeApiClient.Object);

			var result = await controller.Get(id);

			Assert.IsType<NotFoundObjectResult>(result.Result);
		}

		[Fact]
		public void CreateOpenShift_ShouldReturnBadRequest_WhenStartTimeGreaterThanOrEqualToEndTime()
		{
			var shift = new Shift(1, DateTime.Now, DateTime.Now.AddDays(-1), null);
			var controller = new ShiftController(_mockConnectionStringProvider.Object, _mockEmployeeApiClient.Object);

			var result = controller.CreateOpenShift(shift);

			Assert.IsType<BadRequestObjectResult>(result.Result);
		}

		[Fact]
		public void AssignShiftToEmployee_ShouldReturnBadRequest_WhenShiftAssignmentFails()
		{
			var assignShiftModel = new AssignShiftModel { EmployeeId = 1, ShiftId = 1 }; // Assuming AssignShiftModel has EmployeeId and ShiftId properties
			_mockConnectionStringProvider.Setup(p => p.GetConnectionString()).Returns("Data Source=planday-schedule.db;");

			var controller = new ShiftController(_mockConnectionStringProvider.Object, _mockEmployeeApiClient.Object);

			var result = controller.AssignShiftToEmployee(assignShiftModel);

			Assert.IsType<BadRequestObjectResult>(result);
		}

	}
}
