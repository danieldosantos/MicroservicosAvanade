# Microserviços Avanade: Estoque, Vendas e Autenticação

Este projeto demonstra uma arquitetura de e-commerce baseada em microserviços escritos em .NET 8. Os serviços comunicam-se via HTTP através de um API Gateway e usam RabbitMQ para mensageria assíncrona.

## Serviços

- **ApiGateway** – Porta de entrada para os demais serviços.
- **AuthService** – Autenticação e geração de tokens JWT.
- **InventoryService** – Catálogo e controle de estoque.
- **SalesService** – Registro de pedidos e publicação de eventos de venda.

## Pré-requisitos

- [.NET SDK 8.0](https://dotnet.microsoft.com/en-us/download)
- Banco relacional (SQL Server ou PostgreSQL)
- Docker (para subir dependências locais)

### Subindo o RabbitMQ com Docker

```bash
docker run -d --name rabbit \
  -p 5672:5672 -p 15672:15672 \
  rabbitmq:3-management
```

A interface de administração ficará disponível em `http://localhost:15672` (usuário e senha padrão: `guest` / `guest`).

## Configuração

1. Ajuste as variáveis de ambiente das conexões com o banco.
2. O **SalesService** usa SQLite por padrão (`Data Source=sales.db`). Para outro banco:

```bash
export DATABASE_URL="Server=localhost;Database=SalesDb;User Id=sa;Password=Your_password123;"
```

## Execução

```bash
dotnet build
dotnet run --project ApiGateway
dotnet run --project AuthService
dotnet run --project InventoryService
dotnet run --project SalesService
```

Os endpoints podem ser acessados pelo API Gateway (`http://localhost:8000`).

## Autenticação

Antes de executar os comandos abaixo, inicie os serviços:

```bash
# Serviços .NET
dotnet run --project AuthService      # porta 5004
dotnet run --project InventoryService # porta 5001
dotnet run --project SalesService     # porta 5002
dotnet run --project ApiGateway       # porta 8000

# (Opcional) Serviços Node
cd inventory-service && npm start     # porta 3002
cd sales-service && npm start         # porta 3003
cd ApiGateway && npm start            # porta 3000
```

1. Obtenha um token:

```bash
curl -X POST http://localhost:5004/login \
  -H "Content-Type: application/json" \
  -d '{"username":"user","password":"pass"}'
```

2. Utilize o token:

```bash
curl http://localhost:5001/products -H "Authorization: Bearer <jwt>"
curl http://localhost:5002/api/orders -H "Authorization: Bearer <jwt>"
# ou, se estiver usando as versões Node:
# curl http://localhost:3002/items -H "Authorization: Bearer <jwt>"
# curl http://localhost:3003/sales -H "Authorization: Bearer <jwt>"
```

## Desenvolvimento

- `dotnet restore` para restaurar pacotes.
- Cada serviço possui seu próprio banco e lógica de domínio.
- Eventos de venda via RabbitMQ atualizam o estoque.

## Contribuição

Sinta-se à vontade para abrir issues e pull requests com melhorias.

