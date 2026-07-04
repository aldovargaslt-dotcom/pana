# Pana Platform — Deployment Guide

## What you need

- Ubuntu 22.04 or 24.04 VPS (1 GB RAM minimum, 2 GB recommended)
- A domain name (optional — works with IP)
- 15 minutes

## One-command setup

SSH into your VPS and run:

```bash
sudo bash -c "$(curl -fsSL https://raw.githubusercontent.com/YOUR_USER/pana/main/deploy/setup-vps.sh)"
```

Or clone first, then run:

```bash
git clone https://github.com/YOUR_USER/pana.git
cd pana/deploy
sudo bash setup-vps.sh
```

## What it does

1. Updates system packages
2. Installs Docker + Docker Compose
3. Clones your repo
4. Generates secure random passwords for PostgreSQL and JWT
5. Starts PostgreSQL + API + Nginx

## After setup

| URL | Description |
|-----|-------------|
| `http://YOUR_VPS_IP:8080/swagger` | API docs + test UI |
| `http://YOUR_VPS_IP:8080/health` | Health check |
| `http://YOUR_VPS_IP/` | API via Nginx (port 80) |

## First API call

```bash
# Register the first user
curl -X POST http://YOUR_VPS_IP:8080/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@bakery.com","password":"securepassword123","displayName":"Admin"}'

# Login
curl -X POST http://YOUR_VPS_IP:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@bakery.com","password":"securepassword123"}'
```

## Adding SSL (HTTPS)

When you have a domain pointing to the VPS:

```bash
# Install certbot
sudo apt install -y certbot

# Get certificate (stop nginx first)
cd /home/pana/app/deploy
docker compose -f docker-compose.prod.yml stop nginx
sudo certbot certonly --standalone -d yourdomain.com
docker compose -f docker-compose.prod.yml start nginx

# Update nginx.conf to use SSL, then reload
docker compose -f docker-compose.prod.yml restart nginx
```

## Backups

```bash
# Manual backup
docker exec pana-db pg_dump -U pana pana > backup_$(date +%Y%m%d).sql

# Automated daily backup (add to crontab: crontab -e)
0 3 * * * docker exec pana-db pg_dump -U pana pana > /home/pana/backups/backup_$(date +\%Y\%m\%d).sql
```

## Updating the app

```bash
cd /home/pana/app
git pull
cd deploy
docker compose -f docker-compose.prod.yml up -d --build api
```

## Stopping

```bash
cd /home/pana/app/deploy
docker compose -f docker-compose.prod.yml down
```
