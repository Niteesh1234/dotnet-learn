## C# in this project (quick guide for newcomers)

This repo uses **C# with ASP.NET Core** and **Entity Framework Core**. The frontend is plain JS/HTML/CSS served from `wwwroot`, and the backend is a Web API returning JSON.

### Project layout
- **API (presentation)**: `backend/src/InsuranceAnalytics.Api`
  - Controllers (HTTP endpoints) in `Controllers/`
  - Static frontend in `wwwroot/`
- **Core (domain/contracts)**: `backend/src/InsuranceAnalytics.Core`
  - Entities, DTOs, interfaces, filters/enums
- **Infrastructure (data/services)**: `backend/src/InsuranceAnalytics.Infrastructure`
  - EF Core DbContext, services implementing Core interfaces, seeding

### Typical C# flow
1) **Controller** receives HTTP request, validates input, calls a **service**.
2) **Service** performs business logic and data access via **DbContext** (EF Core).
3) Controller returns DTOs as JSON.

### Key C# concepts used here
- **Dependency Injection**: Services are registered in `DependencyInjection.cs` and injected into controllers via constructors.
- **Async/await**: Data access and service calls are asynchronous (e.g., `await _db.Policies.ToListAsync()`).
- **DTOs**: Data Transfer Objects in `Core/DTOs` shape request/response payloads; EF entities are not sent directly to clients.
- **Interfaces**: Contracts in `Core/Interfaces` (e.g., `IKpiService`) decouple API from implementation.
- **Entity Framework Core**: `InsuranceAnalyticsDbContext` maps entities (`Policy`, `Claim`) to SQLite tables.
- **Authentication/Authorization**: JWT-based; controllers use `[Authorize]` (and roles for admin-only actions).

### Where to look
- Controllers: `backend/src/InsuranceAnalytics.Api/Controllers/*.cs`
- Services: `backend/src/InsuranceAnalytics.Infrastructure/Services/*.cs`
- DbContext: `backend/src/InsuranceAnalytics.Infrastructure/Data/InsuranceAnalyticsDbContext.cs`
- DI wiring: `backend/src/InsuranceAnalytics.Infrastructure/DependencyInjection.cs`

### Minimal controller pattern (C#)
```csharp
[ApiController]
[Route("api/[controller]")]
public class PoliciesController : ControllerBase
{
    private readonly IPolicyService _service;

    public PoliciesController(IPolicyService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PolicyDto>>> Get([FromQuery] PolicyFilter filter)
    {
        var result = await _service.GetPoliciesAsync(filter);
        return Ok(result);
    }
}
```

### Minimal service pattern (C#)
```csharp
public class PolicyService : IPolicyService
{
    private readonly InsuranceAnalyticsDbContext _db;

    public PolicyService(InsuranceAnalyticsDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<PolicyDto>> GetPoliciesAsync(PolicyFilter filter)
    {
        var query = _db.Policies.AsQueryable();
        // apply filters, projection
        return await query
            .Select(p => new PolicyDto
            {
                PolicyId = p.PolicyId,
                CustomerName = p.CustomerName,
                PremiumAmount = p.PremiumAmount,
                Region = p.Region
            })
            .ToListAsync();
    }
}
```

### Running and trying it out
```bash
cd backend/src/InsuranceAnalytics.Api
dotnet run --urls http://localhost:5297
# Open http://localhost:5297/ and log in (admin/Admin@123 or user/User@123)
```

### Why C# / ASP.NET Core is useful here
- Strong typing and async-first APIs reduce runtime bugs.
- Integrated DI and middleware make auth, logging, and validation composable.
- EF Core provides LINQ-based data access and migrations; easy to swap SQLite for SQL Server later.
- First-class JSON (System.Text.Json) and minimal hosting model simplify API setup.

### Next steps to learn
- Open controllers and services side-by-side to see request → service → DbContext flow.
- Add a small endpoint (e.g., read-only `/api/health`) to get comfortable.
- Switch DB provider in configuration (SQLite → SQL Server) to practice EF Core provider changes.
- Write a unit test for a service method (injecting an in-memory DbContext).