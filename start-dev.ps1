Write-Host "Starting REA System in Development Mode..." -ForegroundColor Green

# Build and start containers
docker-compose down
docker-compose up --build -d

Write-Host "Services starting..." -ForegroundColor Yellow
Write-Host "Database: localhost:5432" -ForegroundColor Cyan
Write-Host "API: localhost:8080" -ForegroundColor Cyan
Write-Host "Client: localhost:3000" -ForegroundColor Cyan

# Wait for services to be ready
Start-Sleep -Seconds 10

# Show logs
docker-compose logs -f