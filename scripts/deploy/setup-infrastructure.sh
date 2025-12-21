#!/bin/bash
# ============================================
# Script de Configuración de Infraestructura
# PostgreSQL + Redis + RabbitMQ + Microservicios
# ============================================

set -e

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Generar contraseñas seguras
POSTGRES_PASSWORD="$(openssl rand -base64 32 | tr -dc 'a-zA-Z0-9' | head -c 32)"
REDIS_PASSWORD="$(openssl rand -base64 32 | tr -dc 'a-zA-Z0-9' | head -c 32)"
RABBITMQ_PASSWORD="$(openssl rand -base64 32 | tr -dc 'a-zA-Z0-9' | head -c 32)"
JWT_SECRET="$(openssl rand -base64 64 | tr -dc 'a-zA-Z0-9' | head -c 64)"

# Configuración de dominio (cambiar según necesites)
DOMAIN="${DOMAIN:-tudominio.com}"
API_SUBDOMAIN="api"
AUTH_SUBDOMAIN="auth"

CREDENTIALS_FILE="/root/.ecommerce-credentials"

echo -e "${BLUE}============================================${NC}"
echo -e "${BLUE}   Configuración de Infraestructura        ${NC}"
echo -e "${BLUE}============================================${NC}"
echo ""

log() { echo -e "${GREEN}[✓]${NC} $1"; }
warn() { echo -e "${YELLOW}[!]${NC} $1"; }
error() { echo -e "${RED}[✗]${NC} $1"; exit 1; }

# ============================================
# 1. Crear Red Docker
# ============================================
log "Creando red Docker ecommerce-network..."
docker network create ecommerce-network 2>/dev/null || warn "Red ya existe"

# ============================================
# 2. Desplegar PostgreSQL 16
# ============================================
log "Desplegando PostgreSQL 16..."
docker stop ecommerce-postgres 2>/dev/null || true
docker rm ecommerce-postgres 2>/dev/null || true

docker run -d \
    --name ecommerce-postgres \
    --network ecommerce-network \
    --restart unless-stopped \
    -e POSTGRES_USER=postgres \
    -e POSTGRES_PASSWORD="${POSTGRES_PASSWORD}" \
    -e POSTGRES_DB=ecommerce \
    -v postgres_data:/var/lib/postgresql/data \
    -p 5432:5432 \
    postgres:16-alpine

# ============================================
# 3. Desplegar Redis 7
# ============================================
log "Desplegando Redis 7..."
docker stop ecommerce-redis 2>/dev/null || true
docker rm ecommerce-redis 2>/dev/null || true

docker run -d \
    --name ecommerce-redis \
    --network ecommerce-network \
    --restart unless-stopped \
    -v redis_data:/data \
    -p 6379:6379 \
    redis:7-alpine redis-server --requirepass "${REDIS_PASSWORD}"

# ============================================
# 4. Desplegar RabbitMQ 3
# ============================================
log "Desplegando RabbitMQ 3..."
docker stop ecommerce-rabbitmq 2>/dev/null || true
docker rm ecommerce-rabbitmq 2>/dev/null || true

docker run -d \
    --name ecommerce-rabbitmq \
    --network ecommerce-network \
    --restart unless-stopped \
    -e RABBITMQ_DEFAULT_USER=admin \
    -e RABBITMQ_DEFAULT_PASS="${RABBITMQ_PASSWORD}" \
    -v rabbitmq_data:/var/lib/rabbitmq \
    -p 5672:5672 \
    -p 15672:15672 \
    rabbitmq:3-management-alpine

# Esperar a que los servicios inicien
log "Esperando a que los servicios inicien..."
sleep 10

# ============================================
# 5. Guardar Credenciales
# ============================================
log "Guardando credenciales..."
cat > "${CREDENTIALS_FILE}" << EOF
# ============================================
# E-Commerce - Credenciales de Infraestructura
# Generado: $(date)
# ============================================

