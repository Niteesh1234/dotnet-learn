## Multi-stage build for InsuranceAnalytics API
# Uses .NET 10 SDK/runtime. If Render runtime lags, change to 8.0 and retarget csproj.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and restore
COPY InsuranceAnalyticsPlatform.slnx ./
COPY backend/src/InsuranceAnalytics.Core/InsuranceAnalytics.Core.csproj backend/src/InsuranceAnalytics.Core/
COPY backend/src/InsuranceAnalytics.Infrastructure/InsuranceAnalytics.Infrastructure.csproj backend/src/InsuranceAnalytics.Infrastructure/
COPY backend/src/InsuranceAnalytics.Api/InsuranceAnalytics.Api.csproj backend/src/InsuranceAnalytics.Api/

RUN dotnet restore backend/src/InsuranceAnalytics.Api/InsuranceAnalytics.Api.csproj

# Copy all source
COPY backend/src ./backend/src

# Publish
RUN dotnet publish backend/src/InsuranceAnalytics.Api/InsuranceAnalytics.Api.csproj -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# For Render/containers, respect PORT env and HTTP
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080}
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

COPY --from=build /app/publish .

# Expose default port (Render sets PORT)
EXPOSE 8080

ENTRYPOINT ["dotnet", "InsuranceAnalytics.Api.dll"]