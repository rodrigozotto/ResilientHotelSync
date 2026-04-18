# Execute esse script no powersehll na raiz da sln para testar.

Write-Host "--- Iniciando Orquestração de Testes de Integração ---" -ForegroundColor Cyan

# 1. Garante que a Infra (SQL/Azurite) está UP
Write-Host "1. Subindo Containers (Docker)..." -ForegroundColor Gray
docker-compose up -d

# 2. Sobe a API em background (sem abrir janela)
Write-Host "2. Iniciando API em background..." -ForegroundColor Gray
$apiProcess = Start-Process dotnet -ArgumentList "run --project HotelSync.Api" -PassThru -NoNewWindow

# 3. Aguarda a API estabilizar (Health Check simples)
Write-Host "3. Aguardando API ficar online..." -ForegroundColor Gray
Start-Sleep -Seconds 10 

# 4. Executa os Testes de Integração
Write-Host "4. Executando xUnit Tests..." -ForegroundColor Yellow
dotnet test HotelSync.Tests

# 5. Finaliza a API após o teste
Write-Host "5. Limpando ambiente..." -ForegroundColor Gray
Stop-Process -Id $apiProcess.Id -Force

Write-Host "`n✅ Ciclo de teste finalizado com sucesso!" -ForegroundColor Green