# Resilient Hotel Rate Sync 🏨⚡

Este projeto demonstra uma arquitetura robusta para sincronização de tarifas de hotéis, focada em **escalabilidade**, **resiliência** e **idempotência**.

## 🚀 Arquitetura

O sistema foi desenhado seguindo o padrão **Producer-Consumer** (Produtor-Consumidor) para desacoplar a recepção de dados do seu processamento pesado.

- **API (ASP.NET Core 8):** Atua como o produtor, validando a unicidade da requisição e enfileirando a tarefa.
- **Worker Service:** Atua como o consumidor, processando as mensagens da fila de forma assíncrona.
- **SQL Server:** Utilizado para persistência de logs e garantia de idempotência via índices únicos.
- **Azure Queue Storage (Azurite):** Middleware de mensageria para garantir que nenhuma tarefa seja perdida.



## 🛠️ Tecnologias Utilizadas

- **.NET 8** (C#)
- **Entity Framework Core**
- **Docker & Docker Compose**
- **Azurite** (Emulador de Azure Storage)
- **SQL Server for Linux**

## 💡 Conceitos Implementados

### 1. Idempotência com X-Idempotency-Key
Para evitar o reprocessamento de cobranças ou atualizações em caso de falhas de rede (retries do cliente), implementamos o uso de uma chave de idempotência no cabeçalho HTTP. O SQL Server garante que a mesma chave não seja inserida duas vezes.

### 2. Resiliência de Conexão
Configuramos o `EnableRetryOnFailure` no Entity Framework para lidar com falhas transitórias de conexão com o banco de dados.

### 3. Visibility Timeout
O Worker utiliza o conceito de confirmação (ACK). Se o processo falhar antes do término, a mensagem volta para a fila automaticamente, garantindo **Garantia de Entrega**.

## 🔧 Como rodar o projeto

### Como executar os testes (Script de Orquestração)

Copie o bloco de código abaixo e execute-o no seu terminal **PowerShell** na raiz do projeto. Este script automatiza o ciclo de vida completo do teste:

```powershellcls
# --- LIMPEZA PREVENTIVA ---
Write-Host "Limpando processos anteriores na porta 5000..." -ForegroundColor Gray
Get-Process dotnet -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowTitle -eq "" } | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2

# 1. Garante que a Infraestrutura (SQL Server e Azurite) esteja ativa
Write-Host "Subindo containers do Docker..." -ForegroundColor Gray
docker-compose up -d

# 2. Inicia a API em background REDIRECIONANDO O LOG
# Redirecionamos a saída para 'api_log.txt' para o console ficar limpo
Write-Host "Iniciando API HotelSync.Api (Silenciosa)..." -ForegroundColor Gray
$apiProcess = Start-Process dotnet -ArgumentList "run --project HotelSync.Api" -PassThru -NoNewWindow -RedirectStandardOutput "api_log.txt" -RedirectStandardError "api_err.txt"

# 3. Aguarda o tempo de boot
Write-Host "Aguardando inicialização (12s)..." -ForegroundColor Gray
Start-Sleep -Seconds 12

# 4. Executa os Testes de Integração
Write-Host "Executando Testes de Idempotência..." -ForegroundColor Yellow
dotnet test HotelSync.Tests --logger "console;verbosity=minimal"

# 5. Encerra o processo da API de forma segura
Write-Host "Finalizando processos e limpando logs temporários..." -ForegroundColor Gray
if ($apiProcess) {
    Stop-Process -Id $apiProcess.Id -Force -ErrorAction SilentlyContinue
}
# Opcional: Remove os logs se o teste passar, ou deixe para inspeção
Remove-Item "api_log.txt", "api_err.txt" -ErrorAction SilentlyContinue

Write-Host "`n✅ Ciclo de teste concluído com sucesso e console limpo!" -ForegroundColor Green