# ============================================
# Dockerfile for Notification.Api
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

# Copy Notification projects
COPY ["src/Services/Notification/Notification.Api/Notification.Api.csproj", "src/Services/Notification/Notification.Api/"]
COPY ["src/Services/Notification/Notification.Common/Notification.Common.csproj", "src/Services/Notification/Notification.Common/"]
COPY ["src/Services/Notification/Notification.Domain/Notification.Domain.csproj", "src/Services/Notification/Notification.Domain/"]
COPY ["src/Services/Notification/Notification.Persistence.Database/Notification.Persistence.Database.csproj", "src/Services/Notification/Notification.Persistence.Database/"]
COPY ["src/Services/Notification/Notification.Service.EventHandlers/Notification.Service.EventHandlers.csproj", "src/Services/Notification/Notification.Service.EventHandlers/"]
COPY ["src/Services/Notification/Notification.Service.Queries/Notification.Service.Queries.csproj", "src/Services/Notification/Notification.Service.Queries/"]

# Restore dependencies
RUN dotnet restore "src/Services/Notification/Notification.Api/Notification.Api.csproj"

# Copy all source code
COPY ["src/Common/", "src/Common/"]
COPY ["src/Services/Service.Common.Authentication/", "src/Services/Service.Common.Authentication/"]
COPY ["src/Services/Service.Common.Collection/", "src/Services/Service.Common.Collection/"]
COPY ["src/Services/Service.Common.Mapping/", "src/Services/Service.Common.Mapping/"]
COPY ["src/Services/Service.Common.Paging/", "src/Services/Service.Common.Paging/"]
COPY ["src/Services/Notification/", "src/Services/Notification/"]

# Build
RUN dotnet build "src/Services/Notification/Notification.Api/Notification.Api.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "src/Services/Notification/Notification.Api/Notification.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

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

ENTRYPOINT ["dotnet", "Notification.Api.dll"]
