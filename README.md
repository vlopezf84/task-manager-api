# Task Manager API

REST API built with .NET 8 and ASP.NET Core for managing tasks.

## Tech Stack
- .NET 8
- ASP.NET Core Web API
- SQL Server (coming soon)
- Entity Framework Core (coming soon)
- Docker (coming soon)
- Azure (coming soon)

## Endpoints
| Method | Route | Description |
|--------|-------|-------------|
| GET | /api/tasks | Get all tasks |
| GET | /api/tasks/{id} | Get task by ID |
| POST | /api/tasks | Create task |
| PUT | /api/tasks/{id} | Update task |
| DELETE | /api/tasks/{id} | Delete task |

## Running locally
```bash
dotnet run
```
Navigate to `https://localhost:{port}/swagger` to explore the API.