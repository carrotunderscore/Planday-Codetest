using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Planday.Schedule.Clients;
using Planday.Schedule.Infrastructure.Providers.Interfaces;
using System;


namespace Planday.Schedule.Controllers
{
	[ApiController]
	[Route("[controller]")]
	[ApiVersion("1.0")]
	[ApiExplorerSettings(GroupName = "v1")]
	public class ShiftController : ControllerBase
	{
		private readonly IConnectionStringProvider _connectionStringProvider;
		private readonly IEmployeeApiClient _employeeApiClient;

		public ShiftController(IConnectionStringProvider connectionStringProvider, IEmployeeApiClient employeeApiClient)
		{
			_connectionStringProvider = connectionStringProvider;
			_employeeApiClient = employeeApiClient;
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<Shift>> Get(long id)
		{
			Shift shift = null;

			using (SqliteConnection connection = new SqliteConnection(_connectionStringProvider.GetConnectionString()))
			{
				connection.Open();
				using (SqliteCommand command = new SqliteCommand("SELECT Id, EmployeeId, Start, End FROM Shift WHERE Id = @Id", connection))
				{
					command.Parameters.AddWithValue("@Id", id);
					using (SqliteDataReader reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							if (!reader.IsDBNull(1))
							{
								long? employeeId = reader.GetInt64(1);

								Employee employee = await _employeeApiClient.GetEmployeeById((long)employeeId);

								shift = new Shift(reader.GetInt64(0), reader.GetDateTime(2), reader.GetDateTime(3), employee);
							}
							else
							{
								shift = new Shift(reader.GetInt64(0), reader.GetDateTime(2), reader.GetDateTime(3), null);

							}
						}
					}
				}
			}

			if (shift == null)
			{
				return NotFound($"Shift with ID {id} not found.");
			}

			return Ok(shift);
		}

		[HttpPost]
		public ActionResult<Shift> CreateOpenShift([FromBody] Shift shift)
		{
			if (shift.Start >= shift.End)
			{
				return BadRequest("Start time cannot be greater than or equal to End time.");
			}
			if (shift.Start.Date != shift.End.Date)
			{
				return BadRequest("Start and End time must be on the same day.");
			}
			long insertedId;

			using (SqliteConnection connection = new SqliteConnection(_connectionStringProvider.GetConnectionString()))
			{
				connection.Open();

				using (SqliteCommand command = new SqliteCommand("INSERT INTO Shift (EmployeeId, Start, End) VALUES (@EmployeeId, @Start, @End); SELECT last_insert_rowid();", connection))
				{

					if(shift.Employee != null)
					{
						if (shift.Employee.Id.HasValue)
						{
							command.Parameters.AddWithValue("@EmployeeId", shift.Employee.Id.Value);
						}
						else
						{
							command.Parameters.AddWithValue("@EmployeeId", DBNull.Value);
						}
					}
					else
					{
						command.Parameters.AddWithValue("@EmployeeId", DBNull.Value);
					}

					command.Parameters.AddWithValue("@Start", shift.Start);
					command.Parameters.AddWithValue("@End", shift.End);

					insertedId = (long)command.ExecuteScalar();
				}
			}

			if (insertedId <= 0)
			{
				return BadRequest("Failed to create the shift.");
			}

			Shift newShift = new Shift(
				insertedId,
				shift.Start,
				shift.End,
				null
			);
			return Ok(newShift);
		}

		[HttpPost("AssignToEmployee")]
		public ActionResult AssignShiftToEmployee([FromBody] AssignShiftModel assignShift)
		{
			
			using (SqliteConnection connection = new SqliteConnection(_connectionStringProvider.GetConnectionString()))
			{
				connection.Open();

				// Assign shift to employee only if there's no overlap
				using (SqliteCommand command = new SqliteCommand(@"
						WITH TargetShift AS (
							SELECT Start, End
							FROM Shift
							WHERE Id = @ShiftId
						)
						UPDATE Shift 
						SET EmployeeId = @EmployeeId 
						WHERE Id = @ShiftId 
						AND NOT EXISTS (
							SELECT 1 
							FROM Shift 
							INNER JOIN TargetShift ON 
								(Shift.Start <= TargetShift.End AND Shift.End >= TargetShift.Start)
							WHERE Shift.EmployeeId = @EmployeeId
				 )", connection))
				{
					command.Parameters.AddWithValue("@EmployeeId", assignShift.EmployeeId);
					command.Parameters.AddWithValue("@ShiftId", assignShift.ShiftId);

					int affectedRows = command.ExecuteNonQuery();

					if (affectedRows <= 0)
					{
						return BadRequest("Failed to assign the shift to the employee or there's an overlapping shift.");
					}
				}
			}
			
			return Ok("Shift successfully assigned to the employee.");
		}

	}
}
