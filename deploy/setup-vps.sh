#!/bin/bash
# ─────────────────────────────────────────────────────────────
# Pana Platform — VPS Setup Script (Ubuntu 22.04 / 24.04)
# Run as root:  sudo bash setup-vps.sh
# ─────────────────────────────────────────────────────────────
set -e

echo "=== Pana VPS Setup ==="

# ── 1. Update system ────────────────────────────────────────
echo "[1/7] Updating system packages..."
apt update -y && apt upgrade -y

# ── 2. Install Docker ───────────────────────────────────────
echo "[2/7] Installing Docker..."
apt install -y ca-certificates curl
install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc
chmod a+r /etc/apt/keyrings/docker.asc
echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] https://download.docker.com/linux/ubuntu $(. /etc/os-release && echo "$VERSION_CODENAME") stable" | tee /etc/apt/sources.list.d/docker.list > /dev/null
apt update -y
apt install -y docker-ce docker-ce-cli containerd.io docker-compose-plugin

# ── 3. Install Git ──────────────────────────────────────────
echo "[3/7] Installing Git..."
apt install -y git

# ── 4. Create app user ──────────────────────────────────────
echo "[4/7] Creating pana user..."
useradd -m -s /bin/bash pana 2>/dev/null || echo "User pana already exists"
usermod -aG docker pana

# ── 5. Clone repo ───────────────────────────────────────────
echo "[5/7] Cloning repository..."
REPO_URL="${REPO_URL:-https://github.com/aldovargaslt-dotcom/pana.git}"
APP_DIR="/home/pana/app"

if [ -d "$APP_DIR" ]; then
    echo "App directory exists, pulling latest..."
    cd "$APP_DIR"
    git pull
else
    git clone "$REPO_URL" "$APP_DIR"
fi
chown -R pana:pana "$APP_DIR"

# ── 6. Create .env file if missing ──────────────────────────
echo "[6/7] Setting up environment..."
ENV_FILE="$APP_DIR/deploy/.env"
if [ ! -f "$ENV_FILE" ]; then
    JWT_SECRET=$(openssl rand -base64 48)
    cat > "$ENV_FILE" << EOF
# Pana Production Environment
POSTGRES_PASSWORD=$(openssl rand -base64 24)
JWT__KEY=$JWT_SECRET
JWT__ISSUER=Pana
JWT__AUDIENCE=Pana
ASPNETCORE_ENVIRONMENT=Production
EOF
    echo "Generated secure .env file with random passwords."
else
    echo ".env file already exists — keeping existing secrets."
fi

# ── 7. Start services ───────────────────────────────────────
echo "[7/7] Starting Pana..."
cd "$APP_DIR/deploy"
docker compose -f docker-compose.prod.yml up -d

echo ""
echo "=============================================="
echo " Setup complete!"
echo " API:    http://$(curl -s ifconfig.me):8080"
echo " Health: http://$(curl -s ifconfig.me):8080/health"
echo " Swagger: http://$(curl -s ifconfig.me):8080/swagger"
echo ""
echo " Logs:    docker compose -f $APP_DIR/deploy/docker-compose.prod.yml logs -f"
echo " Stop:    docker compose -f $APP_DIR/deploy/docker-compose.prod.yml down"
echo "=============================================="
