# Dokploy Deployment Guide

This guide explains how to deploy the E-Commerce Microservices Architecture to a VPS using Dokploy (self-hosted).

## Architecture Overview

```
                     ┌─────────────────────────────────────────────────────┐
                     │                    Hostinger VPS                     │
                     │              (KVM 2: 2vCPU, 8GB RAM)                 │
                     │                                                       │
                     │  ┌─────────────────────────────────────────────────┐ │
                     │  │                  Dokploy                         │ │
                     │  │  ┌─────────────────────────────────────────────┐│ │
                     │  │  │              Traefik (Reverse Proxy)        ││ │
                     │  │  │           SSL/TLS + Load Balancing          ││ │
                     │  │  └─────────────────────────────────────────────┘│ │
                     │  │                       │                          │ │
                     │  │  ┌────────────────────┼───────────────────────┐ │ │
                     │  │  │                    │                       │ │ │
                     │  │  ▼                    ▼                       ▼ │ │
                     │  │ ┌────────┐ ┌─────────┐ ┌─────────┐ ┌──────────┐│ │
                     │  │ │Gateway │ │Identity │ │Catalog  │ │ Customer ││ │
                     │  │ │:45000  │ │ :45001  │ │ :45002  │ │  :45003  ││ │
                     │  │ └────────┘ └─────────┘ └─────────┘ └──────────┘│ │
                     │  │ ┌────────┐ ┌─────────┐ ┌─────────┐ ┌──────────┐│ │
                     │  │ │ Order  │ │  Cart   │ │Payment  │ │Notificat.││ │
                     │  │ │:45004  │ │ :45005  │ │ :45006  │ │  :45007  ││ │
                     │  │ └────────┘ └─────────┘ └─────────┘ └──────────┘│ │
                     │  │                                                 │ │
                     │  │  ┌─────────────────────────────────────────────┐│ │
                     │  │  │              Infrastructure                 ││ │
                     │  │  │  ┌────────────┐ ┌────────┐ ┌─────────────┐  ││ │
                     │  │  │  │ PostgreSQL │ │ Redis  │ │  RabbitMQ   │  ││ │
                     │  │  │  │   :5432    │ │ :6379  │ │    :5672    │  ││ │
                     │  │  │  └────────────┘ └────────┘ └─────────────┘  ││ │
                     │  │  └─────────────────────────────────────────────┘│ │
                     │  └─────────────────────────────────────────────────┘ │
                     └─────────────────────────────────────────────────────┘
```

## Prerequisites

- Hostinger VPS KVM 2 ($7.49/month): 2 vCPU, 8GB RAM, 100GB NVMe
- GitHub repository (public or private with access token)
- Domain or subdomain (Hostinger provides free `.cloud` subdomain)

## Step 1: VPS Initial Setup

### 1.1 Connect to VPS

```bash
ssh root@your-vps-ip
```

### 1.2 Update System

```bash
apt update && apt upgrade -y
```

### 1.3 Install Docker

```bash
curl -fsSL https://get.docker.com | sh
systemctl enable docker
systemctl start docker
```

## Step 2: Install Dokploy

```bash
curl -sSL https://dokploy.com/install.sh | sh
```

This will:
- Install Dokploy
- Set up Traefik as reverse proxy
- Configure automatic SSL with Let's Encrypt
- Create admin interface at `http://your-vps-ip:3000`

### 2.1 Access Dokploy Dashboard

1. Open `http://your-vps-ip:3000` in browser
2. Create admin account
3. Configure domain settings

## Step 3: Configure Infrastructure Services

### 3.1 Create PostgreSQL Database

In Dokploy dashboard:

1. Go to **Services** > **New Service** > **Database** > **PostgreSQL**
2. Configure:
   - Name: `ecommerce-db`
   - Version: `16`
   - Database: `ecommerce`
   - Username: `postgres`
   - Password: `<generate-secure-password>`
3. Deploy

### 3.2 Create Redis Cache

1. Go to **Services** > **New Service** > **Database** > **Redis**
2. Configure:
   - Name: `ecommerce-redis`
   - Version: `7`
   - Password: `<generate-secure-password>`
3. Deploy

### 3.3 Create RabbitMQ Message Broker

1. Go to **Services** > **New Service** > **Docker Image**
2. Configure:
   - Name: `ecommerce-rabbitmq`
   - Image: `rabbitmq:3-management`
   - Ports: `5672`, `15672`
   - Environment variables:
     ```
     RABBITMQ_DEFAULT_USER=guest
     RABBITMQ_DEFAULT_PASS=<secure-password>
     ```
