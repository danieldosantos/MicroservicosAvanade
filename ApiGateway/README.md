# ApiGateway

Gateway construído com [Ocelot](https://github.com/ThreeMammals/Ocelot) para rotear chamadas aos serviços `InventoryService` e `SalesService` com autenticação JWT.

## Rotas

| Caminho (Upstream) | Métodos | Destino (Downstream) |
|--------------------|---------|----------------------|
| `/inventory/{*}`   | GET, POST, PUT, DELETE | `http://localhost:5001/api/inventory/{*}` |
| `/sales/{*}`       | GET, POST, PUT, DELETE | `http://localhost:5002/api/sales/{*}` |

### Parâmetros

- `*` representa qualquer segmento adicional encaminhado ao serviço de destino.
- Parâmetros de query string são encaminhados automaticamente.

## Autenticação

Todas as requisições precisam incluir um token JWT válido no cabeçalho `Authorization` utilizando o esquema Bearer.
