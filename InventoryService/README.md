# InventoryService

## Generating EF Core Migration Scripts

Set the `DATABASE_URL` environment variable to the connection string of the target database. Example values:

```bash
export DATABASE_URL="Server=localhost;Database=InventoryDb;User Id=sa;Password=Your_password123;"  # SQL Server
export DATABASE_URL="Host=localhost;Database=inventorydb;Username=postgres;Password=secret"      # PostgreSQL
```

Generate a SQL script from the migrations:

```bash
dotnet ef migrations script -o inventory.sql
```

### Running the Script

**SQL Server**

```bash
sqlcmd -S localhost -d InventoryDb -i inventory.sql
```

**PostgreSQL**

```bash
psql "$DATABASE_URL" -f inventory.sql
```
