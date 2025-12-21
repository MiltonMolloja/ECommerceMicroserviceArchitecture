#!/bin/bash
# ============================================
# Script de Instalación Automatizada
# E-Commerce Microservices en VPS con Dokploy
# ============================================

set -e

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuración - MODIFICAR ESTOS VALORES
DOMAIN="ecommerce.example.com"           # Tu dominio principal
API_SUBDOMAIN="api"                       # api.ecommerce.example.com
AUTH_SUBDOMAIN="auth"                     # auth.ecommerce.example.com
POSTGRES_PASSWORD="$(openssl rand -base64 32 | tr -dc 'a-zA-Z0-9' | head -c 32)"
REDIS_PASSWORD="$(openssl rand -base64 32 | tr -dc 'a-zA-Z0-9' | head -c 32)"
RABBITMQ_PASSWORD="$(openssl rand -base64 32 | tr -dc 'a-zA-Z0-9' | head -c 32)"
JWT_SECRET="$(openssl rand -base64 64 | tr -dc 'a-zA-Z0-9' | head -c 64)"

# Guardar credenciales
CREDENTIALS_FILE="/root/.ecommerce-credentials"

echo -e "${BLUE}============================================${NC}"
echo -e "${BLUE}   E-Commerce Microservices Installer      ${NC}"
echo -e "${BLUE}============================================${NC}"
echo ""

# Función para logging
log() {
    echo -e "${GREEN}[$(date +'%Y-%m-%d %H:%M:%S')]${NC} $1"
}

warn() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

error() {
    echo -e "${RED}[ERROR]${NC} $1"
    exit 1
}

# ============================================
# 1. Actualizar Sistema
# ============================================
log "Actualizando sistema operativo..."
apt update && apt upgrade -y

# ============================================
# 2. Instalar Dependencias
# ============================================
log "Instalando dependencias..."
apt install -y curl wget git htop nano ufw

# ============================================
# 3. Configurar Firewall
# ============================================
log "Configurando firewall..."
ufw allow 22/tcp      # SSH
ufw allow 80/tcp      # HTTP
ufw allow 443/tcp     # HTTPS
ufw allow 3000/tcp    # Dokploy Dashboard
ufw --force enable

# ============================================
# 4. Instalar Docker
# ============================================
if ! command -v docker &> /dev/null; then
    log "Instalando Docker..."
    curl -fsSL https://get.docker.com | sh
    systemctl enable docker
    systemctl start docker
else
    log "Docker ya está instalado"
fi

# ============================================
# 5. Instalar Dokploy
# ============================================
log "Instalando Dokploy..."
curl -sSL https://dokploy.com/install.sh | sh

# Esperar a que Dokploy inicie
log "Esperando a que Dokploy inicie..."
sleep 30

# ============================================
# 6. Crear Red Docker
# ============================================
log "Creando red Docker para microservicios..."
docker network create ecommerce-network 2>/dev/null || true

# ============================================
# 7. Desplegar PostgreSQL
# ============================================
log "Desplegando PostgreSQL..."
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
# 8. Desplegar Redis
# ============================================
log "Desplegando Redis..."
docker run -d \
    --name ecommerce-redis \
    --network ecommerce-network \
    --restart unless-stopped \
    -v redis_data:/data \
    -p 6379:6379 \
    redis:7-alpine redis-server --requirepass "${REDIS_PASSWORD}"

# ============================================
# 9. Desplegar RabbitMQ
# ============================================
log "Desplegando RabbitMQ..."
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

# ============================================
# 10. Guardar Credenciales
# ============================================
log "Guardando credenciales en ${CREDENTIALS_FILE}..."
cat > "${CREDENTIALS_FILE}" << EOF
# ============================================
# E-Commerce Microservices - Credenciales
# Generado: $(date)
# ============================================

# PostgreSQL
POSTGRES_HOST=ecommerce-postgres
POSTGRES_PORT=5432
POSTGRES_USER=postgres
POSTGRES_PASSWORD=${POSTGRES_PASSWORD}
POSTGRES_DB=ecommerce
POSTGRES_CONNECTION_STRING=Host=ecommerce-postgres;Database=ecommerce;Username=postgres;Password=${POSTGRES_PASSWORD}

# Redis
REDIS_HOST=ecommerce-redis
REDIS_PORT=6379
REDIS_PASSWORD=${REDIS_PASSWORD}
REDIS_CONNECTION_STRING=ecommerce-redis:6379,password=${REDIS_PASSWORD}

# RabbitMQ
RABBITMQ_HOST=ecommerce-rabbitmq
RABBITMQ_PORT=5672
RABBITMQ_MANAGEMENT_PORT=15672
RABBITMQ_USER=admin
RABBITMQ_PASSWORD=${RABBITMQ_PASSWORD}

# JWT
JWT_SECRET=${JWT_SECRET}

