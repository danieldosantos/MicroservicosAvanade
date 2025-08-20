# SalesService

## Endpoints

- `GET /api/orders`: lists all orders with their current status.
- `GET /api/orders/{id}`: retrieves a specific order.
- `POST /api/orders`: creates a new order.

## Order Status

Each order includes a `Status` field (default: `Pending`) to track its processing state.

## Generating EF Core Migration Scripts

Set the `DATABASE_URL` environment variable to the connection string for the sales database. Examples:

```bash
export DATABASE_URL="Server=localhost;Database=SalesDb;User Id=sa;Password=Your_password123;"  # SQL Server
export DATABASE_URL="Host=localhost;Database=salesdb;Username=postgres;Password=secret"          # PostgreSQL
```

Generate a SQL script from the migrations:

```bash
dotnet ef migrations script -o sales.sql
```

### Running the Script

**SQL Server**

```bash
sqlcmd -S localhost -d SalesDb -i sales.sql
```

**PostgreSQL**

```bash
psql "$DATABASE_URL" -f sales.sql
```
