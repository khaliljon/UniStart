#!/bin/bash
# ═══════════════════════════════════════════════════════
#  UniStart — PostgreSQL Daily Backup
# ═══════════════════════════════════════════════════════
#  Usage: Add to crontab:
#    0 3 * * * /opt/unistart/scripts/backup-db.sh
#
#  Keeps last 7 daily backups.
# ═══════════════════════════════════════════════════════

set -euo pipefail

BACKUP_DIR="/opt/unistart/backups"
RETENTION_DAYS=7
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="${BACKUP_DIR}/unistart_${TIMESTAMP}.sql.gz"

mkdir -p "$BACKUP_DIR"

# Dump via docker compose
docker compose -f /opt/unistart/docker-compose.yml exec -T postgres \
  pg_dump -U "${POSTGRES_USER:-postgres}" "${POSTGRES_DB:-UniStart}" \
  | gzip > "$BACKUP_FILE"

# Verify non-empty
if [ ! -s "$BACKUP_FILE" ]; then
  echo "ERROR: Backup file is empty" >&2
  rm -f "$BACKUP_FILE"
  exit 1
fi

# Remove old backups
find "$BACKUP_DIR" -name "unistart_*.sql.gz" -mtime +${RETENTION_DAYS} -delete

echo "Backup OK: ${BACKUP_FILE} ($(du -h "$BACKUP_FILE" | cut -f1))"