# URLs (actualizar con tu dominio)
API_GATEWAY_URL=https://${API_SUBDOMAIN}.${DOMAIN}
AUTH_URL=https://${AUTH_SUBDOMAIN}.${DOMAIN}
FRONTEND_URL=https://${DOMAIN}

# Dokploy Dashboard
DOKPLOY_URL=http://$(curl -s ifconfig.me):3000
EOF

chmod 600 "${CREDENTIALS_FILE}"

# ============================================
# 11. Crear script de variables de entorno
# ============================================
log "Creando script de variables de entorno..."
cat > /root/env-vars.sh << EOF
#!/bin/bash
# Variables de entorno para los microservicios

# Database
export Database__Provider=PostgreSQL
export ConnectionStrings__DefaultConnection="Host=ecommerce-postgres;Database=ecommerce;Username=postgres;Password=${POSTGRES_PASSWORD}"

# Redis
export Redis__ConnectionString="ecommerce-redis:6379,password=${REDIS_PASSWORD}"

# RabbitMQ
export RabbitMQ__Host=ecommerce-rabbitmq
export RabbitMQ__Port=5672
export RabbitMQ__Username=admin
export RabbitMQ__Password=${RABBITMQ_PASSWORD}

# JWT
export Jwt__SecretKey=${JWT_SECRET}
export Jwt__Issuer=https://${API_SUBDOMAIN}.${DOMAIN}
export Jwt__Audience=ecommerce-api

# Environment
export ASPNETCORE_ENVIRONMENT=Production
EOF

chmod +x /root/env-vars.sh

# ============================================
# 12. Crear docker-compose para servicios
# ============================================
log "Creando docker-compose.yml..."
mkdir -p /opt/ecommerce
cat > /opt/ecommerce/docker-compose.yml << EOF
version: '3.8'

services:
  # ============================================
  # Frontend - E-Commerce
  # ============================================
  frontend:
    image: ghcr.io/miltonmolloja/ecommercefrontend:latest
    container_name: ecommerce-frontend
    restart: unless-stopped
    environment:
      - API_GATEWAY_URL=https://${API_SUBDOMAIN}.${DOMAIN}
      - IDENTITY_URL=https://${API_SUBDOMAIN}.${DOMAIN}
      - LOGIN_SERVICE_URL=https://${AUTH_SUBDOMAIN}.${DOMAIN}
      - PRODUCTION=true
    networks:
      - ecommerce-network
    labels:
      - "traefik.enable=true"
      - "traefik.http.routers.frontend.rule=Host(\`${DOMAIN}\`)"
      - "traefik.http.routers.frontend.entrypoints=websecure"
      - "traefik.http.routers.frontend.tls.certresolver=letsencrypt"

  # ============================================
  # Frontend - Auth
  # ============================================
  auth-frontend:
    image: ghcr.io/miltonmolloja/clientsauthenticationfrontend:latest
    container_name: ecommerce-auth-frontend
    restart: unless-stopped
    environment:
      - API_URL=https://${API_SUBDOMAIN}.${DOMAIN}
      - IDENTITY_SERVER_URL=https://${API_SUBDOMAIN}.${DOMAIN}
      - ECOMMERCE_URL=https://${DOMAIN}
      - PRODUCTION=true
    networks:
      - ecommerce-network
    labels:
      - "traefik.enable=true"
      - "traefik.http.routers.auth.rule=Host(\`${AUTH_SUBDOMAIN}.${DOMAIN}\`)"
      - "traefik.http.routers.auth.entrypoints=websecure"
      - "traefik.http.routers.auth.tls.certresolver=letsencrypt"

  # ============================================
  # API Gateway
  # ============================================
  api-gateway:
    image: ghcr.io/miltonmolloja/ecommerce-gateway:latest
    container_name: ecommerce-gateway
    restart: unless-stopped
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:45000
      - Database__Provider=PostgreSQL
      - ConnectionStrings__DefaultConnection=Host=ecommerce-postgres;Database=ecommerce;Username=postgres;Password=${POSTGRES_PASSWORD}
      - Redis__ConnectionString=ecommerce-redis:6379,password=${REDIS_PASSWORD}
      - RabbitMQ__Host=ecommerce-rabbitmq
      - RabbitMQ__Username=admin
      - RabbitMQ__Password=${RABBITMQ_PASSWORD}
    networks:
      - ecommerce-network
    labels:
      - "traefik.enable=true"
      - "traefik.http.routers.api.rule=Host(\`${API_SUBDOMAIN}.${DOMAIN}\`)"
      - "traefik.http.routers.api.entrypoints=websecure"
      - "traefik.http.routers.api.tls.certresolver=letsencrypt"
      - "traefik.http.services.api.loadbalancer.server.port=45000"

  # ============================================
  # Identity Service
  # ============================================
  identity-api:
    image: ghcr.io/miltonmolloja/ecommerce-identity:latest
    container_name: ecommerce-identity
    restart: unless-stopped
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
    networks:
      - ecommerce-network

  # ============================================
  # Catalog Service
  # ============================================
  catalog-api:
    image: ghcr.io/miltonmolloja/ecommerce-catalog:latest
    container_name: ecommerce-catalog
    restart: unless-stopped
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
    image: ghcr.io/miltonmolloja/ecommerce-customer:latest
    container_name: ecommerce-customer
    restart: unless-stopped
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
    image: ghcr.io/miltonmolloja/ecommerce-order:latest
    container_name: ecommerce-order
    restart: unless-stopped
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
    image: ghcr.io/miltonmolloja/ecommerce-cart:latest
    container_name: ecommerce-cart
    restart: unless-stopped
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
    image: ghcr.io/miltonmolloja/ecommerce-payment:latest
    container_name: ecommerce-payment
    restart: unless-stopped
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
    image: ghcr.io/miltonmolloja/ecommerce-notification:latest
    container_name: ecommerce-notification
    restart: unless-stopped
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

