# MediCare Hospital Management

A small full-stack hospital management MVP using two independent .NET 10 projects.

## Projects
- `HospitalManagement.API` — ASP.NET Core Web API, EF Core, PostgreSQL, JWT, Swagger.
- `HospitalManagement.Web` — ASP.NET Core MVC frontend using Razor views and HttpClient.

## Requirements
- .NET 10 SDK
- PostgreSQL 16+
- Visual Studio 2022/2026 or VS Code

## 1. Create PostgreSQL database

Create a database named:

`hospital_management`

Then update the password in:

`HospitalManagement.API/appsettings.json`

## 2. Run API

From the solution folder:

```bash
dotnet restore
dotnet run --project HospitalManagement.API
```

The API uses the URL shown by ASP.NET Core, typically HTTPS port 7001 if configured. Update `HospitalManagement.Web/appsettings.json` if your API port differs.

Swagger:
`https://localhost:<api-port>/swagger`

## 3. Run Web

In another terminal:

```bash
dotnet run --project HospitalManagement.Web
```

Open the URL shown in the terminal.

## Demo accounts

Admin:
- username: `admin`
- password: `admin123`

Doctor:
- username: `doctor`
- password: `doctor123`

Receptionist:
- username: `reception`
- password: `reception123`

## Important
The sample authentication stores demo passwords as plain text for simplicity. Before production use, replace this with ASP.NET Core Identity or a secure password-hashing implementation, rotate the JWT secret, enable HTTPS-only production configuration, add audit logging, validation, backups, and appropriate healthcare/privacy controls.
