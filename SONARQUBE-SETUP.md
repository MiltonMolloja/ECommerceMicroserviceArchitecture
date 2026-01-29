# SonarQube Setup Guide

This document describes how to set up and use SonarQube for code quality analysis in the E-Commerce Microservices Architecture project.

## Overview

SonarQube is configured to analyze:
- **C# Code** (.NET 9.0 projects)
- **HTML/CSS** (Email templates)
- **JavaScript** (Utility scripts)
- **Docker** files
- **YAML/JSON** configuration files

## Quick Start

### 1. Start SonarQube

```bash
docker-compose up -d sonarqube sonarqube-db
```

Wait for SonarQube to be healthy (about 1-2 minutes):
```bash
docker ps --filter "name=ecommerce_sonar"
```

### 2. Access SonarQube

- **URL**: http://localhost:9000
- **Default credentials**: admin / admin (you'll be prompted to change on first login)

### 3. Generate a Token

1. Go to **My Account** > **Security** > **Generate Tokens**
2. Create a token named `analysis-token`
3. Copy the token (you won't see it again!)

### 4. Run Analysis

**Option A: Using PowerShell Script**
```powershell
.\scripts\sonar-scan.ps1 -SonarToken "your-token-here"
```

**Option B: Manual Commands**
```bash
# Set your token
$env:SONAR_TOKEN = "your-token-here"

# Begin analysis
dotnet sonarscanner begin /k:"ECommerceMicroserviceArchitecture" /d:sonar.host.url="http://localhost:9000" /d:sonar.token="$env:SONAR_TOKEN"

# Build
dotnet build ECommerce.sln --configuration Release

# End analysis (uploads results)
dotnet sonarscanner end /d:sonar.token="$env:SONAR_TOKEN"
```

### 5. View Results

Open http://localhost:9000/dashboard?id=ECommerceMicroserviceArchitecture

## Current Analysis Results

| Metric | Value | Status |
|--------|-------|--------|
| **Lines of Code** | ~27,000 | - |
| **Bugs** | 16 | Needs attention |
| **Vulnerabilities** | 3 | Critical - secrets detected |
| **Code Smells** | 2,203 | Technical debt |
| **Security Hotspots** | 34 | Review needed |
| **Coverage** | 0% | No tests with coverage |
| **Duplications** | 18.1% | Above threshold |

### Key Issues Found

#### Vulnerabilities (BLOCKER)
1. **Hardcoded secrets** in configuration files
2. **PostgreSQL password** in migration script
3. **JWT secret** in utility script

#### Bugs (MAJOR)
1. **HTML accessibility** - Missing `<th>` headers in tables
2. **Unused object instantiation** in EF Core configurations
3. **CSS selector issues** in email templates

## Docker Configuration

SonarQube runs in Docker with PostgreSQL for persistence:

```yaml
# docker-compose.yml
services:
  sonarqube:
    image: sonarqube:community
    container_name: ecommerce_sonarqube
    ports:
      - "9000:9000"
    environment:
      - SONAR_JDBC_URL=jdbc:postgresql://sonarqube-db:5432/sonar
      - SONAR_JDBC_USERNAME=sonar
      - SONAR_JDBC_PASSWORD=sonar
    volumes:
      - sonarqube_data:/opt/sonarqube/data
      - sonarqube_extensions:/opt/sonarqube/extensions
      - sonarqube_logs:/opt/sonarqube/logs
    depends_on:
      sonarqube-db:
        condition: service_healthy

  sonarqube-db:
    image: postgres:15-alpine
    container_name: ecommerce_sonarqube_db
    environment:
      - POSTGRES_USER=sonar
      - POSTGRES_PASSWORD=sonar
      - POSTGRES_DB=sonar
    volumes:
      - sonarqube_postgresql:/var/lib/postgresql/data
```

## MCP Integration

SonarQube MCP is configured for Claude Code integration:

```bash
# Add MCP (already configured)
claude mcp add sonarqube -e SONARQUBE_URL=http://localhost:9000 -e SONARQUBE_TOKEN=your-token -- npx -y @godrix/mcp-sonarqube

# Verify
claude mcp list
```

**Important**: Set the `SONARQUBE_TOKEN` environment variable before using the MCP.

## CI/CD Integration

### GitHub Actions

A workflow is configured at `.github/workflows/sonarqube.yml`:

```yaml
name: SonarQube Analysis
on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  sonarqube:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'
      - name: Install SonarScanner
        run: dotnet tool install --global dotnet-sonarscanner
      - name: Build and Analyze
        env:
          SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}
          SONAR_HOST_URL: ${{ secrets.SONAR_HOST_URL }}
        run: |
          dotnet sonarscanner begin /k:"ECommerceMicroserviceArchitecture" /d:sonar.host.url="$SONAR_HOST_URL" /d:sonar.token="$SONAR_TOKEN"
          dotnet build ECommerce.sln --configuration Release
          dotnet sonarscanner end /d:sonar.token="$SONAR_TOKEN"
```

## Quality Gate

The default "Sonar way" quality gate is applied. Consider customizing for:
- Coverage threshold (e.g., 80%)
- Duplications threshold (e.g., 3%)
- New code requirements

## Exclusions

The following are excluded from analysis:
- `**/bin/**` - Build output
- `**/obj/**` - Build artifacts
- `**/Migrations/**` - EF Core migrations
- `**/coverage.opencover.xml` - Coverage reports

## Troubleshooting

### SonarQube won't start
```bash
# Check logs
docker logs ecommerce_sonarqube

# Common fix: increase vm.max_map_count (Linux)
sudo sysctl -w vm.max_map_count=262144
```

### Analysis fails
```bash
# Ensure SonarQube is healthy
curl http://localhost:9000/api/system/status

# Check token is valid
curl -H "Authorization: Bearer YOUR_TOKEN" http://localhost:9000/api/authentication/validate
```

### Token expired
Generate a new token in SonarQube UI and update your environment variable.

## Useful Commands

```bash
# Check SonarQube status
curl http://localhost:9000/api/system/status

# Get project metrics
curl -H "Authorization: Bearer TOKEN" "http://localhost:9000/api/measures/component?component=ECommerceMicroserviceArchitecture&metricKeys=bugs,vulnerabilities,code_smells"

# Get issues
curl -H "Authorization: Bearer TOKEN" "http://localhost:9000/api/issues/search?componentKeys=ECommerceMicroserviceArchitecture&types=BUG"

# Stop SonarQube
docker-compose stop sonarqube sonarqube-db
```

## References

- [SonarQube Documentation](https://docs.sonarqube.org/)
- [SonarScanner for .NET](https://docs.sonarqube.org/latest/analysis/scan/sonarscanner-for-msbuild/)
- [Quality Gates](https://docs.sonarqube.org/latest/user-guide/quality-gates/)