# PostgreSQL
POSTGRES_HOST=ecommerce-postgres
POSTGRES_PORT=5432
POSTGRES_USER=postgres
POSTGRES_PASSWORD=${POSTGRES_PASSWORD}
POSTGRES_DB=ecommerce
CONNECTION_STRING=Host=ecommerce-postgres;Database=ecommerce;Username=postgres;Password=${POSTGRES_PASSWORD}

# Redis
REDIS_HOST=ecommerce-redis
REDIS_PORT=6379
REDIS_PASSWORD=${REDIS_PASSWORD}
REDIS_CONNECTION=ecommerce-redis:6379,password=${REDIS_PASSWORD}

# RabbitMQ
RABBITMQ_HOST=ecommerce-rabbitmq
RABBITMQ_PORT=5672
RABBITMQ_USER=admin
RABBITMQ_PASSWORD=${RABBITMQ_PASSWORD}
RABBITMQ_MANAGEMENT=http://$(curl -s ifconfig.me):15672

# JWT
JWT_SECRET=${JWT_SECRET}
EOF

chmod 600 "${CREDENTIALS_FILE}"

# ============================================
# 6. Crear directorio y docker-compose.yml
# ============================================
log "Creando docker-compose.yml..."
mkdir -p /opt/ecommerce

cat > /opt/ecommerce/docker-compose.yml << EOF
version: '3.8'

