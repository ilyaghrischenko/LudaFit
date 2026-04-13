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
- **Telegram.Bot**.
- **Azure Blob Storage**.
- **Scalar for API documentation**.

## Critical Coding Rules (MUST FOLLOW)
- **Typing:** Use EXPLICIT types. Use `var` ONLY when the type is obvious from the right side (e.g., `new()`).
- **Async:** All I/O operations must be `async/await`. Append `Async` to method names.
- **Results:** Always return `Result<T>` or `Result` from the LudaFit.SharedKernel.Models. Do not throw exceptions for flow control.
- **Result Propagation:** When propagating a failure from a nested `Result<T>` call, return it directly — rely on implicit conversion: `if (x.IsFailure) return x;` ✓. Do NOT re-wrap: `if (x.IsFailure) return Result.Failure(x);` ✗ — это double-wraps ошибку и теряет оригинальные детали. `Result.Failure(...)` конструируй только когда **сам создаёшь** новую ошибку на этом call site.
- **Result Variable Naming:** Name `Result<T>` variables after the operation that produced them, prefixed by the verb: `createUserResult`, `sendEmailResult`, `uploadFileResult`. Never use generic names like `result`, `res`, or the entity name alone (`clientMetricsResult` instead of `createClientMetricsResult`). Helper methods that map failures to HTTP responses MUST reflect their specificity in the name: `ToFailureHttpResult()`, not `ToHttpResult()`. Parameter names for domain entities inside lambdas MUST reflect the domain concept, not the generic `entity` (e.g., `user =>`, `client =>`, `workout =>`).
- **DI:** Use constructor injection only.
- **Rich Models (DDD):** Use private setters for entities. Instantiation MUST happen via static factory methods (e.g., Create()) returning Result<T>.
- **VSA:** Endpoints MUST be defined as Minimal APIs using IEndpointRouteBuilder inside the static file.
- **Data Access:** DO NOT USE the Repository pattern. Inject and use `LudaFit.Infrastructure.SQLite.LudaFitDbContext` DIRECTLY.
- **EF Core:** Use `.AsNoTracking()` for pure read operations.
- **Hashing:** Use Microsoft.AspNetCore.Identity.IPasswordHasher<T> for hashing passwords.
- **Validation:** FluentValidation is strictly for surface-level, stateless checks (e.g., NotEmpty, length, format). Deep business validation and invariants MUST be enforced inside the Rich Domain Entities.
- **Types Auto-Registration:** Ensure services/handlers explicitly implements the custom interfaces (IScopedType, ITransientType, ISingletonType) from `LudaFit.SharedKernel.Interfaces` to guarantee automatic DI registration.
- **Endpoints Auto-Registration:** Ensure Endpoint classes explicitly implements `LudaFit.Core.Features.Common.Interfaces.IEndpoint` to guarantee automatic registration.

## Available Skills & Tools
- **Unit Testing:** Use the `$unit-tests` skill for any testing tasks. Follow its internal xUnit/FluentAssertions rules.
- **Vertical Slices:** Use `$vertical-slice` to scaffold new features.
- **Bulk Vertical Slices:** Use `$bulk-vertical-slices` to scaffold new CRUD features group for specific entity.

## Workspace Commands
- **Build:** `dotnet build LudaFit.Server.slnx`
- **Run Tests:** `dotnet test LudaFit.Tests`
