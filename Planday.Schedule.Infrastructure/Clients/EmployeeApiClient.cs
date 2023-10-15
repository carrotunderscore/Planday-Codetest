using System;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Planday.Schedule.Clients;

namespace Planday.Schedule.Clients
{
	

	public class EmployeeApiClient : IEmployeeApiClient
	{
		private readonly HttpClient _client;
		private readonly string _apiKey;
		private readonly string _baseUrl;

		public EmployeeApiClient(string apiKey, string baseUrl, HttpClient client)
		{
			_apiKey = apiKey;
			_baseUrl = baseUrl;
			_client = client;
		}

		public async Task<Employee> GetEmployeeById(long id)
		{
			if (id <= 0)
			{
				throw new ArgumentException("Invalid employee ID provided", nameof(id));
			}

			var requestUrl = $"{_baseUrl}employee/{id}";
			_client.DefaultRequestHeaders.Clear();
			_client.DefaultRequestHeaders.Add("Authorization", _apiKey);
			_client.DefaultRequestHeaders.Add("accept", "*/*");

			var response = await _client.GetAsync(requestUrl);

			if (response.IsSuccessStatusCode)
			{
				var json = await response.Content.ReadAsStringAsync();

				var jsonObject = JObject.Parse(json);

				var employee = new Employee(id, jsonObject.Value<string>("name"), jsonObject.Value<string>("email"));
				

				return employee;
			}
			throw new Exception($"Failed to fetch the employee. Response status: {response.StatusCode}");
		}
	}

}
