# .NET CLI Commands Used to Create Relay Service

This document lists all the commands used to scaffold the Relay Service microservice for future reference.

## Project Creation

```bash
# Navigate to workspace root
cd c:\Users\Ofek Cofman\Personal\yearC\finalProject\SQL-Detective-Backend\SqlDetective.Backend

# Create solution folder
mkdir SqlDetective.RelayService
cd SqlDetective.RelayService

# Create API project (ASP.NET Core Web API)
dotnet new webapi -n SqlDetective.RelayService.Api -f net8.0

# Create Domain project (Class Library)
dotnet new classlib -n SqlDetective.RelayService.Domain -f net8.0

# Create Infrastructure project (Class Library)
dotnet new classlib -n SqlDetective.RelayService.Infrastructure -f net8.0
```

## Add Project References

```bash
# Add Domain reference to API
cd SqlDetective.RelayService.Api
dotnet add reference ../SqlDetective.RelayService.Domain/SqlDetective.RelayService.Domain.csproj

# Add Infrastructure reference to API
dotnet add reference ../SqlDetective.RelayService.Infrastructure/SqlDetective.RelayService.Infrastructure.csproj

# Add Domain reference to Infrastructure
cd ../SqlDetective.RelayService.Infrastructure
dotnet add reference ../SqlDetective.RelayService.Domain/SqlDetective.RelayService.Domain.csproj
```

## Add NuGet Packages

### API Project
```bash
cd SqlDetective.RelayService.Api
dotnet add package RabbitMQ.Client
dotnet add package Newtonsoft.Json
```

### Infrastructure Project
```bash
cd ../SqlDetective.RelayService.Infrastructure
dotnet add package RabbitMQ.Client
dotnet add package Microsoft.Extensions.Hosting.Abstractions
dotnet add package Microsoft.Extensions.Logging.Abstractions
dotnet add package Microsoft.Extensions.Http
dotnet add package Microsoft.Extensions.Configuration.Abstractions
```

## Add to Solution

```bash
# Navigate to solution root
cd ../..

# Add all three projects to solution
dotnet sln SqlDetective.Backend.sln add SqlDetective.RelayService/SqlDetective.RelayService.Api/SqlDetective.RelayService.Api.csproj
dotnet sln SqlDetective.Backend.sln add SqlDetective.RelayService/SqlDetective.RelayService.Domain/SqlDetective.RelayService.Domain.csproj
dotnet sln SqlDetective.Backend.sln add SqlDetective.RelayService/SqlDetective.RelayService.Infrastructure/SqlDetective.RelayService.Infrastructure.csproj
```

## Build and Run

```bash
# Build the entire solution
cd SqlDetective.Backend
dotnet build

# Or build just the Relay Service
dotnet build SqlDetective.RelayService/SqlDetective.RelayService.Api/SqlDetective.RelayService.Api.csproj

# Run the Relay Service
cd SqlDetective.RelayService/SqlDetective.RelayService.Api
dotnet run

# Or run in watch mode (auto-restart on file changes)
dotnet watch run
```

## Folder Structure Creation

```bash
# Create Domain folders
cd SqlDetective.RelayService/SqlDetective.RelayService.Domain
mkdir Models
mkdir Services

# Create Infrastructure folders
cd ../SqlDetective.RelayService.Infrastructure
mkdir MessageQueue
mkdir Validation

# Create API Controllers folder
cd ../SqlDetective.RelayService.Api
mkdir Controllers
```

## Package Versions Installed

As of February 2026:

| Package | Version |
|---------|---------|
| RabbitMQ.Client | 7.2.0 |
| Newtonsoft.Json | 13.0.4 |
| Microsoft.Extensions.Http | 10.0.3 |
| Microsoft.Extensions.Hosting.Abstractions | 10.0.3 |
| Microsoft.Extensions.Logging.Abstractions | 10.0.3 |
| Microsoft.Extensions.Configuration.Abstractions | 10.0.3 |

## Quick Setup Script (PowerShell)

```powershell
# Complete setup in one script
$root = "c:\Users\Ofek Cofman\Personal\yearC\finalProject\SQL-Detective-Backend\SqlDetective.Backend"
cd $root

# Create projects
mkdir SqlDetective.RelayService
cd SqlDetective.RelayService
dotnet new webapi -n SqlDetective.RelayService.Api -f net8.0
dotnet new classlib -n SqlDetective.RelayService.Domain -f net8.0
dotnet new classlib -n SqlDetective.RelayService.Infrastructure -f net8.0

# Add references
cd SqlDetective.RelayService.Api
dotnet add reference ../SqlDetective.RelayService.Domain/SqlDetective.RelayService.Domain.csproj
dotnet add reference ../SqlDetective.RelayService.Infrastructure/SqlDetective.RelayService.Infrastructure.csproj

cd ../SqlDetective.RelayService.Infrastructure
dotnet add reference ../SqlDetective.RelayService.Domain/SqlDetective.RelayService.Domain.csproj

# Add packages
cd ../SqlDetective.RelayService.Api
dotnet add package RabbitMQ.Client
dotnet add package Newtonsoft.Json

cd ../SqlDetective.RelayService.Infrastructure
dotnet add package RabbitMQ.Client
dotnet add package Microsoft.Extensions.Hosting.Abstractions
dotnet add package Microsoft.Extensions.Logging.Abstractions
dotnet add package Microsoft.Extensions.Http
dotnet add package Microsoft.Extensions.Configuration.Abstractions

# Add to solution
cd $root
dotnet sln SqlDetective.Backend.sln add SqlDetective.RelayService/SqlDetective.RelayService.Api/SqlDetective.RelayService.Api.csproj
dotnet sln SqlDetective.Backend.sln add SqlDetective.RelayService/SqlDetective.RelayService.Domain/SqlDetective.RelayService.Domain.csproj
dotnet sln SqlDetective.Backend.sln add SqlDetective.RelayService/SqlDetective.RelayService.Infrastructure/SqlDetective.RelayService.Infrastructure.csproj

# Create folder structure
cd SqlDetective.RelayService/SqlDetective.RelayService.Domain
mkdir Models, Services

cd ../SqlDetective.RelayService.Infrastructure
mkdir MessageQueue, Validation

cd ../SqlDetective.RelayService.Api
mkdir Controllers

Write-Host "✅ Relay Service scaffolding complete!" -ForegroundColor Green
```

## Verification Commands

```bash
# Verify solution structure
dotnet sln list

# Verify project references
cd SqlDetective.RelayService/SqlDetective.RelayService.Api
dotnet list reference

# Verify packages
dotnet list package

# Restore all dependencies
dotnet restore

# Clean build artifacts
dotnet clean

# Full rebuild
dotnet build --no-incremental
```

## Common Issues and Solutions

### Issue: "Cursor Sandbox is unsupported"
**Solution**: Use `required_permissions: ["all"]` in Shell commands or use semicolon (`;`) instead of `&&` in PowerShell

### Issue: Package not found
**Solution**: Run `dotnet restore` or add `--force` flag:
```bash
dotnet add package RabbitMQ.Client --force
```

### Issue: Project not in solution
**Solution**: Re-add the project:
```bash
dotnet sln add path/to/project.csproj
```

### Issue: Reference not found
**Solution**: Verify the path is correct and the referenced project exists:
```bash
dotnet add reference ../path/to/project.csproj
```

---

**Note**: All commands assume you're using PowerShell on Windows. For bash/zsh, replace `;` with `&&` and adjust path separators.