services:
  # ============================================
  # Frontend E-Commerce (Angular)
  # ============================================
  frontend:
    build:
      context: https://github.com/MiltonMolloja/ECommerceFrontend.git
      dockerfile: Dockerfile
    container_name: ecommerce-frontend
    restart: unless-stopped
    ports:
      - "4200:80"
    environment:
      - API_GATEWAY_URL=http://api-gateway:45000
      - IDENTITY_URL=http://identity-api:45001
      - LOGIN_SERVICE_URL=http://auth-frontend:80
      - PRODUCTION=true
    networks:
      - ecommerce-network
    depends_on:
      - api-gateway

  # ============================================
  # Frontend Auth (Angular)
  # ============================================
  auth-frontend:
    build:
      context: https://github.com/MiltonMolloja/ClientsAuthenticationFrontend.git
      dockerfile: Dockerfile
    container_name: ecommerce-auth
    restart: unless-stopped
    ports:
      - "4400:80"
    environment:
      - API_URL=http://api-gateway:45000
      - IDENTITY_SERVER_URL=http://identity-api:45001
      - ECOMMERCE_URL=http://frontend:80
      - PRODUCTION=true
    networks:
      - ecommerce-network
    depends_on:
      - identity-api

  # ============================================
  # API Gateway
  # ============================================
  api-gateway:
    build:
      context: https://github.com/MiltonMolloja/ECommerceMicroserviceArchitecture.git
      dockerfile: src/Gateways/Api.Gateway.WebClient/Dockerfile
    container_name: ecommerce-gateway
    restart: unless-stopped
    ports:
      - "45000:45000"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:45000
      - Database__Provider=PostgreSQL
      - ConnectionStrings__DefaultConnection=Host=ecommerce-postgres;Database=ecommerce;Username=postgres;Password=${POSTGRES_PASSWORD}
      - Redis__ConnectionString=ecommerce-redis:6379,password=${REDIS_PASSWORD}
      - RabbitMQ__Host=ecommerce-rabbitmq
      - RabbitMQ__Username=admin
      - RabbitMQ__Password=${RABBITMQ_PASSWORD}
      - Jwt__SecretKey=${JWT_SECRET}
    networks:
      - ecommerce-network
    depends_on:
      - identity-api
      - catalog-api
      - customer-api
      - order-api
      - cart-api
      - payment-api

  # ============================================
  # Identity Service
  # ============================================
  identity-api:
    build:
      context: https://github.com/MiltonMolloja/ECommerceMicroserviceArchitecture.git
      dockerfile: src/Services/Identity/Identity.Api/Dockerfile
    container_name: ecommerce-identity
    restart: unless-stopped
    ports:
      - "45001:45001"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:45001
      - Database__Provider=PostgreSQL
      - ConnectionStrings__DefaultConnection=Host=ecommerce-postgres;Database=ecommerce;Username=postgres;Password=${POSTGRES_PASSWORD}
      - Redis__ConnectionString=ecommerce-redis:6379,password=${REDIS_PASSWORD}
      - RabbitMQ__Host=ecommerce-rabbitmq
      - RabbitMQ__Username=admin
      - RabbitMQ__Password=${RABBITMQ_PASSWORD}
      - Jwt__SecretKey=${JWT_SECRET}
      - Jwt__Issuer=https://${API_SUBDOMAIN}.${DOMAIN}
      - Jwt__Audience=ecommerce-api
      - Jwt__ExpirationMinutes=60
      - Jwt__RefreshTokenExpirationDays=7
    networks:
      - ecommerce-network

  # ============================================
  # Catalog Service
  # ============================================
  catalog-api:
    build:
      context: https://github.com/MiltonMolloja/ECommerceMicroserviceArchitecture.git
      dockerfile: src/Services/Catalog/Catalog.Api/Dockerfile
    container_name: ecommerce-catalog
    restart: unless-stopped
    ports:
      - "45002:45002"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:45002
      - Database__Provider=PostgreSQL
      - ConnectionStrings__DefaultConnection=Host=ecommerce-postgres;Database=ecommerce;Username=postgres;Password=${POSTGRES_PASSWORD}
      - Redis__ConnectionString=ecommerce-redis:6379,password=${REDIS_PASSWORD}
      - RabbitMQ__Host=ecommerce-rabbitmq
      - RabbitMQ__Username=admin
      - RabbitMQ__Password=${RABBITMQ_PASSWORD}
    networks:
      - ecommerce-network

  # ============================================
  # Customer Service
  # ============================================
  customer-api:
    build:
      context: https://github.com/MiltonMolloja/ECommerceMicroserviceArchitecture.git
      dockerfile: src/Services/Customer/Customer.Api/Dockerfile
    container_name: ecommerce-customer
    restart: unless-stopped
    ports:
      - "45003:45003"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:45003
      - Database__Provider=PostgreSQL
      - ConnectionStrings__DefaultConnection=Host=ecommerce-postgres;Database=ecommerce;Username=postgres;Password=${POSTGRES_PASSWORD}
      - Redis__ConnectionString=ecommerce-redis:6379,password=${REDIS_PASSWORD}
    networks:
      - ecommerce-network

  # ============================================
  # Order Service
  # ============================================
  order-api:
    build:
      context: https://github.com/MiltonMolloja/ECommerceMicroserviceArchitecture.git
      dockerfile: src/Services/Order/Order.Api/Dockerfile
    container_name: ecommerce-order
    restart: unless-stopped
    ports:
      - "45004:45004"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:45004
      - Database__Provider=PostgreSQL
      - ConnectionStrings__DefaultConnection=Host=ecommerce-postgres;Database=ecommerce;Username=postgres;Password=${POSTGRES_PASSWORD}
      - Redis__ConnectionString=ecommerce-redis:6379,password=${REDIS_PASSWORD}
      - RabbitMQ__Host=ecommerce-rabbitmq
      - RabbitMQ__Username=admin
      - RabbitMQ__Password=${RABBITMQ_PASSWORD}
    networks:
      - ecommerce-network

  # ============================================
  # Cart Service
  # ============================================
  cart-api:
    build:
      context: https://github.com/MiltonMolloja/ECommerceMicroserviceArchitecture.git
      dockerfile: src/Services/Cart/Cart.Api/Dockerfile
    container_name: ecommerce-cart
    restart: unless-stopped
    ports:
      - "45005:45005"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:45005
      - Database__Provider=PostgreSQL
      - ConnectionStrings__DefaultConnection=Host=ecommerce-postgres;Database=ecommerce;Username=postgres;Password=${POSTGRES_PASSWORD}
      - Redis__ConnectionString=ecommerce-redis:6379,password=${REDIS_PASSWORD}
      - RabbitMQ__Host=ecommerce-rabbitmq
      - RabbitMQ__Username=admin
      - RabbitMQ__Password=${RABBITMQ_PASSWORD}
    networks:
      - ecommerce-network

  # ============================================
  # Payment Service
  # ============================================
  payment-api:
    build:
      context: https://github.com/MiltonMolloja/ECommerceMicroserviceArchitecture.git
      dockerfile: src/Services/Payment/Payment.Api/Dockerfile
    container_name: ecommerce-payment
    restart: unless-stopped
    ports:
      - "45006:45006"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:45006
      - Database__Provider=PostgreSQL
      - ConnectionStrings__DefaultConnection=Host=ecommerce-postgres;Database=ecommerce;Username=postgres;Password=${POSTGRES_PASSWORD}
      - Redis__ConnectionString=ecommerce-redis:6379,password=${REDIS_PASSWORD}
      - RabbitMQ__Host=ecommerce-rabbitmq
      - RabbitMQ__Username=admin
      - RabbitMQ__Password=${RABBITMQ_PASSWORD}
    networks:
      - ecommerce-network

  # ============================================
  # Notification Service
  # ============================================
  notification-api:
    build:
      context: https://github.com/MiltonMolloja/ECommerceMicroserviceArchitecture.git
      dockerfile: src/Services/Notification/Notification.Api/Dockerfile
    container_name: ecommerce-notification
    restart: unless-stopped
    ports:
      - "45007:45007"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:45007
      - Database__Provider=PostgreSQL
      - ConnectionStrings__DefaultConnection=Host=ecommerce-postgres;Database=ecommerce;Username=postgres;Password=${POSTGRES_PASSWORD}
      - RabbitMQ__Host=ecommerce-rabbitmq
      - RabbitMQ__Username=admin
      - RabbitMQ__Password=${RABBITMQ_PASSWORD}
    networks:
      - ecommerce-network

