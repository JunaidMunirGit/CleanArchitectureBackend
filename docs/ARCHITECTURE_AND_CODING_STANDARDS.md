# Clean Architecture — Deep Dive Guide

This document explains the **architecture**, **coding standards**, and **patterns** used in this project so you can learn and extend it confidently.

---

## 1. Solution Structure (Clean Architecture Layers)

The solution follows **Clean Architecture** with clear dependency rules:

```
src/
├── SharedKernel/     ← Shared by all layers (DDD primitives, Result, Error)
├── Domain/           ← Core business entities and events (no dependencies)
├── Application/      ← Use cases, CQRS, interfaces (depends on Domain + SharedKernel)
├── Infrastructure/   ← DB, auth, external services (implements Application interfaces)
└── Web.Api/          ← HTTP API, endpoints (Presentation layer)
tests/
└── ArchitectureTests/  ← Enforces layer rules
```

### Dependency rules (enforced by tests)

| Layer          | Must NOT depend on        |
|----------------|---------------------------|
| **Domain**     | Application, Infrastructure, Web.Api |
| **Application**| Infrastructure, Web.Api   |
| **Infrastructure** | Web.Api              |

So: **Domain** is innermost; **Web.Api** is outermost. Dependencies point **inward** only.

---

## 2. Project-by-Project Role

### SharedKernel

- **Error** — Typed errors with `Code`, `Description`, `ErrorType` (Failure, Validation, NotFound, Conflict).
- **Result / Result&lt;T&gt;** — Success/failure return type; no exceptions for expected failures.
- **Entity** — Base for domain entities; holds **domain events** and `Raise()` / `ClearDomainEvents()`.
- **IDomainEvent** — Marker for domain events.
- **IDomainEventHandler&lt;T&gt;** — Handler contract for domain events.
- **ValidationError** — Special error that carries a list of validation errors.

Used by Domain, Application, and optionally Infrastructure/Web.Api.

### Domain

- **Entities**: e.g. `User`, `TodoItem` (inherit `Entity`).
- **Value objects**: e.g. `Priority` (enum).
- **Domain events**: e.g. `TodoItemCreatedDomainEvent`, `UserRegisteredDomainEvent`.
- **Domain errors**: e.g. `UserErrors.NotFound()`, `TodoItemErrors.NotFound()`.

**No references** to Application, Infrastructure, or Web.Api. Pure business concepts.

### Application

- **CQRS**:
  - **Commands**: `ICommand` (void) and `ICommand<TResponse>`.
  - **Queries**: `IQuery<TResponse>`.
  - Handlers: `ICommandHandler<TCommand>`, `ICommandHandler<TCommand, TResponse>`, `IQueryHandler<TQuery, TResponse>`.
- **Use cases**: One folder per feature (e.g. `Todos/Create`, `Users/Register`) containing:
  - Command/Query (request DTO).
  - Handler (business logic).
  - Validator (FluentValidation).
  - Response DTOs when needed.
- **Abstractions**: Interfaces for data access (`IApplicationDbContext`), auth (`IUserContext`, `IPasswordHasher`, `ITokenProvider`), time (`IDateTimeProvider`), and domain events (`IDomainEventsDispatcher`).
- **Behaviors (decorators)**:
  - **ValidationDecorator** — Runs FluentValidation before the handler.
  - **LoggingDecorator** — Logs command/query name and result (success/error).

Application **never** references Infrastructure or Web.Api; it only defines interfaces.

### Infrastructure

- **Database**: EF Core, PostgreSQL, `ApplicationDbContext` implementing `IApplicationDbContext`.
- **Authentication**: JWT, `UserContext`, `PasswordHasher`, `TokenProvider`.
- **Authorization**: Permission-based (`PermissionAuthorizationHandler`, `PermissionRequirement`).
- **Domain events**: `DomainEventsDispatcher` dispatches events after `SaveChangesAsync`.
- **Time**: `DateTimeProvider` for testable `UtcNow`.

Implements Application interfaces and references Application + Domain + SharedKernel.

### Web.Api (Presentation)

