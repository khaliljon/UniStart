# ═══════════════════════════════════════════════════════
#  UniStart API — multi-stage Docker build (.NET 8)
# ═══════════════════════════════════════════════════════

# ── Stage 1: restore & publish ──────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY UniStart.csproj ./
RUN dotnet restore UniStart.csproj

# EF Core tools for migrations
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

COPY . .
RUN dotnet publish UniStart.csproj -c Release -o /app/publish --no-restore

# ── Stage 2: runtime ────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Tesseract OCR runtime deps (question import) + postgresql-client-17 (DB backups).
# pg_dump must be >= the PostgreSQL server version (17), so install from the
# official PGDG apt repo rather than Debian's default (which ships v15).
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
        libleptonica-dev libtesseract-dev curl ca-certificates gnupg && \
    install -d /usr/share/postgresql-common/pgdg && \
    curl -fsSL https://www.postgresql.org/media/keys/ACCC4CF8.asc \
        -o /usr/share/postgresql-common/pgdg/apt.postgresql.org.asc && \
    echo "deb [signed-by=/usr/share/postgresql-common/pgdg/apt.postgresql.org.asc] https://apt.postgresql.org/pub/repos/apt bookworm-pgdg main" \
        > /etc/apt/sources.list.d/pgdg.list && \
    apt-get update && \
    apt-get install -y --no-install-recommends postgresql-client-17 && \
    rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# tessdata language models
COPY tessdata ./tessdata

ENV ASPNETCORE_URLS=http://+:5009
ENV ASPNETCORE_ENVIRONMENT=Production
ENV BACKUP_DIR=/backups

EXPOSE 5009

HEALTHCHECK --interval=30s --timeout=5s --retries=3 \
    CMD wget -qO- http://localhost:5009/health/live || exit 1

ENTRYPOINT ["dotnet", "UniStart.dll"]
