## Insurance Analytics Platform – Visual Overview

Architecture at a glance (Mermaid diagram):

```mermaid
flowchart TD
    subgraph User
        Browser[Browser UI]
    end

    subgraph Frontend
        UI[index.html / app.js / styles.css]
    end

    subgraph Backend[InsuranceAnalytics.Api]
        Controllers[Controllers (Auth, KPI, Policies, Claims, Filters)]
        Services[Services (Auth/Kpi/Policy/Claim/Filter)]
        DbCtx[EF Core DbContext]
    end

    subgraph Data[SQLite]
        DB[(insurance_analytics.db)]
    end

    Browser --> UI
    UI -->|HTTP JSON| Controllers
    Controllers --> Services
    Services --> DbCtx
    DbCtx --> DB

    Controllers -->|JWT auth| Browser
```

### Key flows
- Login → `/api/auth/login` (JWT) → token stored in localStorage
- Dashboard → `/api/filters`, `/api/kpi` → KPI cards, tables, pie chart
- Policies tab → `/api/policies` (list) and POST create (Admin)
- Claims tab → `/api/claims` (list) and POST create (Admin)

### Components
- **Frontend:** vanilla JS/HTML/CSS in `backend/src/InsuranceAnalytics.Api/wwwroot/`
- **Backend:** ASP.NET Core controllers + services in `backend/src/InsuranceAnalytics.Api` and `Infrastructure`
- **Data:** SQLite file `insurance_analytics.db` (seed/demo data)

### Credentials
- Admin: `admin / Admin@123`
- User: `user / User@123`

For a detailed request/response sequence, see `README_FLOW.md`.