# Project: LudaFit (backend)

## Architecture & Patterns
- **VSA (Vertical-Sliced-Architecture) with single static file**.
- **Result pattern**.
- **Rich models (like in DDD)**. 

## Stack
- **ASP.NET Core Web API**.
- **EntityFrameworkCore (SQLite)**.
- **FluentValidation**.
- **xUnit + FluentAssertions**.
- **MailKit**.
- **Azure Blob Storage**.
- **Scalar for API documentation**.

## Critical Coding Rules (MUST FOLLOW)
- **Typing:** Use EXPLICIT types. Use `var` ONLY when the type is obvious from the right side (e.g., `new()`).
- **Async:** All I/O operations must be `async/await`. Append `Async` to method names.
- **Results:** Always return `Result<T>` or `Result` from the SharedKernel. Do not throw exceptions for flow control.
- **DI:** Use constructor injection only.
- **Rich Models (DDD):** Use private setters for entities. Instantiation MUST happen via static factory methods (e.g., Create()) returning Result<T>.
- **VSA:** Endpoints MUST be defined as Minimal APIs using IEndpointRouteBuilder inside the static file.
- **EF Core:** Use .AsNoTracking() for pure read operations in EF Core.
- **Hashing:** Use Microsoft.AspNetCore.Identity.IPasswordHasher<T> for hashing passwords.

## Available Skills & Tools
- **Unit Testing:** Use the `$unit-tests` skill for any testing tasks. Follow its internal xUnit/FluentAssertions rules.
- **Vertical Slices:** Use `$vertical-slice` to scaffold new features.

## Workspace Commands
- **Build:** `dotnet build LudaFit.Server.slnx`
- **Run Tests:** `dotnet test LudaFit.Tests`
