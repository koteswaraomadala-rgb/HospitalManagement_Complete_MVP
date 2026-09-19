# Hospital Management System

A portfolio-ready hospital management MVP built with ASP.NET Core Web API, EF Core, PostgreSQL and a static HTML/CSS/JavaScript frontend.

## Architecture

Browser (HTML/CSS/JavaScript) -> REST API -> Service layer -> EF Core -> PostgreSQL

The frontend no longer uses Razor Pages or MVC views. All create/edit operations use `fetch()` and update the page without normal form navigation.

## Projects

- `HospitalManagement.API` - ASP.NET Core 7 Web API, JWT authentication, EF Core, PostgreSQL, Swagger.
- `HospitalManagement.Web` - ASP.NET Core static-file host serving HTML/CSS/JavaScript.

## Main modules

- Authentication
- Dashboard
- Patients
- Doctors
- Appointments
- Prescriptions
- Reports
- Settings
- Patient Flow / Queue Management

## Run

1. Start PostgreSQL and make sure the connection string in `HospitalManagement.API/appsettings.json` matches your local database.
2. Start `HospitalManagement.API` on `https://localhost:7001`.
3. Start `HospitalManagement.Web` on `https://localhost:7002`.
4. Open `https://localhost:7002`.

Demo account for the MVP: `admin` / `admin123`.

## Frontend

The frontend uses readable intermediate-level JavaScript modules under `HospitalManagement.Web/wwwroot/js` and plain `.html` pages under `wwwroot`.

`wwwroot/js/config.js` contains the API base URL for local development.

## Notes

This project targets .NET 7 for compatibility with the original Visual Studio for Mac environment. .NET 7 is end-of-life and should not be used for production. Password storage in this MVP is intentionally simple and should be replaced with ASP.NET Core Identity/password hashing for production.
