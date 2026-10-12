# Devices API

A RESTful API for creating, retrieving, updating, filtering, and deleting device resources. The application uses ASP.NET Core, Entity Framework Core, and SQLite, with Docker support for containerized execution.

## Features

- Create devices and retrieve a single device or all devices.
- Update devices completely using `PUT` or partially using `PATCH`.
- Filter devices by brand and state.
- Delete devices when business rules allow it.
- Persist data using SQLite.
- Apply database migrations automatically at application startup.
- Explore API endpoints through Swagger UI.
- Run automated tests with xUnit.
- Run the application using Docker Compose with persistent database storage.

## Technology Stack

- C# 13
- .NET 9 / ASP.NET Core Web API
- Entity Framework Core 9
- SQLite
- Swagger / OpenAPI
- xUnit
- Docker and Docker Compose

## Project Structure

```text
Devices/
├── Devices.Api/
│   ├── Controllers/
│   ├── Data/
│   ├── DTOs/
│   ├── Migrations/
│   ├── Models/
│   ├── Properties/
│   ├── Services/
│   ├── Program.cs
│   └── appsettings.json
├── Devices.Tests/
├── Dockerfile
├── .dockerignore
├── docker-compose.yml
├── .gitignore
├── Devices.sln
└── README.md
```

## Prerequisites

For local development:

- .NET 9 SDK
- A compatible IDE or code editor

For containerized execution:

- Docker Desktop with Docker Compose support
- WSL 2 on Windows when using the Docker Desktop WSL 2 backend

## Getting Started

### Run Locally

Clone the repository and navigate to its root directory.

Restore the dependencies:

```bash
dotnet restore Devices.sln
```

Build the solution:

```bash
dotnet build Devices.sln
```

Run the API:

```bash
dotnet run --project Devices.Api/Devices.Api.csproj
```

The API applies pending database migrations during startup. The local SQLite database is stored under `Devices.Api/App_Data/devices.db`.

Open the URL displayed in the terminal and append `/swagger` to access the interactive API documentation.

### Run with Docker Compose

From the repository root, build the image and start the application:

```bash
docker compose up --build -d
```

Open Swagger UI:

http://localhost:8080/swagger

Check the container status:

```bash
docker compose ps
```

View the application logs:

```bash
docker compose logs -f devices-api
```

Stop the application:

```bash
docker compose down
```

The Compose configuration uses a named Docker volume for the SQLite database. The database persists when containers are stopped and recreated.

To remove the containers and the persistent database volume, use:

```bash
docker compose down -v
```

**Warning:** The `-v` option deletes the Compose-managed database volume and its stored data.

## Database and Migrations

The application uses Entity Framework Core with SQLite.

Pending migrations are applied when the API starts. The SQLite database is stored in a local file when running outside Docker and in a mounted volume when running through Docker Compose.

Migration files are committed to the repository so that the database schema can be recreated in a new environment.

The local database file is excluded from version control.

## API Endpoints

The base route is `/api/devices`.

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/devices` | Create a device |
| GET | `/api/devices` | Retrieve all devices |
| GET | `/api/devices/{id}` | Retrieve a device by ID |
| GET | `/api/devices?brand=Samsung` | Filter by brand |
| GET | `/api/devices?state=available` | Filter by state |
| GET | `/api/devices?brand=Samsung&state=available` | Combine filters |
| PUT | `/api/devices/{id}` | Fully update a device |
| PATCH | `/api/devices/{id}` | Partially update a device |
| DELETE | `/api/devices/{id}` | Delete a device |

### Device States

The supported states are:

- `available`
- `in-use`
- `inactive`

### Create a Device

Request:

```http
POST /api/devices
Content-Type: application/json
```

```json
{
  "name": "Galaxy ZFlip 5",
  "brand": "Samsung",
  "state": "available"
}
```

A successful creation returns `201 Created`, including the generated device ID and creation timestamp.

### Update a Device

`PUT` requires the editable fields to be provided:

```json
{
  "name": "Galaxy ZFlip 5",
  "brand": "Samsung",
  "state": "inactive"
}
```

`PATCH` changes only the fields supplied in the request:

```json
{
  "state": "available"
}
```

The device ID and creation timestamp are managed by the application and cannot be changed through these update requests.

## Business Rules

- The creation timestamp cannot be updated.
- A device in the `in-use` state cannot have its name or brand changed.
- A device in the `in-use` state cannot be deleted.
- A device's state can be changed independently of its name and brand.
- Device names and brands must not be empty or whitespace.
- Only the supported device states are accepted.

### HTTP Response Codes

| Status | Meaning |
|---|---|
| `200 OK` | The operation completed successfully and returned a response |
| `201 Created` | A device was created |
| `204 No Content` | A device was deleted successfully |
| `400 Bad Request` | The request or a filter value is invalid |
| `404 Not Found` | The requested device does not exist |
| `409 Conflict` | The operation violates a business rule |

## Automated Tests

Run the test suite from the repository root:

```bash
dotnet test Devices.sln
```

The test project uses xUnit and an isolated SQLite database for service tests. The tests cover device creation, retrieval, filtering, updates, creation-time immutability, and deletion constraints.

## Design Overview

- **Controllers:** Handle HTTP requests and responses.
- **Services:** Implement application logic and business rules.
- **DTOs:** Define the request and response contracts.
- **Models:** Represent device domain data.
- **Data:** Configure Entity Framework Core and database access.
- **Migrations:** Track database schema changes.
- **Tests:** Verify the expected service behavior.

The service layer keeps business rules separate from HTTP handling, while DTOs prevent clients from directly controlling internal persistence fields.

## Future Improvements

Potential improvements include centralized exception handling, API versioning, structured logging, health checks, stronger validation coverage, CI workflows, and a dedicated migration step for multi-instance production deployments.
