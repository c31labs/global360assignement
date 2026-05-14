# TaskFlow Reference Implementation

A one-screen task management app for a marketing agency, written as the reference
project a new delivery team should look at before they start writing production code.
The brief asked for breadth over depth to be *avoided*, so this repo is deliberately
small. What's here exists to demonstrate **how** we'd structure and operate the
service, not to ship every feature.

The companion delivery slides (architecture, roadmap, technical leadership) live in
[`docs/presentation/`](docs/presentation/) and are published as a public link via
GitHub Pages. See [`docs/presentation/README.md`](docs/presentation/README.md).

---

## What's in the box

| Area     | Stack                                                                            |
| -------- | -------------------------------------------------------------------------------- |
| Frontend | Angular 18 standalone components, signals, reactive forms, RxJS, SCSS            |
| Backend  | .NET 8 Web API (minimal endpoints), EF Core 8 + SQLite, FluentValidation, Serilog |
| Tests    | xUnit + FluentAssertions + NSubstitute (API), Jasmine/Karma (web)                 |

---

## Prerequisites

* **.NET 8 SDK**. Install with `brew install --cask dotnet-sdk` (sudo) or
  `curl -sSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 8.0 --install-dir $HOME/.dotnet`
  then `export PATH="$HOME/.dotnet:$PATH"`.