volumes:
  postgres_data:
  redis_data:
  rabbitmq_data:
EOF

# ============================================
# 13. Crear script de health check
# ============================================
log "Creando script de health check..."
cat > /opt/ecommerce/health-check.sh << 'EOF'
#!/bin/bash
# Health check para todos los servicios

echo "=========================================="
echo "  E-Commerce Health Check"
echo "  $(date)"
echo "=========================================="
echo ""

# Verificar contenedores
echo "📦 Estado de Contenedores:"
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" | grep ecommerce

echo ""
echo "🔍 Verificando servicios:"

# PostgreSQL
if docker exec ecommerce-postgres pg_isready -U postgres > /dev/null 2>&1; then
    echo "  ✅ PostgreSQL: OK"
else
    echo "  ❌ PostgreSQL: ERROR"
fi

# Redis
if docker exec ecommerce-redis redis-cli ping > /dev/null 2>&1; then
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
echo "💾 Uso de Disco:"
df -h / | tail -1

echo ""
echo "🧠 Uso de Memoria:"
free -h | head -2
EOF

chmod +x /opt/ecommerce/health-check.sh

# ============================================
# 14. Crear script de backup
# ============================================
log "Creando script de backup..."
cat > /opt/ecommerce/backup.sh << 'EOF'
#!/bin/bash
# Backup de base de datos PostgreSQL

BACKUP_DIR="/opt/ecommerce/backups"
DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="${BACKUP_DIR}/ecommerce_${DATE}.sql.gz"

mkdir -p "${BACKUP_DIR}"

echo "Creando backup: ${BACKUP_FILE}"
docker exec ecommerce-postgres pg_dump -U postgres ecommerce | gzip > "${BACKUP_FILE}"

# Mantener solo los últimos 7 backups
ls -t "${BACKUP_DIR}"/*.sql.gz | tail -n +8 | xargs -r rm

echo "Backup completado: ${BACKUP_FILE}"
echo "Backups disponibles:"
ls -lh "${BACKUP_DIR}"
EOF

chmod +x /opt/ecommerce/backup.sh

# Programar backup diario
(crontab -l 2>/dev/null; echo "0 2 * * * /opt/ecommerce/backup.sh >> /var/log/ecommerce-backup.log 2>&1") | crontab -

# ============================================
# 15. Resumen Final
# ============================================
VPS_IP=$(curl -s ifconfig.me)

echo ""
echo -e "${GREEN}============================================${NC}"
echo -e "${GREEN}   ¡Instalación Completada!                ${NC}"
echo -e "${GREEN}============================================${NC}"
echo ""
echo -e "${BLUE}📋 Servicios de Infraestructura:${NC}"
echo "   PostgreSQL: ecommerce-postgres:5432"
echo "   Redis:      ecommerce-redis:6379"
echo "   RabbitMQ:   ecommerce-rabbitmq:5672 (Management: 15672)"
echo ""
echo -e "${BLUE}🔗 URLs de Acceso:${NC}"
echo "   Dokploy Dashboard: http://${VPS_IP}:3000"
echo "   RabbitMQ Management: http://${VPS_IP}:15672"
echo ""
echo -e "${BLUE}📁 Archivos Importantes:${NC}"
echo "   Credenciales:    ${CREDENTIALS_FILE}"
echo "   Docker Compose:  /opt/ecommerce/docker-compose.yml"
echo "   Health Check:    /opt/ecommerce/health-check.sh"
echo "   Backup Script:   /opt/ecommerce/backup.sh"
echo ""
echo -e "${YELLOW}⚠️  PRÓXIMOS PASOS:${NC}"
echo "   1. Accede a Dokploy: http://${VPS_IP}:3000"
echo "   2. Crea una cuenta de administrador"
echo "   3. Conecta tus repositorios de GitHub"
echo "   4. Configura los dominios y SSL"
echo "   5. Revisa las credenciales: cat ${CREDENTIALS_FILE}"
echo ""
echo -e "${GREEN}✅ Infraestructura lista para desplegar microservicios${NC}"