3. Deploy

## Step 4: Deploy Microservices

### 4.1 Connect GitHub Repository

1. Go to **Settings** > **Git Providers**
2. Add GitHub connection (OAuth or Personal Access Token)
3. Select repository: `your-username/ECommerceMicroserviceArchitecture`

### 4.2 Create Application Services

For each microservice, create a new application:

#### API Gateway (Port 45000)

1. **Services** > **New Service** > **Application**
2. Configure:
   - Name: `api-gateway`
   - Source: GitHub repository
   - Dockerfile Path: `src/Gateways/Api.Gateway.WebClient/Dockerfile`
   - Build Context: `.` (root)
   - Port: `45000`
3. Environment Variables:
   ```env
   ASPNETCORE_ENVIRONMENT=Production
   ASPNETCORE_URLS=http://+:45000
   Database__Provider=PostgreSQL
   ConnectionStrings__DefaultConnection=Host=ecommerce-db;Database=ecommerce;Username=postgres;Password=xxx
   Redis__ConnectionString=ecommerce-redis:6379,password=xxx
   RabbitMQ__Host=ecommerce-rabbitmq
   RabbitMQ__Username=guest
   RabbitMQ__Password=xxx
   ```

#### Identity Service (Port 45001)

1. **Services** > **New Service** > **Application**
2. Configure:
   - Name: `identity-api`
   - Dockerfile Path: `src/Services/Identity/Identity.Api/Dockerfile`
   - Port: `45001`
3. Environment Variables:
   ```env
   ASPNETCORE_ENVIRONMENT=Production
   ASPNETCORE_URLS=http://+:45001
   Database__Provider=PostgreSQL
   ConnectionStrings__DefaultConnection=Host=ecommerce-db;Database=ecommerce;Username=postgres;Password=xxx
   Redis__ConnectionString=ecommerce-redis:6379,password=xxx
   RabbitMQ__Host=ecommerce-rabbitmq
   Jwt__SecretKey=<your-256-bit-secret-key>
   Jwt__Issuer=https://yourdomain.cloud
   Jwt__Audience=ecommerce-api
   ```

#### Catalog Service (Port 45002)

```env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:45002
Database__Provider=PostgreSQL
ConnectionStrings__DefaultConnection=Host=ecommerce-db;Database=ecommerce;Username=postgres;Password=xxx
Redis__ConnectionString=ecommerce-redis:6379,password=xxx
RabbitMQ__Host=ecommerce-rabbitmq
```

#### Customer Service (Port 45003)

```env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:45003
Database__Provider=PostgreSQL
ConnectionStrings__DefaultConnection=Host=ecommerce-db;Database=ecommerce;Username=postgres;Password=xxx
Redis__ConnectionString=ecommerce-redis:6379,password=xxx
```

#### Order Service (Port 45004)

```env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:45004
Database__Provider=PostgreSQL
ConnectionStrings__DefaultConnection=Host=ecommerce-db;Database=ecommerce;Username=postgres;Password=xxx
Redis__ConnectionString=ecommerce-redis:6379,password=xxx
RabbitMQ__Host=ecommerce-rabbitmq
```

#### Cart Service (Port 45005)

```env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:45005
Database__Provider=PostgreSQL
ConnectionStrings__DefaultConnection=Host=ecommerce-db;Database=ecommerce;Username=postgres;Password=xxx
Redis__ConnectionString=ecommerce-redis:6379,password=xxx
RabbitMQ__Host=ecommerce-rabbitmq
```

#### Payment Service (Port 45006)

```env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:45006
Database__Provider=PostgreSQL
ConnectionStrings__DefaultConnection=Host=ecommerce-db;Database=ecommerce;Username=postgres;Password=xxx
Redis__ConnectionString=ecommerce-redis:6379,password=xxx
RabbitMQ__Host=ecommerce-rabbitmq
MercadoPago__AccessToken=<your-access-token>
```

#### Notification Service (Port 45007)

```env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:45007
Database__Provider=PostgreSQL
ConnectionStrings__DefaultConnection=Host=ecommerce-db;Database=ecommerce;Username=postgres;Password=xxx
RabbitMQ__Host=ecommerce-rabbitmq
Smtp__Host=smtp.gmail.com
Smtp__Port=587
Smtp__Username=your@email.com
Smtp__Password=app-password
```

## Step 5: Configure Domains and SSL

### 5.1 Configure Traefik Labels

For each service, add Traefik labels in Dokploy:

