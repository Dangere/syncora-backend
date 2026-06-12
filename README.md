# Syncora Backend
#### ASP.NET Core REST API and SignalR hub powering [Syncora](https://github.com/Dangere/syncora-frontend).
#### Syncora is an open source full-stack project that allows users to create personal or collaborative tasks with others in real-time with offline first support.
![.NET](https://img.shields.io/badge/.NET-9.0-purple)
![Platform](https://img.shields.io/badge/Platform-ASP.NET%20Core-blue)
![Database](https://img.shields.io/badge/Database-PostgreSQL-336791)

## Features
- JWT authentication with short-lived access tokens and refresh token rotation
- Google Sign-In via server-side token verification against Google's public keys
- Email verification and password reset flows via SMTP
- Full CRUD for groups, tasks, and group membership
- Real-time push events to connected members via SignalR when tasks/groups/users are created, modified, or deleted
- Signed image uploads through Cloudinary
- PostgreSQL with Entity Framework Core

## Frontend
This backend is built specifically to serve [Syncora's Flutter frontend](https://github.com/Dangere/syncora-frontend). The frontend uses specific conventions for signed Cloudinary uploads, SignalR event names, and error codes.
Refer to its README before integrating a custom client.

## Getting Started

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL instance (local or hosted — [Supabase](https://supabase.com) works well with the pooled connection string)
- [Cloudinary](https://cloudinary.com) account (for signed image uploads)
- SMTP credentials (the example config uses [Brevo](https://brevo.com))

### Installation
```bash
git clone https://github.com/Dangere/syncora-backend.git
cd syncora-backend
dotnet restore
```

### Environment Setup
Rename the `appsettings.Exmaple.json` to `appsettings.Development.json` and fill in the fields to your liking and configuration.


**Notes:**
- `ConnectionStrings.DefaultConnection`: your PostgreSQL connection string. If using Supabase, use the pooled connection string.
- `Jwt.SecretKey`: use a long random string; tokens are signed with HMACSHA256 at 600,000 PBKDF2 iterations.
- `GoogleJWTValidation.Audience`: your Google OAuth client ID(s) as an array.
- `EmailingConfig`: any SMTP provider works; the example uses Brevo's relay.
- `CloudinaryConfig`: all four fields are required for signed upload generation.

### Running
```bash
# Development
dotnet run

# With explicit environment
ASPNETCORE_ENVIRONMENT=Development dotnet run
```

Apply migrations before first run:
```bash
dotnet ef database update
```

## Deployment

### CI/CD GitHub Actions
1. On every push to `main`, the workflow in `.github/workflows`, injects production secrets, and deploys to the host via FTP. No manual steps required after initial secrets setup.
2. A Cron job workflow `health-check.yaml` runs every 3 days to call the backend's health which keeps supabase alive.

**Required GitHub secrets:**

| Secret | Description |
|---|---|
| `APPSETTINGS_PRODUCTION` | Full contents of `appsettings.Production.json` |
| `FTP_SERVER` | FTP host address |
| `FTP_USERNAME` | FTP username |
| `FTP_PASSWORD` | FTP password |

## Built With
- [ASP.NET Core](https://learn.microsoft.com/aspnet/core) — Web framework
- [Entity Framework Core](https://learn.microsoft.com/ef/core) — ORM
- [Npgsql](https://www.npgsql.org) — PostgreSQL driver
- [SignalR](https://learn.microsoft.com/aspnet/core/signalr/introduction) — Real-time communication
- [AutoMapper](https://automapper.org) — Object mapping
- [MailKit](https://github.com/jstedfast/MailKit) — Email sending
- [Cloudinary .NET SDK](https://cloudinary.com/documentation/dotnet_integration) — Image upload signing

## License
MIT
