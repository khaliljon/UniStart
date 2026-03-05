# ═══════════════════════════════════════════════════════
#  UniStart Client — Vite build + nginx
# ═══════════════════════════════════════════════════════

# ── Stage 1: build React app ────────────────────────────
FROM node:20-alpine AS build
WORKDIR /app

COPY package.json package-lock.json* ./
RUN [ -f package-lock.json ] && npm ci || npm install

COPY . .
RUN npm run build

# ── Stage 2: serve via nginx ────────────────────────────
FROM nginx:1.27-alpine AS runtime

COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

HEALTHCHECK --interval=30s --timeout=5s --retries=3 \
    CMD wget -qO- http://localhost/health || exit 1
