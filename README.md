# DREAMHOMES Server

Backend API for the DREAMHOMES application, built with ASP.NET Core 6.0 and Entity Framework Core.

## 📋 Table of Contents

- [About](#about)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Database Setup](#database-setup)
- [Running the Application](#running-the-application)
- [API Documentation](#api-documentation)
- [Testing](#testing)
- [Project Structure](#project-structure)
- [Architecture](#architecture)
- [Contributing](#contributing)

## 🏠 About

DREAMHOMES Server is a RESTful API backend that powers the DREAMHOMES property management application. It provides secure authentication, property management, user management, and real-time communication capabilities through SignalR integration.

## ✨ Features

- 🔐 **JWT Authentication** - Secure token-based authentication
- 👤 **Identity Management** - ASP.NET Core Identity for user management
- 🗺️ **Geospatial Support** - NetTopologySuite for location-based queries
- 📊 **Entity Framework Core** - Code-first database approach with SQL Server
- ✅ **Validation** - FluentValidation for request validation
- 🔄 **AutoMapper** - Object-to-object mapping
- 📝 **API Documentation** - Swagger/OpenAPI documentation
- 🧪 **Testing** - Unit and Integration tests with SpecFlow (Gherkin)

## 🛠️ Tech Stack

### Core Framework
- **.NET**: 6.0
- **C#**: 10.0
- **ASP.NET Core**: 6.0

### Database & ORM
- **SQL Server**: Database engine
- **Entity Framework Core**: 7.0.10
- **NetTopologySuite**: 7.0.10 - Geospatial data support

### Authentication & Security
- **ASP.NET Core Identity**: 6.0.26 - User management
- **JWT Bearer Authentication**: 6.0.26 - Token-based auth

### Libraries & Tools
- **AutoMapper**: 12.0.1 - Object mapping
- **FluentValidation**: 11.3.0 - Input validation
- **Swashbuckle (Swagger)**: 6.2.3 - API documentation
- **SpecFlow**: BDD testing framework (Integration Tests)

## 🚀 Getting Started

### Prerequisites

Before you begin, ensure you have the following installed:

- **.NET SDK**: 6.0 or higher - [Download here](https://dotnet.microsoft.com/download/dotnet/6.0)
- **SQL Server**: 2019 or higher (Express edition works fine)
- **Visual Studio 2022** or **VS Code** with C# extension
- **SQL Server Management Studio (SSMS)** - Optional but recommended

### Installation

1. **Clone the repository**
```bash
git clone https://github.com/Madhurirao95/dreamhomesserver.git
cd dreamhomesserver
```

2. **Restore NuGet packages**
```bash
dotnet restore
```

3. **Update the connection string**

Edit `appsettings.json` and update the connection string to match your SQL Server instance:
```json
"ConnectionStrings": {
  "SqlConnection": "Server=YOUR_SERVER_NAME;Database=DreamHomes;Trusted_Connection=True;TrustServerCertificate=True"
}
```

4. **Apply database migrations**
```bash
cd DREAMHOMES
dotnet ef database update
```

If migrations don't exist yet, create them:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

5. **Run the application**
```bash
dotnet run
```

The API will be available at `https://localhost:9000` (or the port specified in launchSettings.json)

## ⚙️ Configuration

### JWT Settings

Configure JWT authentication in `appsettings.json`:

```json
"JwtBearerTokenSettings": {
  "SecretKey": "YOUR_SECRET_KEY_HERE",
  "Audience": "https://localhost:4200",
  "Issuer": "https://localhost:9000",
  "ExpiryTimeInMinutes": 1440
}
```

**Security Note**: 
- Change the `SecretKey` to a strong, unique value in production
- Never commit sensitive keys to source control
- Use environment variables or Azure Key Vault for production secrets

### CORS Configuration

The API is configured to allow requests from the Angular frontend at `https://localhost:4200`. Update CORS settings in `Program.cs` or `Startup.cs` if your frontend runs on a different port.

### Connection String

Update the SQL Server connection string for your environment:

**Development (Windows Authentication)**:
```json
"SqlConnection": "Server=YOUR_SERVER;Database=DreamHomes;Trusted_Connection=True;TrustServerCertificate=True"
```

**Production (SQL Authentication)**:
```json
"SqlConnection": "Server=YOUR_SERVER;Database=DreamHomes;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True"
```

## 🗄️ Database Setup

### Using Entity Framework Migrations

1. **Create a new migration**
```bash
dotnet ef migrations add MigrationName
```

2. **Update the database**
```bash
dotnet ef database update
```

3. **Remove last migration** (if needed)
```bash
dotnet ef migrations remove
```

### Database Features

- **Identity Tables**: User authentication and authorization
- **Geospatial Data**: Location-based property queries using NetTopologySuite
- **Code-First Approach**: Database schema defined in C# models

## 🏃 Running the Application

### Development Mode

```bash
dotnet run
```

Or with watch mode (auto-restart on file changes):
```bash
dotnet watch run
```

### Production Build

```bash
dotnet publish -c Release -o ./publish
```

### Running Tests

**Unit Tests**:
```bash
cd DREAMHOMESTEST
dotnet test
```

**Integration Tests** (SpecFlow/Gherkin):
```bash
cd "Integration Tests"
dotnet test
```

## 📚 API Documentation

Once the application is running, access the Swagger UI documentation at:

```
https://localhost:9000/swagger
```

Swagger provides:
- Interactive API documentation
- Request/response examples
- Try-it-out functionality for testing endpoints

## 🧪 Testing

### Project Structure

The solution includes comprehensive testing:

- **DREAMHOMESTEST/**: Unit tests for business logic and services
- **Integration Tests/**: BDD tests using SpecFlow (Gherkin syntax)

### Running All Tests

```bash
dotnet test
```

### Test Coverage

The project includes:
- Unit tests for services and repositories
- Integration tests for API endpoints
- BDD scenarios using Gherkin syntax

## 📁 Project Structure

```
dreamhomesserver/
├── DREAMHOMES/              # Main API project
│   ├── Controllers/         # API endpoints
│   ├── Models/             # Domain models and entities
│   │   └── Enums/          # Enumeration types
│   ├── Services/           # Business logic layer
│   ├── Repositories/       # Data access layer
│   ├── Data/               # DbContext and configurations
│   ├── DTOs/               # Data Transfer Objects
│   ├── Validators/         # FluentValidation validators
│   ├── Profiles/           # AutoMapper profiles
│   └── Program.cs          # Application entry point
├── DREAMHOMESTEST/         # Unit tests
├── Integration Tests/      # Integration tests (SpecFlow)
└── DREAMHOMES.sln          # Solution file
```

## 🏗️ Architecture

### Layered Architecture

The application follows a clean architecture pattern:

1. **Controllers Layer**: HTTP request handling and routing
2. **Services Layer**: Business logic and orchestration
3. **Repository Layer**: Data access and persistence
4. **Models Layer**: Domain entities and DTOs

### Key Patterns

- **Repository Pattern**: Data access abstraction
- **Dependency Injection**: Built-in ASP.NET Core DI
- **DTO Pattern**: Data Transfer Objects for API contracts
- **Mapper Pattern**: AutoMapper for object transformations
- **Validator Pattern**: FluentValidation for input validation

### Authentication Flow

1. User sends credentials to `/api/auth/login`
2. Server validates credentials against Identity database
3. JWT token generated with claims
4. Client includes token in `Authorization: Bearer {token}` header
5. Middleware validates token on protected endpoints

## 🔐 Security

- JWT token-based authentication
- Password hashing via ASP.NET Core Identity
- HTTPS enforcement
- CORS policy configuration
- SQL injection protection via parameterized queries (EF Core)

## 📄 Related Projects

- **Frontend Repository**: [dreamhomes](https://github.com/Madhurirao95/dreamhomes) - Angular 17 client application

## 🔧 Troubleshooting

### Common Issues

**Database connection fails**:
- Verify SQL Server is running
- Check connection string in appsettings.json
- Ensure database exists or run migrations

**JWT authentication fails**:
- Verify SecretKey matches between configuration and token generation
- Check token expiry time
- Ensure Audience and Issuer URLs are correct

**Migration errors**:
- Delete existing migrations and recreate
- Check for model configuration conflicts
- Ensure database provider (SQL Server) is correct\
  
## 👨‍💻 Author

**Madhurirao95**

- GitHub: [@Madhurirao95](https://github.com/Madhurirao95)

---

Built with ❤️ using ASP.NET Core 6.0
