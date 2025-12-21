# ============================================
# Dockerfile for Payment.Api
# ECommerce Microservices Architecture
# ============================================

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copy solution file
COPY ["ECommerce.sln", "./"]

# Copy Common projects
COPY ["src/Common/Common.ApiKey/Common.ApiKey.csproj", "src/Common/Common.ApiKey/"]
COPY ["src/Common/Common.Caching/Common.Caching.csproj", "src/Common/Common.Caching/"]
COPY ["src/Common/Common.CorrelationId/Common.CorrelationId.csproj", "src/Common/Common.CorrelationId/"]
COPY ["src/Common/Common.Logging/Common.Logging.csproj", "src/Common/Common.Logging/"]
COPY ["src/Common/Common.Messaging/Common.Messaging.csproj", "src/Common/Common.Messaging/"]
COPY ["src/Common/Common.RateLimiting/Common.RateLimiting.csproj", "src/Common/Common.RateLimiting/"]
COPY ["src/Common/Common.Validation/Common.Validation.csproj", "src/Common/Common.Validation/"]
COPY ["src/Common/Common.Database/Common.Database.csproj", "src/Common/Common.Database/"]

# Copy Service Common projects
COPY ["src/Services/Service.Common.Authentication/Service.Common.Authentication.csproj", "src/Services/Service.Common.Authentication/"]
COPY ["src/Services/Service.Common.Collection/Service.Common.Collection.csproj", "src/Services/Service.Common.Collection/"]
COPY ["src/Services/Service.Common.Mapping/Service.Common.Mapping.csproj", "src/Services/Service.Common.Mapping/"]
COPY ["src/Services/Service.Common.Paging/Service.Common.Paging.csproj", "src/Services/Service.Common.Paging/"]

# Copy Payment projects
COPY ["src/Services/Payment/Payment.Api/Payment.Api.csproj", "src/Services/Payment/Payment.Api/"]
COPY ["src/Services/Payment/Payment.Common/Payment.Common.csproj", "src/Services/Payment/Payment.Common/"]
COPY ["src/Services/Payment/Payment.Domain/Payment.Domain.csproj", "src/Services/Payment/Payment.Domain/"]
COPY ["src/Services/Payment/Payment.Persistence.Database/Payment.Persistence.Database.csproj", "src/Services/Payment/Payment.Persistence.Database/"]
COPY ["src/Services/Payment/Payment.Service.EventHandlers/Payment.Service.EventHandlers.csproj", "src/Services/Payment/Payment.Service.EventHandlers/"]
COPY ["src/Services/Payment/Payment.Service.Queries/Payment.Service.Queries.csproj", "src/Services/Payment/Payment.Service.Queries/"]
COPY ["src/Services/Payment/Payment.Service.Proxies/Payment.Service.Proxies.csproj", "src/Services/Payment/Payment.Service.Proxies/"]
COPY ["src/Services/Payment/Payment.Service.Gateways/Payment.Service.Gateways.csproj", "src/Services/Payment/Payment.Service.Gateways/"]

# Restore dependencies
RUN dotnet restore "src/Services/Payment/Payment.Api/Payment.Api.csproj"

# Copy all source code
COPY ["src/Common/", "src/Common/"]
COPY ["src/Services/Service.Common.Authentication/", "src/Services/Service.Common.Authentication/"]
COPY ["src/Services/Service.Common.Collection/", "src/Services/Service.Common.Collection/"]
COPY ["src/Services/Service.Common.Mapping/", "src/Services/Service.Common.Mapping/"]
COPY ["src/Services/Service.Common.Paging/", "src/Services/Service.Common.Paging/"]
COPY ["src/Services/Payment/", "src/Services/Payment/"]

# Build
RUN dotnet build "src/Services/Payment/Payment.Api/Payment.Api.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "src/Services/Payment/Payment.Api/Payment.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app

RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=publish /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/hc || exit 1

ENTRYPOINT ["dotnet", "Payment.Api.dll"]