```yaml
# For API Gateway (main entry point)
traefik.enable: "true"
traefik.http.routers.gateway.rule: "Host(`api.yourdomain.cloud`)"
traefik.http.routers.gateway.entrypoints: "websecure"
traefik.http.routers.gateway.tls.certresolver: "letsencrypt"
```

### 5.2 DNS Configuration

In Hostinger DNS panel, add:

| Type | Name | Value |
|------|------|-------|
| A | api | your-vps-ip |
| A | @ | your-vps-ip |

## Step 6: Database Migrations

### 6.1 Run Initial Migrations

Connect to your VPS and run migrations using the Identity service container:

```bash
# Get container ID
docker ps | grep identity-api

# Run migrations
docker exec -it <container-id> dotnet ef database update
```

Or create a migration job in Dokploy that runs on deployment.

### 6.2 Seed Initial Data

```sql
-- Connect to PostgreSQL and run initial seed scripts
-- (This would be the PostgreSQL-adapted version of your existing seed scripts)
```

## Step 7: Health Checks and Monitoring

### 7.1 Configure Health Check Endpoints

Each service exposes:
- `/health` - Basic health check
- `/health/ready` - Readiness probe
- `/health/live` - Liveness probe

### 7.2 Configure Dokploy Health Checks

For each service, set:
- Health Check Path: `/health`
- Health Check Interval: `30s`
- Health Check Timeout: `10s`

## Step 8: CI/CD with GitHub Actions

Create `.github/workflows/deploy.yml`:

```yaml
name: Deploy to Dokploy

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - name: Trigger Dokploy Deployment
        run: |
          curl -X POST \
            -H "Authorization: Bearer ${{ secrets.DOKPLOY_API_TOKEN }}" \
            "https://your-dokploy-url/api/application/deploy/${{ secrets.APP_ID }}"
```

## Resource Allocation (8GB RAM VPS)

| Service | Memory Limit | CPU Limit |
|---------|-------------|-----------|
| PostgreSQL | 1.5 GB | 0.5 |
| Redis | 256 MB | 0.25 |
| RabbitMQ | 512 MB | 0.25 |
| API Gateway | 512 MB | 0.5 |
| Identity | 384 MB | 0.25 |
| Catalog | 512 MB | 0.5 |
| Customer | 384 MB | 0.25 |
| Order | 384 MB | 0.25 |
| Cart | 384 MB | 0.25 |
| Payment | 384 MB | 0.25 |
| Notification | 256 MB | 0.25 |
| **Total** | **~5.5 GB** | **~3.5 cores** |

Leaves ~2.5GB for OS and Docker overhead.

## Troubleshooting

### View Logs

```bash
# In Dokploy dashboard, click on service > Logs
# Or via Docker:
docker logs -f <container-name>
```

### Common Issues

1. **Database Connection Failed**
   - Verify PostgreSQL is running
   - Check connection string format (PostgreSQL uses `Host=` not `Server=`)
   - Ensure services are on same Docker network

2. **Redis Connection Issues**
   - Check Redis password is correct
   - Verify Redis container is healthy

3. **RabbitMQ Connection Failed**
   - Verify RabbitMQ is running
   - Check credentials
   - Ensure port 5672 is accessible internally

### Restart All Services

```bash
docker restart $(docker ps -q)
```

## Backup Strategy

### Database Backup

```bash
# Create backup
docker exec ecommerce-db pg_dump -U postgres ecommerce > backup_$(date +%Y%m%d).sql

# Restore backup
docker exec -i ecommerce-db psql -U postgres ecommerce < backup.sql
```

### Automated Backups

Use Dokploy's built-in backup feature or set up a cron job:

```bash
0 2 * * * docker exec ecommerce-db pg_dump -U postgres ecommerce | gzip > /backups/db_$(date +\%Y\%m\%d).sql.gz
```

## Cost Summary

| Item | Monthly Cost |
|------|-------------|
| Hostinger VPS KVM 2 | $7.49 |
| Domain (optional) | ~$1-2 |
| **Total** | ~$8-10/month |

With $200 budget: **20-25 months** of hosting.

## Security Checklist

- [ ] Change all default passwords
- [ ] Enable firewall (UFW)
- [ ] Configure SSL certificates
- [ ] Set up SSH key authentication
- [ ] Disable root login
- [ ] Configure rate limiting
- [ ] Set up monitoring alerts
- [ ] Regular security updates

## Next Steps

1. Set up monitoring (Prometheus + Grafana via Dokploy)
2. Configure log aggregation
3. Set up alerting
4. Implement blue-green deployments
5. Add horizontal scaling when needed
