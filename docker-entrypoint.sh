#!/bin/bash
set -e

# Wait for SQL Server to be ready
echo "Waiting for SQL Server to be ready..."
/wait-for-it.sh sqlserver:1433 -t 60

# Run migrations
echo "Running database migrations..."
dotnet ef database update --project AccessManagementAPI

# Start the application
echo "Starting the application..."
exec "$@"