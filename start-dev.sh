#!/bin/bash
echo "Starting REA System in Development Mode..."

# Build and start containers
docker-compose down
docker-compose up --build -d

echo "Services starting..."
echo "Database: localhost:5432"
echo "API: localhost:8080" 
echo "Client: localhost:3000"

# Wait for services to be ready
sleep 10

# Show logs
docker-compose logs -f