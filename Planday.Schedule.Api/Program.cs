using Microsoft.OpenApi.Models;
using Planday.Schedule.Clients;
using Planday.Schedule.Infrastructure.Providers;
using Planday.Schedule.Infrastructure.Providers.Interfaces;
using Planday.Schedule.Infrastructure.Queries;
using Planday.Schedule.Queries;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo { Title = "Planday.Schedule.Api v1", Version = "v1" });
});

builder.Services.AddSingleton<IConnectionStringProvider>(new ConnectionStringProvider(builder.Configuration.GetConnectionString("Database")));
builder.Services.AddScoped<IGetAllShiftsQuery, GetAllShiftsQuery>();

var apiKey = builder.Configuration["EmployeeApi:ApiKey"];
var baseUrl = builder.Configuration["EmployeeApi:BaseUrl"];

builder.Services.AddHttpClient<IEmployeeApiClient, EmployeeApiClient>(client =>
{
	client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddTransient<IEmployeeApiClient>(sp =>
{
	var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
	var client = httpClientFactory.CreateClient(nameof(IEmployeeApiClient));
	return new EmployeeApiClient(apiKey, baseUrl, client);
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
	c.SwaggerEndpoint("/swagger/v1/swagger.json", "Planday.Schedule.Api v1");
});
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();