* **Node 20+** and **npm 10+** (or use [nvm](https://github.com/nvm-sh/nvm)).
* **Google Chrome** (or set `CHROME_BIN`) for `npm run test`.

Verify:

```bash
dotnet --version   # 8.x
node --version     # >= 20
```

---

## Run it

Two terminals. Backend first so the proxy has something to talk to.

### Terminal 1: API (`http://localhost:5080`)

```bash
cd api
dotnet run --project src/TaskFlow.Api
```

* Swagger UI: <http://localhost:5080/swagger>
* Health: <http://localhost:5080/health>
* The first run creates `taskflow.db` (SQLite, gitignored).

### Terminal 2: Web (`http://localhost:4200`)

```bash
cd web
npm install   # first time only
npm start
```

The dev server proxies `/api/*` and `/health` to the API (see
[`web/proxy.conf.json`](web/proxy.conf.json)), so requests stay relative and the
frontend is environment agnostic.

---

## Run the tests

```bash
# API: 11 domain + 6 application + 7 integration = 24 tests
cd api && dotnet test

# Web: service, store, form, interceptor = 14 tests
cd ../web && npm test -- --watch=false
```

`npm run test:ci` runs the same suite in headless Chrome with coverage output.

---

## How the solution is organised

```
.
├── api/
│   ├── TaskFlow.sln
│   ├── Directory.Build.props            # Nullable, warnings-as-errors, analyser level
│   ├── src/
│   │   ├── TaskFlow.Domain/             # Aggregates, value types, domain exceptions (no deps)
│   │   ├── TaskFlow.Application/        # DTOs, validators, service contracts + implementation
│   │   ├── TaskFlow.Infrastructure/     # EF Core DbContext, configurations, repository
│   │   └── TaskFlow.Api/                # Minimal endpoints, middleware, composition root
│   └── tests/
│       ├── TaskFlow.Domain.Tests/       # Pure unit tests for invariants
│       ├── TaskFlow.Application.Tests/  # Service logic with NSubstitute mocks
│       └── TaskFlow.Api.IntegrationTests/  # WebApplicationFactory + in-memory SQLite
└── web/
    └── src/app/
        ├── core/
        │   ├── http/error.interceptor.ts        # Global ProblemDetails to notification
        │   ├── notifications/                   # Toast bus + host component
        │   └── tokens/api-base-url.token.ts     # InjectionToken (testable, proxy-friendly)
        └── features/tasks/
            ├── data/                            # Model, API client, signal-based store
            └── ui/                              # Board, form, card components
```

### Boundaries on the API side

* **Domain** has no references to ASP.NET, EF Core, or anything infrastructural. The
  `TaskItem` aggregate exposes factory methods and behaviour methods (`UpdateDetails`,
  `ChangeStatus`) and enforces its own invariants, so the application layer cannot
  construct an invalid task. Domain validation throws `DomainValidationException`,
  input validation throws `FluentValidation.ValidationException`, and missing records
  throw `TaskNotFoundException`. All three are translated centrally by
  [`ExceptionHandlingMiddleware`](api/src/TaskFlow.Api/Middleware/ExceptionHandlingMiddleware.cs)
  into RFC 7807 `application/problem+json` payloads.

* **Application** orchestrates. Validators run first, then domain operations, then
  persistence. The `TaskService` depends on `ITaskRepository` (defined in this layer)
  and is the only thing the API endpoints know about. Mapping to DTOs lives in an
  extension method next to the DTOs. That keeps the unit tests fast and lets us swap
  mappers later if we ever need to.

* **Infrastructure** owns persistence concerns. The EF Core value converter on
  `DateTimeOffset` is there because SQLite cannot `ORDER BY` it natively. See the
  comment on `TaskItemConfiguration.UtcConverter`. On Postgres/SQL Server this
  converter is removed.

* **Api** is the composition root. Wiring lives here and nowhere else:
  Serilog, CORS, OpenAPI, health checks, the exception middleware, and endpoint
  routing. The `Program.cs` is small because each concern has an extension method in
  its owning layer (`AddApplication`, `AddInfrastructure`).

### Boundaries on the Angular side

* **core/** holds cross-cutting infrastructure that any feature can reuse, like HTTP
  error handling, notifications, and DI tokens.
* **features/tasks/** is a vertical slice. `data/` is everything off component
  (model, API client, signal-based `TaskStore`). `ui/` is the rendered shell.
  Components are `OnPush`, standalone, and read signals through computed views.
* The store does not catch errors from HTTP. The interceptor does. The store *does*
  manage local state (optimistic insertions, busy flags) and surface user-facing
  success notifications.

---

## What we test, and why

Tests are not graded by count. They're graded by what they tell a new developer
the codebase considers important. The breakdown:

| Test                                       | Why it exists                                                                                       |
| ------------------------------------------ | --------------------------------------------------------------------------------------------------- |
| `TaskItemTests`                            | Locks down the domain invariants (title required/trimmed, no past due dates, status transitions).   |
| `TaskServiceTests`                         | Verifies orchestration: validation runs before domain, errors propagate, mutations are persisted.   |
| `TasksEndpointsTests`                      | End-to-end through the real HTTP pipeline, real EF Core, real middleware. Matches production behaviour. |
| `TaskApiServiceTests` (Angular)            | URL contract: paths, methods, payload shape. Cheap regression guard.                                |
| `TaskStoreTests` (Angular)                 | State transition behaviour: grouping, optimistic insert, delete, loading/error flags.               |
| `TaskFormComponentTests`                   | Form invariants: disabled when invalid, trims, emits the normalised shape we expect.                |
| `errorInterceptorSpec`                     | The user-facing contract for "API returns ProblemDetails". Protects the most common error path.     |

Things we deliberately do **not** test in this repo: third party libraries, generated
code, and trivial getters. The Directory.Build.props raises analyser severity rather
than adding "code style" tests.

---

## Configuration, errors, logging. The boring stuff that matters.

* **Configuration.** API reads `appsettings.json` + `appsettings.{Environment}.json`
  plus environment variables. Connection strings, allowed CORS origins, and Serilog
  levels live there. The Angular side uses an `InjectionToken` (`API_BASE_URL`) so
  tests can inject a deterministic value and the dev server proxy can keep paths
  relative.
* **Errors.** One middleware. Every endpoint emits the same RFC 7807 shape, every
  client maps the same response. Logs include `traceId` for correlation.
* **Logging.** Serilog with structured properties (`Application`, `TaskId`,
  request log including duration). Default outputs to console. In production you'd
  add a sink (Seq, or OpenTelemetry into your APM).
* **Validation.** Two layers, two purposes. FluentValidation for the input shape,
  domain methods for the invariants. The API surface is "first failure returns 400 with
  field errors", and the integration tests verify the contract.
* **Health.** `/health` runs a `DbContextCheck` against the real database. Returns
  a small JSON payload (status + per check timings). Production would wire this to
  the orchestrator's liveness and readiness probes.

---

## Decisions worth flagging in interview

1. **Minimal APIs over Controllers.** Less ceremony for CRUD; the file is the route
   map. Easy to swap to Controllers if the team prefers. `Program.cs` would gain
   `AddControllers/MapControllers`, the rest doesn't move.
2. **Repository interface in Application, implementation in Infrastructure.** Lets
   the Application layer be unit-testable without EF Core, and is the seam we'd use
   if we moved to Dapper or split read/write models later.
3. **SQLite for the prototype.** Zero friction local dev. The EF migration path to
   Postgres/SQL Server is a connection string change plus removing the value
   converter. I would not run SQLite in production for this workload.
4. **`TimeProvider` (not `DateTimeOffset.UtcNow`).** Lets tests advance time
   deterministically without `DateTime.Now` lurking anywhere in the domain.
5. **No auth in the prototype.** Out of scope for a one screen reference, but the
   API is structured for it. Endpoints would gain `[Authorize]`, `Program.cs` would
   gain a JWT bearer scheme + tenancy claim. The data model anticipates this via
   the `Assignee` string field. A multi user version would replace it with a `User`
   reference.
6. **Signals + RxJS together.** Components consume signals (synchronous, ergonomic
   templates). Side-effects (HTTP, optimistic updates) still flow through RxJS
   pipelines because that's what `HttpClient` returns and where `tap`/`finalize`
   shine. The store hides that mix; templates only see signals.
7. **One screen, but three components anyway.** `TaskBoardComponent` is smart and
   owns state, `TaskFormComponent` and `TaskCardComponent` are dumb. This is the
   shape any team should reproduce: container coordinates, presentational components
   stay pure and unit-testable.

---

## Things explicitly out of scope (and how I'd add them)

| Out of scope          | Production approach                                                                          |
| --------------------- | -------------------------------------------------------------------------------------------- |
| Authentication / RBAC | OIDC (Entra ID / Auth0) into JWT bearer + role/scope claims; tenant on every query.          |
| Real-time updates     | SignalR hub broadcasting `TaskChanged` events. The store would replace local mutations with subscriptions. |
| Pagination & filters  | Server side filter DTO + cursor pagination; Angular table virtualisation if list > ~200.    |
| Outbox / domain events| MediatR + outbox table for fan-out to notifications, search, audit.                          |
| Observability         | OpenTelemetry traces/metrics; ship to Honeycomb or Application Insights; alert on SLO burn.  |
| CI/CD                 | GitHub Actions: `dotnet test`, `npm test:ci`, build container, deploy via IaC; PR previews.  |

These all bolt on without rewriting the existing layers. That's the point of the
boundaries above.

---

## Repo housekeeping

* All source files compile with `warnings-as-errors`.
* `dotnet format` and `ng lint` are wired through standard tooling. See
  `Directory.Build.props` for analyser rules.
* SQLite database files (`*.db`, `*.db-shm`, `*.db-wal`) and editor scratch
  directories are git-ignored.

---

If you want to discuss any specific decision, jump to the file. The choices that
matter are commented inline rather than buried in this README.
