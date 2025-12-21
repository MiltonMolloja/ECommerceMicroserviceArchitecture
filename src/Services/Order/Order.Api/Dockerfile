# ============================================
# Dockerfile for Order.Api
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

# Copy Order projects
COPY ["src/Services/Order/Order.Api/Order.Api.csproj", "src/Services/Order/Order.Api/"]
COPY ["src/Services/Order/Order.Common/Order.Common.csproj", "src/Services/Order/Order.Common/"]
COPY ["src/Services/Order/Order.Domain/Order.Domain.csproj", "src/Services/Order/Order.Domain/"]
COPY ["src/Services/Order/Order.Persistence.Database/Order.Persistence.Database.csproj", "src/Services/Order/Order.Persistence.Database/"]
COPY ["src/Services/Order/Order.Service.EventHandlers/Order.Service.EventHandlers.csproj", "src/Services/Order/Order.Service.EventHandlers/"]
COPY ["src/Services/Order/Order.Service.Queries/Order.Service.Queries.csproj", "src/Services/Order/Order.Service.Queries/"]
COPY ["src/Services/Order/Order.Service.Proxies/Order.Service.Proxies.csproj", "src/Services/Order/Order.Service.Proxies/"]

# Restore dependencies
RUN dotnet restore "src/Services/Order/Order.Api/Order.Api.csproj"

# Copy all source code
COPY ["src/Common/", "src/Common/"]
COPY ["src/Services/Service.Common.Authentication/", "src/Services/Service.Common.Authentication/"]
COPY ["src/Services/Service.Common.Collection/", "src/Services/Service.Common.Collection/"]
COPY ["src/Services/Service.Common.Mapping/", "src/Services/Service.Common.Mapping/"]
COPY ["src/Services/Service.Common.Paging/", "src/Services/Service.Common.Paging/"]
COPY ["src/Services/Order/", "src/Services/Order/"]

# Build
RUN dotnet build "src/Services/Order/Order.Api/Order.Api.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "src/Services/Order/Order.Api/Order.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

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

ENTRYPOINT ["dotnet", "Order.Api.dll"]