networks:
  ecommerce-network:
    external: true
EOF

# ============================================
# 7. Crear script de Health Check
# ============================================
log "Creando script de health check..."
cat > /opt/ecommerce/health-check.sh << 'HEALTHEOF'
#!/bin/bash
echo "=========================================="
echo "  E-Commerce Health Check"
echo "  $(date)"
echo "=========================================="
echo ""

echo "📦 Contenedores:"
docker ps --format "table {{.Names}}\t{{.Status}}" | grep ecommerce
echo ""

echo "🔍 Servicios de Infraestructura:"

# PostgreSQL
if docker exec ecommerce-postgres pg_isready -U postgres > /dev/null 2>&1; then
    echo "  ✅ PostgreSQL: OK"
else
    echo "  ❌ PostgreSQL: ERROR"
fi

# Redis
if docker exec ecommerce-redis redis-cli ping 2>/dev/null | grep -q PONG; then
    echo "  ✅ Redis: OK"
else
    echo "  ❌ Redis: ERROR"
fi

# RabbitMQ
if docker exec ecommerce-rabbitmq rabbitmq-diagnostics -q ping > /dev/null 2>&1; then
    echo "  ✅ RabbitMQ: OK"
else
    echo "  ❌ RabbitMQ: ERROR"
fi

echo ""
echo "💾 Disco: $(df -h / | tail -1 | awk '{print $5 " usado de " $2}')"
echo "🧠 RAM: $(free -h | grep Mem | awk '{print $3 " / " $2}')"
HEALTHEOF

chmod +x /opt/ecommerce/health-check.sh

# ============================================
# 8. Crear script de Backup
# ============================================
log "Creando script de backup..."
cat > /opt/ecommerce/backup.sh << 'BACKUPEOF'
#!/bin/bash
BACKUP_DIR="/opt/ecommerce/backups"
DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="${BACKUP_DIR}/ecommerce_${DATE}.sql.gz"

mkdir -p "${BACKUP_DIR}"

echo "[$(date)] Iniciando backup..."
docker exec ecommerce-postgres pg_dump -U postgres ecommerce | gzip > "${BACKUP_FILE}"

