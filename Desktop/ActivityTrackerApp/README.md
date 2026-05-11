# ActivityTracker API (.NET 8)

Production-ready ASP.NET Core Web API implementing Audit Logging and Activity Tracking using Clean Architecture:

- Controller -> Service -> Repository -> Data Layer
- EF Core + SQL Server
- JWT Authentication + Role-based Authorization
- Serilog logging + global exception middleware
- API versioning + Swagger + FluentValidation
- Filtering/pagination/sorting for audit logs + CSV export

## Project Structure

- `ActivityTracker.Api` - Controllers, middleware, startup config
- `ActivityTracker.Application` - DTOs, service interfaces/implementations, validators
- `ActivityTracker.Infrastructure` - EF Core DbContext, repositories, token/current-user services
- `ActivityTracker.Domain` - Core entities/models

## Setup Instructions

1. Install .NET 8 SDK and SQL Server.
2. Update connection string in `ActivityTracker.Api/appsettings.json`.
3. Restore and build:
   - `dotnet restore`
   - `dotnet build ActivityTracker.sln`
4. Run API:
   - `dotnet run --project ActivityTracker.Api`
5. Open Swagger:
   - `https://localhost:<port>/swagger`

## Angular UI

A full Angular frontend is included in `ActivityTracker.UI`.

Run UI:

- `cd ActivityTracker.UI`
- `npm install`
- `npm start`

UI URL:

- `http://localhost:4200`

## Default Seed User

- Username: `admin`
- Password: `Admin@123`
- Role: `Admin`

Change this immediately for production.

## API Endpoints (v1)

Auth:
- `POST /api/v1/auth/login`
- `POST /api/v1/auth/logout` (authorized)
- `POST /api/v1/auth/register` (admin only)

Tasks:
- `GET /api/v1/tasks`
- `GET /api/v1/tasks/{id}`
- `POST /api/v1/tasks`
- `PUT /api/v1/tasks/{id}`
- `DELETE /api/v1/tasks/{id}` (soft delete)

Audit:
- `GET /api/v1/audit-logs` (admin only)
- `GET /api/v1/audit-logs/export/csv` (admin only)

Dashboard:
- `GET /api/v1/dashboard/summary` (admin only)

## Sample Request/Response

### Login

Request:

```json
{
  "username": "admin",
  "password": "Admin@123"
}
```

Response:

```json
{
  "token": "<jwt-token>",
  "expiresAtUtc": "2026-04-20T02:00:00Z",
  "userId": 1,
  "username": "admin",
  "role": "Admin"
}
```

### Audit Log Search

`GET /api/v1/audit-logs?pageNumber=1&pageSize=20&action=Update&entityName=TaskItem`

Response includes:
- old/new JSON snapshots
- changed fields only (`changedFields`)
- pagination metadata

## Key Features Implemented

- Automatic audit tracking for Create/Update/Delete in `DbContext.SaveChangesAsync`
- Captures: user id, action, entity name/id, old/new JSON, IP, timestamp
- Request activity tracking middleware for API activity/failure
- Login/logout/failed login activity tracking
- Dynamic filtering + sorting + pagination for audit logs
- CSV export for audit logs
- Soft delete support on `TaskItem`
- Dashboard summary API for activity metrics

## Notes

- Uses `Database.EnsureCreated()` for quick startup. For production, replace with EF migrations.
- JWT signing key in appsettings is for demo only.
- Serilog writes to console and rolling file logs under `logs/`.
