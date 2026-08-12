# ERP System Docker Setup

## Quick Start

```bash
# Copy environment file
cp .env.example .env

# Start all services
docker-compose up -d

# Check status
docker-compose ps

# View logs
docker-compose logs -f api
```

## Services

| Service   | Port  | Description                    |
|-----------|-------|--------------------------------|
| API       | 5000  | ERP REST API                   |
| PostgreSQL| 5432  | Database                       |
| Redis    | 6379  | Cache                          |
| pgAdmin  | 5050  | PostgreSQL Administration       |

## Accessing Services

### API
- Development: http://localhost:5000
- Swagger UI: http://localhost:5000/swagger

### pgAdmin
- URL: http://localhost:5050
- Email: admin@erp.local (or from .env)
- Password: admin123 (or from .env)

### PostgreSQL
- Host: localhost
- Port: 5432
- Database: erp_db
- Username: postgres
- Password: postgres123 (or from .env)

## Useful Commands

```bash
# Rebuild API after code changes
docker-compose up -d --build api

# Stop all services
docker-compose down

# Stop and remove volumes (fresh start)
docker-compose down -v

# View API logs
docker-compose logs -f api

# View PostgreSQL logs
docker-compose logs -f postgres

# Restart a specific service
docker-compose restart api

# Execute command in container
docker-compose exec api dotnet ef database update
docker-compose exec postgres psql -U postgres -d erp_db
```

## Environment Variables

See `.env.example` for all configuration options.

## Troubleshooting

### Database connection issues
```bash
# Check if PostgreSQL is healthy
docker-compose ps postgres

# View PostgreSQL logs
docker-compose logs postgres

# Restart PostgreSQL
docker-compose restart postgres
```

### API fails to start
```bash
# Check API logs
docker-compose logs api

# Rebuild API
docker-compose up -d --build api
```

### Clean slate (remove all data)
```bash
docker-compose down -v
docker-compose up -d
```
