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

1. Certifique-se de ter o **Docker** instalado.
2. Na raiz do projeto, execute:
   ```bash
   docker-compose up -d