- **Minimal APIs** via **IEndpoint** — Each endpoint class implements `void MapEndpoint(IEndpointRouteBuilder app)`.
- **Endpoint discovery**: `AddEndpoints(Assembly)` registers all `IEndpoint` implementations; `MapEndpoints()` maps them.
- **Request/Response**: Endpoint-specific `Request` DTOs; handlers return `Result`/`Result<T>`; `result.Match(Results.Ok, CustomResults.Problem)` turns result into HTTP response.
- **GlobalExceptionHandler** — Unhandled exceptions → 500 ProblemDetails.
- **Middleware**: Request context logging, Serilog request logging.

Web.Api references Application and Infrastructure (and thus Domain, SharedKernel).

---

## 3. Request Flow (End-to-End)

Example: **POST /todos** (create todo).

1. **Web.Api** — `Create` endpoint receives `Request`, builds `CreateTodoCommand`, resolves `ICommandHandler<CreateTodoCommand, Guid>`.
2. **Pipeline** (order of decorators in DI):
   - **Validation** — `ValidationDecorator` runs `CreateTodoCommandValidator`; on failure returns `Result.Failure<T>(ValidationError)`.
   - **Logging** — `LoggingDecorator` logs "Processing command CreateTodoCommand", then calls inner handler.
3. **Handler** — `CreateTodoCommandHandler`:
   - Uses `IUserContext` to check user.
   - Uses `IApplicationDbContext` to load user and add `TodoItem`.
   - Calls `todoItem.Raise(new TodoItemCreatedDomainEvent(...))`.
   - Calls `context.SaveChangesAsync(cancellationToken)`.
4. **Infrastructure** — On save, `DomainEventsDispatcher` runs for each domain event; any `IDomainEventHandler<T>` for that event is invoked.
5. **Response** — Handler returns `Result<Guid>` (e.g. `todoItem.Id`). Decorator returns it. Endpoint uses `result.Match(Results.Ok, CustomResults.Problem)`.

So: **Endpoint → Validation → Logging → Handler → DbContext (and domain events)**.

---

## 4. Coding Standards (from .cursorrules and .editorconfig)

### General

- **C# 12** where appropriate.
- **SOLID**, **dependency injection**, **async/await** for I/O.
- **Explicit typing**: Prefer clear types; use `var` only when type is evident.
- **internal sealed by default** for types unless they must be public.
- **Guid** for identifiers unless there’s a good reason otherwise.
- **Null checks**: Use `is null` / `is not null`, not `== null` / `!= null`.

### Naming and style

- **Interfaces**: Start with `I` (e.g. `ICommandHandler`, `IApplicationDbContext`).
- **PascalCase** for types and non-field members.
- **File-scoped namespaces**: `namespace Application.Todos.Create;`
- **Usings**: Outside namespace; system directives first.

### EditorConfig (selected)

- **Indent**: 4 spaces.
- **Braces**: Always use `{ }`; no single-line blocks where brace is omitted.
- **Prefer**:
  - `??` (coalesce), collection/object initializers, `?.`, `is null`, compound assignment, conditional expressions where configured.
- **var**: Only when type is apparent (`csharp_style_var_when_type_is_apparent = true`); otherwise explicit type.

### Patterns used in this template

- **Primary constructors** for DI in handlers, decorators, and services.
- **Records** for immutable DTOs and domain events.
- **Structured logging** (e.g. Serilog, `LogContext` in decorators).
- **Strongly-typed configuration** (e.g. `IOptions`) where applicable.
- **FluentValidation** for command/query validation in the Application layer.

---

## 5. Key Patterns in Code

### Result-based error handling

Handlers return `Result` or `Result<T>` instead of throwing for expected failures:

```csharp
if (user is null)
    return Result.Failure<Guid>(UserErrors.NotFound(command.UserId));
// ...
return todoItem.Id;  // implicit Success
```

API layer maps failure to HTTP:

```csharp
return result.Match(Results.Ok, CustomResults.Problem);
```

`CustomResults.Problem` maps `ErrorType` to status codes (e.g. NotFound → 404, Validation → 400).

### CQRS

- **Command**: changes state; can return `void` (`ICommand`) or `TResponse` (`ICommand<TResponse>`).
- **Query**: read-only; always `IQuery<TResponse>`.
- One handler per command/query; registered by Scrutor in `Application/DependencyInjection.cs`.

### Decorator order (DI)

1. Register concrete handlers (Scrutor scan).
2. Decorate with **Validation** (so validation runs first).
3. Decorate with **Logging** (so every call is logged).

