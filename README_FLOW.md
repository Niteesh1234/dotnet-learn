## Insurance Analytics Platform – Flow Overview

This diagram and notes explain the request/response flow for the app (frontend in `wwwroot`, backend API, and DB/seed data).

```mermaid
sequenceDiagram
    autonumber
    actor User
    participant UI as Frontend (index.html/app.js)
    participant API as Backend API (InsuranceAnalytics.Api)
    participant DB as SQLite DB (insurance_analytics.db)

    User->>UI: Open / (static files)
    UI-->>API: GET /api/filters (initial dropdown data)
    API-->>DB: Query Regions, PolicyTypes, ClaimStatuses
    DB-->>API: Rows
    API-->>UI: JSON filters

    User->>UI: Login (admin/Admin@123 or user/User@123)
    UI-->>API: POST /api/auth/login (username/password)
    API-->>DB: Validate user, issue JWT
    API-->>UI: { token, role, expiresAtUtc }
    UI->>UI: Save token/role in localStorage

    UI-->>API: GET /api/kpi?[fromDate&toDate&region&policyType]
    API-->>DB: Aggregate KPIs + trends + region breakdown
    DB-->>API: Aggregated results
    API-->>UI: KPI/trend/region JSON
    UI->>UI: Render KPI cards, tables, pie chart

    User->>UI: Switch to Policies tab
    UI-->>API: GET /api/policies?[filters]
    API-->>DB: Query policies + claim counts
    DB-->>API: Rows
    API-->>UI: Policy list
    UI->>UI: Render table; populate policy select for claims form

    User->>UI: Switch to Claims tab
    UI-->>API: GET /api/claims?[filters]
    API-->>DB: Query claims with policy/customer info
    DB-->>API: Rows
    API-->>UI: Claim list
    UI->>UI: Render claims table

    Note over UI,API: Admin only can POST create policy/claim
    UI-->>API: POST /api/policies (Admin)
    API-->>DB: Insert policy
    API-->>UI: 201 Created
    UI->>UI: Refresh policies (and KPI dashboard)

    UI-->>API: POST /api/claims (Admin)
    API-->>DB: Insert claim
    API-->>UI: 201 Created
    UI->>UI: Refresh claims (and KPI dashboard)

    User->>UI: Logout
    UI->>UI: Clear token/role; show login
```

### Components
- **Frontend:** `backend/src/InsuranceAnalytics.Api/wwwroot/index.html`, `styles.css`, `app.js`
  - Tabs: Dashboard, Policies, Claims
  - Widgets: KPI cards, monthly trend table, region table, pie chart (premium vs claims)
  - Forms: filter form; admin-only create forms for policy/claim
- **Backend API:** controllers in `backend/src/InsuranceAnalytics.Api/Controllers` calling services in Infrastructure.
  - Auth: `/api/auth/login` issues JWT (admin/user demo credentials).
  - KPI: `/api/kpi` aggregates monthlyTrends, regionBreakdown, KPIs.
  - Filters: `/api/filters` provides regions, policyTypes, claimStatuses.
  - Policies: `/api/policies` GET/POST
  - Claims: `/api/claims` GET/POST
- **Data:** SQLite (`insurance_analytics.db`) with seeded/demo data; optional seed endpoint for local testing.

### Happy-path checklist
1) **Login** with provided credentials; token stored in localStorage
2) **Filters** loaded -> dashboard KPIs/trends/region/pie render
3) **Tabs**: Policies/Claims fetch lists; admin-only create forms visible for Admin
4) **Create** policy/claim (Admin) -> lists refresh -> dashboard refreshed
5) **Logout** clears token and UI state

### Credentials
- Admin: `admin / Admin@123`
- User: `user / User@123`