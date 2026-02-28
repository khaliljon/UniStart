#!/bin/bash
# ═══════════════════════════════════════════════════════════
#  UniStart Database Restore Script
#  Usage: ./scripts/restore.sh <backup_file.sql.gz>
#  Requires: psql, gunzip
#  Environment variables:
#    PGHOST, PGPORT, PGUSER, PGPASSWORD, PGDATABASE
#
#  WARNING: This will DROP and recreate the target database!
# ═══════════════════════════════════════════════════════════

set -euo pipefail

BACKUP_FILE="${1:-}"
DB_NAME="${PGDATABASE:-unistart}"

if [ -z "${BACKUP_FILE}" ]; then
  echo "Usage: $0 <backup_file.sql.gz>"
  echo ""
  echo "Available backups:"
  find ./backups -name "*.sql.gz" -type f 2>/dev/null | sort -r | head -15
  exit 1
fi

if [ ! -f "${BACKUP_FILE}" ]; then
  echo "ERROR: File not found: ${BACKUP_FILE}"
  exit 1
fi

FILESIZE=$(du -h "${BACKUP_FILE}" | cut -f1)

echo "══════════════════════════════════════════"
echo "  UniStart Database Restore"
echo "  Source: ${BACKUP_FILE} (${FILESIZE})"
echo "  Target: ${DB_NAME}"
echo "  Time: $(date)"
echo "══════════════════════════════════════════"
echo ""
echo "⚠️  WARNING: This will DROP database '${DB_NAME}' and recreate it!"
echo ""
read -p "Are you sure? Type 'YES' to continue: " CONFIRM

if [ "${CONFIRM}" != "YES" ]; then
  echo "Aborted."
  exit 0
fi

echo ""
echo "[1/4] Terminating existing connections..."
psql -d postgres -c "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '${DB_NAME}' AND pid <> pg_backend_pid();" 2>/dev/null || true

echo "[2/4] Dropping and recreating database..."
psql -d postgres -c "DROP DATABASE IF EXISTS \"${DB_NAME}\";"
psql -d postgres -c "CREATE DATABASE \"${DB_NAME}\";"

echo "[3/4] Restoring from backup..."
gunzip -c "${BACKUP_FILE}" | psql -d "${DB_NAME}" --quiet 2>/dev/null

echo "[4/4] Verifying restore..."
TABLE_COUNT=$(psql -d "${DB_NAME}" -t -c "SELECT count(*) FROM information_schema.tables WHERE table_schema = 'public';" | tr -d ' ')
echo "       Tables restored: ${TABLE_COUNT}"

USER_COUNT=$(psql -d "${DB_NAME}" -t -c "SELECT count(*) FROM \"Users\";" 2>/dev/null | tr -d ' ' || echo "?")
QUESTION_COUNT=$(psql -d "${DB_NAME}" -t -c "SELECT count(*) FROM \"Questions\";" 2>/dev/null | tr -d ' ' || echo "?")
echo "       Users: ${USER_COUNT}, Questions: ${QUESTION_COUNT}"

echo ""
echo "✅ Restore complete!"
echo ""
echo "Next steps:"
echo "  1. Run 'dotnet ef database update' if there are pending migrations"
echo "  2. Restart the application: 'dotnet run'"
echo "  3. Verify the application works correctly"
echo "══════════════════════════════════════════"