So pipeline is: **Logging → Validation → Handler**.

### Domain events

- Entity inherits `Entity` and calls `Raise(new SomeDomainEvent(...))`.
- Events are collected and dispatched after `SaveChangesAsync` (in Infrastructure).
- Handlers implement `IDomainEventHandler<TEvent>` and are resolved from the container per event type.

### Endpoint organization

- One class per endpoint (or small group), implementing `IEndpoint`.
- `MapEndpoint` defines route, binds `Request`, resolves handler, calls handler, then `result.Match(..., CustomResults.Problem)`.
- Tags and authorization (e.g. `.RequireAuthorization()`, `.HasPermission("...")`) on the route.

---

## 6. How to Add a New Feature

1. **Domain** (if needed): Add or extend entity, domain event, or domain errors.
2. **Application**:
   - Add folder, e.g. `FeatureName/Action/`.
   - Define command or query (implement `ICommand<T>` / `IQuery<T>`).
   - Add handler (implement `ICommandHandler<...>` / `IQueryHandler<...>`).
   - Add FluentValidation validator.
   - Add response DTOs if needed.
3. **Infrastructure** (if needed): Implement or extend interfaces (e.g. new `IDomainEventHandler`).
4. **Web.Api**: Add endpoint class implementing `IEndpoint`, map route and call handler, use `result.Match(Results.Ok, CustomResults.Problem)`.

No need to register handlers or endpoints by hand: **Scrutor** and **IEndpoint** discovery do it.

---

## 7. Testing

- **ArchitectureTests**: Use NetArchTest to enforce that Domain does not depend on Application/Infrastructure/Presentation, Application does not depend on Infrastructure/Presentation, and Infrastructure does not depend on Presentation.
- Run these tests when changing project references or moving types between layers.

---

## 8. Quick Reference

| Concept            | Location / Example                                      |
|--------------------|---------------------------------------------------------|
| Command (no return)| `ICommand` + `ICommandHandler<TCommand>`                |
| Command (with return) | `ICommand<TResponse>` + `ICommandHandler<TCommand, TResponse>` |
| Query              | `IQuery<TResponse>` + `IQueryHandler<TQuery, TResponse>`|
| Validation         | FluentValidation + `ValidationDecorator`                |
| Logging            | `LoggingDecorator` + Serilog                            |
| Errors             | `Error`, `ValidationError`, `Result` / `Result<T>`      |
| Domain events      | `Entity.Raise()` + `IDomainEventHandler<T>` + `IDomainEventsDispatcher` |
| HTTP errors        | `CustomResults.Problem(result)`                         |
| Layer rules        | `tests/ArchitectureTests/Layers/LayerTests.cs`          |

---

For more on the ideas behind this template (DDD, authorization, caching, testing), see [Pragmatic Clean Architecture](https://www.milanjovanovic.tech/pragmatic-clean-architecture).























--------------------------------------------
this documents from cursor ai

# ASP.NET Core 8 Project Rules

- Use C# 12 language features where appropriate
- Follow SOLID principles in class and interface design
- Implement dependency injection for loose coupling
- Use primary constructors for dependency injection in services, use cases, etc.
- Use async/await for I/O-bound operations
- Prefer record types for immutable data structures
- Prefer controller endpoints over minimal APIs
  - Utilize minimal APIs for simple endpoints (when explicitly stated or when it makes sense)
- Implement proper exception handling and logging
- Use strongly-typed configuration with IOptions pattern
- Implement proper authentication and authorization
- Use Entity Framework Core for database operations
- Implement unit tests for business logic
- Use integration tests for API endpoints
- Implement proper versioning for APIs
- Implement proper caching strategies
- Use middleware for cross-cutting concerns
- Implement health checks for the application
- Use environment-specific configuration files
- Implement proper CORS policies
- Use secure communication with HTTPS
- Implement proper model validation
- Use Swagger/OpenAPI for API documentation
- Implement proper logging with structured logging
- Use background services for long-running tasks
- Favor explicit typing (this is very important). Only use var when evident.
- Make types internal and sealed by default unless otherwise specified
- Prefer Guid for identifiers unless otherwise specified
- Use `is null` checks instead of `== null`. The same goes for `is not null` and `!= null`.

