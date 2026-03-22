# Insurance Analytics Platform

A .NET-based web application for insurance analytics with a dashboard, policy management, and claims tracking.

## Features

- **Authentication**: JWT-based login with Admin and User roles
- **Dashboard**: KPI metrics, filters, and trend analysis
- **Policy Management**: View and create insurance policies (Admin only)
- **Claims Management**: View and create claims (Admin only)

## Tech Stack

- **Backend**: ASP.NET Core Web API (.NET 10)
- **Database**: SQLite with Entity Framework Core
- **Frontend**: Vanilla JavaScript, HTML, CSS
- **Authentication**: JWT tokens

## Getting Started

1. **Prerequisites**
   - .NET 10 SDK
   - A web browser

2. **Run the Application**
   ```bash
   cd backend/src/InsuranceAnalytics.Api
   dotnet run
   ```

3. **Access the Application**
   - Open http://localhost:5295 in your browser
   - Login with:
     - Admin: username `admin`, password `Admin@123`
     - User: username `user`, password `User@123`

## API Endpoints

- `POST /api/auth/login` - User authentication
- `GET /api/kpi` - Dashboard KPIs with optional filters
- `GET /api/filters` - Filter options (regions, policy types)
- `GET /api/policies` - List policies (paginated)
- `POST /api/policies` - Create policy (Admin only)
- `GET /api/claims` - List claims (paginated)
- `POST /api/claims` - Create claim (Admin only)

## Database

The application uses SQLite with seeded sample data including policies and claims.

## Project Structure

- `backend/src/InsuranceAnalytics.Api/` - Web API project
- `backend/src/InsuranceAnalytics.Core/` - Core entities, DTOs, interfaces
- `backend/src/InsuranceAnalytics.Infrastructure/` - Data access, services</content>
<parameter name="filePath">/Users/niteeshdoppalapudi/Desktop/learning.net/README.md