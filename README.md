# Docker Setup for AccessManagementAPI

This guide explains how to run the AccessManagementAPI application using Docker. The setup includes containerization of the API, SQL Server for the database, and MailHog for email testing.

## Prerequisites

- [Docker](https://docs.docker.com/get-docker/)
- [Docker Compose](https://docs.docker.com/compose/install/)

## Files Overview

1. **Dockerfile**: Defines how to build the API container
2. **docker-compose.yml**: Orchestrates all services (API, SQL Server, MailHog)
3. **docker-entrypoint.sh**: Script that runs before starting the API to ensure dependencies are ready
4. **wait-for-it.sh**: Helper script to wait for services to be available
5. **.dockerignore**: Specifies which files to exclude from the Docker build

## Running the Application

### 1. Create the necessary files

Place all the Docker-related files in your solution root directory:

- `Dockerfile`
- `docker-compose.yml`
- `docker-entrypoint.sh`
- `wait-for-it.sh`
- `.dockerignore`

These should all be at the same level as your `AccessManagement.sln` file.

Make sure the shell scripts have execute permissions:

```bash
chmod +x docker-entrypoint.sh
chmod +x wait-for-it.sh
```

### 2. Build and start the containers

```bash
docker-compose up -d --build
```

This command builds the API image and starts all services in detached mode.

### 3. Check if services are running

```bash
docker-compose ps
```

You should see all services (api, sqlserver, mailhog) running.

### 4. Access the application

- API: http://localhost:8080
- Swagger UI: http://localhost:8080/swagger
- MailHog UI (for testing emails): http://localhost:8025

## Environment Variables

The Docker Compose file sets these environment variables:

- **Database Connection**: Connection string to SQL Server
- **JWT Settings**: Key, issuer, and audience for JWT tokens
- **Email Settings**: SMTP configuration for email verification and password reset
- **Base URL**: Base URL for links in emails

You can modify these in the `docker-compose.yml` file or override them using an environment file.

## Data Persistence

The setup includes Docker volumes for:

1. **sqlserver-data**: Persists the SQL Server database data
2. **api-logs**: Stores application logs

This ensures your data isn't lost when containers are restarted.

## Stopping the Application

```bash
docker-compose down
```

To completely remove volumes (which will delete all data):

```bash
docker-compose down -v
```

## Troubleshooting

### Checking Logs

```bash
# API logs
docker-compose logs api

# Database logs
docker-compose logs sqlserver

# Email server logs
docker-compose logs mailhog

# Follow logs in real-time
docker-compose logs -f api
```

### Database Issues

If the database isn't initializing properly:

1. Check if SQL Server is running:
   ```bash
   docker-compose ps sqlserver
   ```

2. Check SQL Server logs:
   ```bash
   docker-compose logs sqlserver
   ```

3. Ensure the migrations are running:
   ```bash
   docker-compose exec api ls -la /app
   ```

### Manual Database Migration

If you need to manually run migrations:

```bash
docker-compose exec api dotnet ef database update --project AccessManagementAPI
```

## Common Commands

Here are some helpful Docker commands:

```bash
# Rebuild and restart a specific service
docker-compose up -d --build api

# Start a bash shell in the container
docker-compose exec api bash

# View container CPU and memory usage
docker stats

# Restart a specific container
docker-compose restart api
```

## Production Deployment Considerations

For production deployment, consider these additional steps:

1. **Use stronger JWT key**: Generate a secure random key
2. **Set up SSL**: Configure proper SSL certificates
3. **Use a production email service**: Replace MailHog with a real SMTP server
4. **Set up backups**: Configure database backups
5. **Use Docker Swarm or Kubernetes**: For higher availability

## Security Considerations

1. Change the default SQL Server password in `docker-compose.yml`
2. Use a strong JWT key for production
3. Hide sensitive data using Docker secrets or environment files
4. Run containers with non-root users (requires Dockerfile adjustments)
5. Implement regular security updates

## Using a Different Database

To use a different SQL Server instance:

1. Remove the `sqlserver` service from `docker-compose.yml`
2. Update the connection string in the `api` service to point to your SQL Server