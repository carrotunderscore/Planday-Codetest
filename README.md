Code test from Planday

An appsettings.json file is needed to run this. Add it to the root of the Planday.Schedule.Api. Substitute the capitalized values with its corresponding values. It should look like this:
```
{ "Logging":
  { "LogLevel":
    { "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings":
    {
      "Database": "DATABASE_URL"
    },
  "EmployeeApi":
  { "ApiKey": "API_KEY",
    "BaseUrl": "BASE_URL"
  }
}
```
