# Microservices Authentication Setup

Este repositório contém quatro serviços de exemplo que demonstram autenticação com JWT:

- **AuthService**: fornece `/login` para gerar tokens JWT.
- **InventoryService**: protege `/items` com middleware de autenticação.
- **SalesService**: protege `/sales` com middleware de autenticação.
- **ApiGateway**: valida tokens antes de permitir acesso a `/status`.

## Fluxo de obtenção e uso de tokens

1. Envie uma requisição `POST` para `AuthService`:
   ```bash
   curl -X POST http://localhost:3001/login -H "Content-Type: application/json" -d '{"username":"user"}'
   ```
   Resposta:
   ```json
   { "token": "<jwt>" }
   ```
2. Use o token para acessar os serviços protegidos, adicionando o cabeçalho `Authorization`:
   ```bash
   curl http://localhost:3002/items -H "Authorization: Bearer <jwt>"
   curl http://localhost:3003/sales -H "Authorization: Bearer <jwt>"
   curl http://localhost:3000/status -H "Authorization: Bearer <jwt>"
   ```

Cada serviço valida o token e expõe informações com base no usuário autenticado.
