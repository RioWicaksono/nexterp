#!/bin/sh
# ============================================================
# NEXTERP ERP - PostgreSQL Backup Script
# ============================================================
# Usage:
#   docker compose --profile backup up postgres-backup
#   Or run manually: docker exec nexterp_postgres_prod pg_dump...
# ============================================================

set -e

# Configuration from environment
POSTGRES_HOST="${POSTGRES_HOST:-postgres}"
POSTGRES_PORT="${POSTGRES_PORT:-5432}"
POSTGRES_USER="${POSTGRES_USER:-postgres}"
POSTGRES_PASSWORD="${POSTGRES_PASSWORD}"
POSTGRES_DB="${POSTGRES_DB:-nexterp_db}"
BACKUP_RETENTION_DAYS="${BACKUP_RETENTION_DAYS:-30}"
BACKUP_DIR="/backup"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

log_info() {
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] ${GREEN}[INFO]${NC} $1"
}

log_warn() {
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] ${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] ${RED}[ERROR]${NC} $1"
}

# Wait for PostgreSQL to be ready
wait_for_postgres() {
    log_info "Waiting for PostgreSQL at ${POSTGRES_HOST}:${POSTGRES_PORT}..."

    for i in $(seq 1 30); do
        if PGPASSWORD="${POSTGRES_PASSWORD}" pg_isready -h "${POSTGRES_HOST}" -p "${POSTGRES_PORT}" -U "${POSTGRES_USER}" -q; then
            log_info "PostgreSQL is ready!"
            return 0
        fi
        log_warn "PostgreSQL not ready yet. Attempt $i/30..."
        sleep 2
    done

    log_error "PostgreSQL did not become ready in time."
    exit 1
}

# Create backup
create_backup() {
    TIMESTAMP=$(date '+%Y%m%d_%H%M%S')
    BACKUP_FILE="${BACKUP_DIR}/nexterp_backup_${TIMESTAMP}.sql.gz"
    BACKUP_METADATA="${BACKUP_DIR}/nexterp_backup_${TIMESTAMP}.meta"

    log_info "Starting backup of database '${POSTGRES_DB}'..."
    log_info "Backup file: ${BACKUP_FILE}"

    # Export PGPASSWORD to avoid password prompt
    export PGPASSWORD

    # Create compressed backup
    if pg_dump -h "${POSTGRES_HOST}" \
               -p "${POSTGRES_PORT}" \
               -U "${POSTGRES_USER}" \
               -d "${POSTGRES_DB}" \
               --format=custom \
               --compress=9 \
               --no-owner \
               --no-acl \
               --verbose \
               -f "${BACKUP_FILE}.dump"; then

        # Create metadata file
        cat > "${BACKUP_METADATA}" << EOF
{
    "backup_date": "$(date -Iseconds)",
    "database": "${POSTGRES_DB}",
    "host": "${POSTGRES_HOST}",
    "format": "custom",
    "compression": 9,
    "file_size_bytes": $(stat -f%z "${BACKUP_FILE}.dump" 2>/dev/null || stat -c%s "${BACKUP_FILE}.dump" 2>/dev/null || echo 0),
    "retention_days": ${BACKUP_RETENTION_DAYS}
}
EOF

        log_info "Backup completed successfully!"
        log_info "File size: $(du -h "${BACKUP_FILE}.dump" | cut -f1)"

        # Create latest symlink
        ln -sf "$(basename "${BACKUP_FILE}.dump")" "${BACKUP_DIR}/latest.dump"
        ln -sf "$(basename "${BACKUP_METADATA}")" "${BACKUP_DIR}/latest.meta"

        return 0
    else
        log_error "Backup failed!"
        return 1
    fi
}

# Cleanup old backups
cleanup_old_backups() {
    log_info "Cleaning up backups older than ${BACKUP_RETENTION_DAYS} days..."

    # Find and remove old backup files
    find "${BACKUP_DIR}" -name "nexterp_backup_*.dump" -type f -mtime +"${BACKUP_RETENTION_DAYS}" -delete
    find "${BACKUP_DIR}" -name "nexterp_backup_*.meta" -type f -mtime +"${BACKUP_RETENTION_DAYS}" -delete

    # Remove broken symlinks
    find "${BACKUP_DIR}" -type l ! -exec test -e {} \; -delete

    log_info "Cleanup completed."
}

# List backups
list_backups() {
    log_info "Available backups:"
    ls -lh "${BACKUP_DIR}"/nexterp_backup_*.dump 2>/dev/null || log_warn "No backups found."
}

# Restore from backup (requires confirmation)
restore_backup() {
    BACKUP_FILE="$1"

    if [ -z "${BACKUP_FILE}" ]; then
        log_error "Usage: restore_backup <backup_file>"
        return 1
    fi

    if [ ! -f "${BACKUP_FILE}" ]; then
        log_error "Backup file not found: ${BACKUP_FILE}"
        return 1
    fi

    log_warn "This will overwrite the current database!"
    log_warn "Press Ctrl+C to cancel or wait 10 seconds to continue..."
    sleep 10

    export PGPASSWORD

    log_info "Restoring from ${BACKUP_FILE}..."

    if pg_restore -h "${POSTGRES_HOST}" \
                   -p "${POSTGRES_PORT}" \
                   -U "${POSTGRES_USER}" \
                   -d "${POSTGRES_DB}" \
                   --clean \
                   --if-exists \
                   --verbose \
                   "${BACKUP_FILE}"; then
        log_info "Restore completed successfully!"
        return 0
    else
        log_error "Restore failed!"
        return 1
    fi
}

# Main execution
main() {
    log_info "=========================================="
    log_info "NEXTERP ERP - PostgreSQL Backup Script"
    log_info "=========================================="

    # Ensure backup directory exists
    mkdir -p "${BACKUP_DIR}"

    # Wait for PostgreSQL
    wait_for_postgres

    # Execute backup
    create_backup

    # Cleanup old backups
    cleanup_old_backups

    # List current backups
    list_backups

    log_info "=========================================="
    log_info "Backup process completed!"
    log_info "=========================================="
}

# Handle script arguments
case "${1:-backup}" in
    backup)
        main
        ;;
    restore)
        restore_backup "${2}"
        ;;
    list)
        list_backups
        ;;
    *)
        echo "Usage: $0 {backup|restore <file>|list}"
        exit 1
        ;;
esac