# Mantener solo últimos 7 backups
ls -t "${BACKUP_DIR}"/*.sql.gz 2>/dev/null | tail -n +8 | xargs -r rm

echo "[$(date)] Backup completado: ${BACKUP_FILE}"
ls -lh "${BACKUP_DIR}"
BACKUPEOF

chmod +x /opt/ecommerce/backup.sh

# Agregar cron para backup diario a las 2am
(crontab -l 2>/dev/null | grep -v "ecommerce/backup"; echo "0 2 * * * /opt/ecommerce/backup.sh >> /var/log/ecommerce-backup.log 2>&1") | crontab -

# ============================================
# 9. Crear script para iniciar servicios
# ============================================
log "Creando script de inicio..."
cat > /opt/ecommerce/start-services.sh << 'STARTEOF'
#!/bin/bash
echo "Iniciando servicios de infraestructura..."
docker start ecommerce-postgres ecommerce-redis ecommerce-rabbitmq

echo "Esperando 10 segundos..."
sleep 10

echo "Iniciando microservicios..."
cd /opt/ecommerce && docker compose up -d --build

echo "Servicios iniciados. Ejecuta health-check.sh para verificar."
STARTEOF

chmod +x /opt/ecommerce/start-services.sh

# ============================================
# 10. Crear script para detener servicios
# ============================================
cat > /opt/ecommerce/stop-services.sh << 'STOPEOF'
#!/bin/bash
echo "Deteniendo microservicios..."
cd /opt/ecommerce && docker compose down

echo "¿Detener infraestructura (PostgreSQL, Redis, RabbitMQ)? [y/N]"
read -r response
if [[ "$response" =~ ^[Yy]$ ]]; then
    docker stop ecommerce-postgres ecommerce-redis ecommerce-rabbitmq
    echo "Infraestructura detenida."
fi
STOPEOF

chmod +x /opt/ecommerce/stop-services.sh

# ============================================
# 11. Crear script para ver logs
# ============================================
cat > /opt/ecommerce/logs.sh << 'LOGSEOF'
#!/bin/bash
SERVICE=${1:-all}

if [ "$SERVICE" == "all" ]; then
    echo "Uso: ./logs.sh [servicio]"
    echo "Servicios: postgres, redis, rabbitmq, gateway, identity, catalog, customer, order, cart, payment, notification, frontend, auth"
    echo ""
    echo "Mostrando logs de todos los contenedores ecommerce..."
    docker logs --tail 50 ecommerce-postgres
else
    docker logs --tail 100 -f "ecommerce-${SERVICE}"
fi
LOGSEOF

chmod +x /opt/ecommerce/logs.sh

# ============================================
# Verificar servicios
# ============================================
log "Verificando servicios..."
sleep 5

echo ""
/opt/ecommerce/health-check.sh

# ============================================
# Resumen Final
# ============================================
VPS_IP=$(curl -s ifconfig.me 2>/dev/null || echo "TU_IP")

echo ""
echo -e "${GREEN}============================================${NC}"
echo -e "${GREEN}   ¡Infraestructura Lista!                 ${NC}"
echo -e "${GREEN}============================================${NC}"
echo ""
echo -e "${BLUE}🔑 Credenciales guardadas en:${NC}"
echo "   ${CREDENTIALS_FILE}"
echo ""
echo -e "${BLUE}📁 Scripts disponibles en /opt/ecommerce/:${NC}"
echo "   ./health-check.sh    - Verificar estado"
echo "   ./backup.sh          - Backup manual"
echo "   ./start-services.sh  - Iniciar todo"
echo "   ./stop-services.sh   - Detener todo"
echo "   ./logs.sh [servicio] - Ver logs"
echo ""
echo -e "${BLUE}🌐 URLs de Acceso:${NC}"
echo "   RabbitMQ Management: http://${VPS_IP}:15672"
echo "   (user: admin, pass: ver credenciales)"
echo ""
echo -e "${YELLOW}▶ Para desplegar los microservicios:${NC}"
echo "   cd /opt/ecommerce && docker compose up -d --build"
echo ""
echo -e "${YELLOW}▶ Para ver las credenciales:${NC}"
echo "   cat ${CREDENTIALS_FILE}"
