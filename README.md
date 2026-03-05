# Clean Architecture Template

## Running the application

Login and signup (and all API calls) are proxied from the frontend to the backend API. **Start the API first**, then the frontend.

1. **Start the backend API** (from repo root):
   ```bash
   cd src/Web.Api
   dotnet run
   ```
   The API listens on **https://localhost:5001** (and http://localhost:5000). Leave this terminal running.

2. **Start the frontend** (in another terminal):
   ```bash
   cd frontend
   npm start
   # or: npx ng serve
   ```
   Open http://localhost:4200. If you see `[vite] http proxy error` / `ECONNREFUSED` on login or signup, the API is not running—go back to step 1.

---

What's included in the template?

- SharedKernel project with common Domain-Driven Design abstractions.
- Domain layer with sample entities.
- Application layer with abstractions for:
  - CQRS
  - Example use cases
  - Cross-cutting concerns (logging, validation)
- Infrastructure layer with:
  - Authentication
  - Permission authorization
  - EF Core, PostgreSQL
  - Serilog
- Seq for searching and analyzing structured logs
  - Seq is available at http://localhost:8081 by default
- Testing projects
  - Architecture testing

I'm open to hearing your feedback about the template and what you'd like to see in future iterations.

If you're ready to learn more, check out [**Pragmatic Clean Architecture**](https://www.milanjovanovic.tech/pragmatic-clean-architecture?utm_source=ca-template):

- Domain-Driven Design
- Role-based authorization
- Permission-based authorization
- Distributed caching with Redis
- OpenTelemetry
- Outbox pattern
- API Versioning
- Unit testing
- Functional testing
- Integration testing

Stay awesome!
