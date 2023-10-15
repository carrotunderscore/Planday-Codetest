using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;
using Planday.Schedule.Infrastructure;
using Planday.Schedule.Clients;

namespace Planday.Schedule.Clients.Tests
{
	public class EmployeeApiClientTests
	{
		private const string TestApiKey = "testApiKey";
		private const string TestBaseUrl = "http://test.com/";

		private readonly Mock<HttpMessageHandler> _handlerMock;
		private readonly HttpClient _httpClient;
		private readonly EmployeeApiClient _client;

		public EmployeeApiClientTests()
		{
			_handlerMock = new Mock<HttpMessageHandler>();

			_httpClient = new HttpClient(_handlerMock.Object);
			_client = new EmployeeApiClient(TestApiKey, TestBaseUrl, _httpClient);
		}

		private void SetupHttpResponse(HttpResponseMessage response)
		{
			_handlerMock.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>()
				)
				.ReturnsAsync(response)
				.Verifiable();
		}

		[Fact]
		public async Task GetEmployeeById_ShouldThrowArgumentException_WhenIdIsZeroOrNegative()
		{
			await Assert.ThrowsAsync<ArgumentException>(() => _client.GetEmployeeById(0));
			await Assert.ThrowsAsync<ArgumentException>(() => _client.GetEmployeeById(-1));
		}

		[Fact]
		public async Task GetEmployeeById_ShouldReturnEmployee_WhenResponseIsSuccessful()
		{
			var response = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(@"{ 'name': 'Test Name', 'email': 'test@email.com' }"),
			};
			SetupHttpResponse(response);

			var result = await _client.GetEmployeeById(1);

			Assert.Equal("Test Name", result.Name);
			Assert.Equal("test@email.com", result.Email);
		}
	}
}
