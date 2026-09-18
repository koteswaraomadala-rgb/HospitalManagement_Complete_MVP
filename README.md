# MediCare Hospital Management

ASP.NET Core 7 MVC + JavaScript/AJAX + ASP.NET Core Web API + EF Core + PostgreSQL.

## Architecture

Browser UI -> JavaScript validation -> AJAX -> Web MVC Controller -> HospitalApiService -> API Controller -> Service Layer -> EF Core -> PostgreSQL -> JSON -> Web Controller -> JavaScript -> UI.

The same pattern is used for login and the application modules.

## Modules

- Login / Logout with JWT authentication and server Session.
- Dashboard statistics and today's appointments.
- Patients: list, search, add, edit, delete.
- Doctors: list, add, edit, delete.
- Appointments: list, search/filter, book, edit, status update, delete.
- Prescriptions: list, search, create, delete.
- Reports: date-range activity summary.
- Settings: hospital details, profile, password change.

## Run

Open `HospitalManagement.sln` in Visual Studio on Mac.

Start both projects together:

- API: https://localhost:7001 (Swagger: `/swagger`)
- Web: https://localhost:7002

PostgreSQL must be running on localhost:5432 and the connection string in `HospitalManagement.API/appsettings.json` must match your local PostgreSQL credentials.

The API creates the database/tables with `EnsureCreated` and creates a demo user when no users exist.

Demo login:

- Username: `admin`
- Password: `admin123`

## Important security note

The included connection string and JWT key are development values for the local portfolio project. Do not publish real database credentials or production secrets to GitHub. Before making a public repository, replace them with environment variables/user secrets and rotate any credential that has already been exposed.

## .NET note

The project targets .NET 7 to remain compatible with the legacy Visual Studio for Mac environment used for this project. .NET 7 is end-of-support and should not be used for a new production deployment.

## Latest UI/AJAX fix
- Patient and Doctor creation/editing from the list pages now uses Bootstrap modal forms and AJAX only; clicking Save no longer navigates the browser to `/Patients` or `/Doctors`.
- Field-level validation errors are returned by the MVC controllers and mapped to the matching input.
- Add/Edit forms are protected with `preventDefault()` and explicit `type="submit"`/`type="button"` semantics.
