#!/bin/bash
# ═══════════════════════════════════════════════════════════
#  UniStart Database Backup Script
#  Usage: ./scripts/backup.sh [daily|weekly|monthly]
#  Requires: pg_dump, gzip
#  Environment variables:
#    PGHOST, PGPORT, PGUSER, PGPASSWORD, PGDATABASE
#    BACKUP_DIR (default: ./backups)
#    S3_BUCKET (optional, for off-site storage)
# ═══════════════════════════════════════════════════════════

set -euo pipefail

# Configuration
BACKUP_TYPE="${1:-daily}"
BACKUP_DIR="${BACKUP_DIR:-./backups}"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
DB_NAME="${PGDATABASE:-unistart}"
RETENTION_DAILY=7
RETENTION_WEEKLY=4
RETENTION_MONTHLY=2

# Create backup directory
mkdir -p "${BACKUP_DIR}/${BACKUP_TYPE}"

FILENAME="${DB_NAME}_${BACKUP_TYPE}_${TIMESTAMP}.sql.gz"
FILEPATH="${BACKUP_DIR}/${BACKUP_TYPE}/${FILENAME}"

echo "══════════════════════════════════════════"
echo "  UniStart Backup - ${BACKUP_TYPE}"
echo "  Database: ${DB_NAME}"
echo "  Time: $(date)"
echo "══════════════════════════════════════════"

# Perform backup
echo "[1/4] Running pg_dump..."
pg_dump \
  --format=plain \
  --no-owner \
  --no-privileges \
  --verbose \
  "${DB_NAME}" 2>/dev/null | gzip > "${FILEPATH}"

FILESIZE=$(du -h "${FILEPATH}" | cut -f1)
echo "[2/4] Backup created: ${FILEPATH} (${FILESIZE})"

# Verify backup is not empty
if [ ! -s "${FILEPATH}" ]; then
  echo "ERROR: Backup file is empty!"
  rm -f "${FILEPATH}"
  exit 1
fi

# Upload to S3/R2 (optional)
if [ -n "${S3_BUCKET:-}" ]; then
  echo "[3/4] Uploading to ${S3_BUCKET}..."
  aws s3 cp "${FILEPATH}" "s3://${S3_BUCKET}/backups/${BACKUP_TYPE}/${FILENAME}" --quiet
  echo "       Uploaded successfully."
else
  echo "[3/4] S3_BUCKET not set, skipping off-site upload."
fi

# Cleanup old backups
echo "[4/4] Cleaning up old backups..."
case "${BACKUP_TYPE}" in
  daily)
    find "${BACKUP_DIR}/daily" -name "*.sql.gz" -mtime +${RETENTION_DAILY} -delete 2>/dev/null || true
    echo "       Removed daily backups older than ${RETENTION_DAILY} days."
    ;;
  weekly)
    find "${BACKUP_DIR}/weekly" -name "*.sql.gz" -mtime +$((RETENTION_WEEKLY * 7)) -delete 2>/dev/null || true
    echo "       Removed weekly backups older than ${RETENTION_WEEKLY} weeks."
    ;;
  monthly)
    find "${BACKUP_DIR}/monthly" -name "*.sql.gz" -mtime +$((RETENTION_MONTHLY * 30)) -delete 2>/dev/null || true
    echo "       Removed monthly backups older than ${RETENTION_MONTHLY} months."
    ;;
esac

echo ""
echo "✅ Backup complete: ${FILENAME} (${FILESIZE})"
echo "══════════════════════════════════════════"

# List recent backups
echo ""
echo "Recent ${BACKUP_TYPE} backups:"
ls -lh "${BACKUP_DIR}/${BACKUP_TYPE}/" 2>/dev/null | tail -5
