# ============================================
# Dockerfile for Api.Gateway.WebClient
# ECommerce Microservices Architecture
# ============================================

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copy solution and project files first (better layer caching)
COPY ["ECommerce.sln", "./"]

# Copy Common projects
COPY ["src/Common/Common.ApiKey/Common.ApiKey.csproj", "src/Common/Common.ApiKey/"]
COPY ["src/Common/Common.Caching/Common.Caching.csproj", "src/Common/Common.Caching/"]
COPY ["src/Common/Common.CorrelationId/Common.CorrelationId.csproj", "src/Common/Common.CorrelationId/"]
COPY ["src/Common/Common.Logging/Common.Logging.csproj", "src/Common/Common.Logging/"]
COPY ["src/Common/Common.Messaging/Common.Messaging.csproj", "src/Common/Common.Messaging/"]
COPY ["src/Common/Common.RateLimiting/Common.RateLimiting.csproj", "src/Common/Common.RateLimiting/"]
COPY ["src/Common/Common.Validation/Common.Validation.csproj", "src/Common/Common.Validation/"]

# Copy Gateway projects
COPY ["src/Gateways/Api.Gateway.Models/Api.Gateway.Models.csproj", "src/Gateways/Api.Gateway.Models/"]
COPY ["src/Gateways/Api.Gateway.Proxies/Api.Gateway.Proxies.csproj", "src/Gateways/Api.Gateway.Proxies/"]
COPY ["src/Gateways/Api.Gateway.WebClient/Api.Gateway.WebClient.csproj", "src/Gateways/Api.Gateway.WebClient/"]
COPY ["src/Gateways/Api.Gateway.WebClient.Proxy/Api.Gateway.WebClient.Proxy.csproj", "src/Gateways/Api.Gateway.WebClient.Proxy/"]

# Copy Service Common projects
COPY ["src/Services/Service.Common.Authentication/Service.Common.Authentication.csproj", "src/Services/Service.Common.Authentication/"]
COPY ["src/Services/Service.Common.Collection/Service.Common.Collection.csproj", "src/Services/Service.Common.Collection/"]
COPY ["src/Services/Service.Common.Mapping/Service.Common.Mapping.csproj", "src/Services/Service.Common.Mapping/"]
COPY ["src/Services/Service.Common.Paging/Service.Common.Paging.csproj", "src/Services/Service.Common.Paging/"]

# Restore dependencies
RUN dotnet restore "src/Gateways/Api.Gateway.WebClient/Api.Gateway.WebClient.csproj"

# Copy all source code
COPY ["src/Common/", "src/Common/"]
COPY ["src/Gateways/", "src/Gateways/"]
COPY ["src/Services/Service.Common.Authentication/", "src/Services/Service.Common.Authentication/"]
COPY ["src/Services/Service.Common.Collection/", "src/Services/Service.Common.Collection/"]
COPY ["src/Services/Service.Common.Mapping/", "src/Services/Service.Common.Mapping/"]
COPY ["src/Services/Service.Common.Paging/", "src/Services/Service.Common.Paging/"]

# Build
RUN dotnet build "src/Gateways/Api.Gateway.WebClient/Api.Gateway.WebClient.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "src/Gateways/Api.Gateway.WebClient/Api.Gateway.WebClient.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage (minimal image)
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app

# Install cultures for globalization support
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Expose port
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Copy published app
COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/hc || exit 1

ENTRYPOINT ["dotnet", "Api.Gateway.WebClient.dll"]
