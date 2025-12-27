using Npgsql;

class Program
{
    static async Task Main(string[] args)
    {
        var connectionString = "Host=72.61.128.126;Port=5433;Database=postgres;Username=postgres;Password=3jxEbemom6JTy9dqbrpAoAlNfUVpzmbQ2";
        
        if (args.Length == 0)
        {
            Console.WriteLine("PostgreSQL Migration Tool");
            Console.WriteLine("Usage:");
            Console.WriteLine("  dotnet run -- test                    : Test connection");
            Console.WriteLine("  dotnet run -- schema                  : Create all schemas and tables");
            Console.WriteLine("  dotnet run -- data                    : Import all data");
            Console.WriteLine("  dotnet run -- all                     : Run schema + data");
            Console.WriteLine("  dotnet run -- file <path>             : Execute specific SQL file");
            Console.WriteLine("  dotnet run -- verify                  : Verify migration");
            return;
        }

        var command = args[0].ToLower();
        var scriptsPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "scripts", "deploy");
        
        try
        {
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();
            Console.WriteLine("Connected to PostgreSQL successfully!");

            switch (command)
            {
                case "test":
                    await TestConnection(conn);
                    break;
                case "schema":
                    await ExecuteSchemaScript(conn, scriptsPath);
                    break;
                case "data":
                    await ExecuteDataScripts(conn, scriptsPath);
                    break;
                case "all":
                    await ExecuteSchemaScript(conn, scriptsPath);
                    await ExecuteDataScripts(conn, scriptsPath);
                    break;
                case "file":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Please provide a file path");
                        return;
                    }
                    await ExecuteSqlFile(conn, args[1]);
                    break;
                case "verify":
                    await VerifyMigration(conn);
                    break;
                case "columns":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Please provide schema.table name");
                        return;
                    }
                    await ShowColumns(conn, args[1]);
                    break;
                case "allcolumns":
                    await ShowAllColumns(conn);
                    break;
                default:
                    Console.WriteLine($"Unknown command: {command}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }

    static async Task TestConnection(NpgsqlConnection conn)
    {
        await using var cmd = new NpgsqlCommand("SELECT version()", conn);
        var version = await cmd.ExecuteScalarAsync();
        Console.WriteLine($"PostgreSQL Version: {version}");
        
        // List existing schemas
        await using var schemaCmd = new NpgsqlCommand(
            "SELECT schema_name FROM information_schema.schemata WHERE schema_name NOT IN ('pg_catalog', 'information_schema', 'pg_toast')", 
            conn);
        await using var reader = await schemaCmd.ExecuteReaderAsync();
        Console.WriteLine("\nExisting schemas:");
        while (await reader.ReadAsync())
        {
            Console.WriteLine($"  - {reader.GetString(0)}");
        }
        await reader.CloseAsync();
        
        // List tables
        await using var tableCmd = new NpgsqlCommand(
            @"SELECT table_schema, table_name 
              FROM information_schema.tables 
              WHERE table_schema NOT IN ('pg_catalog', 'information_schema') 
              ORDER BY table_schema, table_name", 
            conn);
        await using var tableReader = await tableCmd.ExecuteReaderAsync();
        Console.WriteLine("\nExisting tables:");
        while (await tableReader.ReadAsync())
        {
            Console.WriteLine($"  - {tableReader.GetString(0)}.{tableReader.GetString(1)}");
        }
    }

    static async Task ExecuteSchemaScript(NpgsqlConnection conn, string scriptsPath)
    {
        var schemaFile = Path.Combine(scriptsPath, "00-create-all-tables-postgres.sql");
        if (!File.Exists(schemaFile))
        {
            Console.WriteLine($"Schema file not found: {schemaFile}");
            return;
        }

        Console.WriteLine("Executing schema creation script...");
        await ExecuteSqlFile(conn, schemaFile);
        Console.WriteLine("Schema creation completed!");
    }

    static async Task ExecuteDataScripts(NpgsqlConnection conn, string scriptsPath)
    {
        var migrationDataPath = Path.Combine(scriptsPath, "migration-data");
        if (!Directory.Exists(migrationDataPath))
        {
            Console.WriteLine($"Migration data directory not found: {migrationDataPath}");
            return;
        }

        var sqlFiles = Directory.GetFiles(migrationDataPath, "*.sql")
            .OrderBy(f => Path.GetFileName(f))
            .ToList();

        Console.WriteLine($"Found {sqlFiles.Count} data files to execute");

        foreach (var file in sqlFiles)
        {
            var fileName = Path.GetFileName(file);
            Console.WriteLine($"Executing: {fileName}");
            try
            {
                await ExecuteSqlFile(conn, file);
                Console.WriteLine($"  OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ERROR: {ex.Message}");
                // Continue with next file
            }
        }

        Console.WriteLine("Data import completed!");
    }

    static async Task ExecuteSqlFile(NpgsqlConnection conn, string filePath)
    {
        var sql = await File.ReadAllTextAsync(filePath);
        
        // Split by GO statements (if any) or semicolons for batch execution
        // For PostgreSQL, we can execute the whole file at once in most cases
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.CommandTimeout = 300; // 5 minutes timeout
        await cmd.ExecuteNonQueryAsync();
    }

    static async Task VerifyMigration(NpgsqlConnection conn)
    {
        Console.WriteLine("\n=== Migration Verification ===\n");

        var tables = new Dictionary<string, string>
        {
            // Catalog
            { "\"Catalog\".\"Brands\"", "Brands" },
            { "\"Catalog\".\"Categories\"", "Categories" },
            { "\"Catalog\".\"Products\"", "Products" },
            { "\"Catalog\".\"ProductCategories\"", "Product Categories" },
            { "\"Catalog\".\"ProductInStock\"", "Product Stock" },
            { "\"Catalog\".\"AttributeValues\"", "Attribute Values" },
            { "\"Catalog\".\"ProductAttributes\"", "Product Attributes" },
            { "\"Catalog\".\"ProductAttributeValues\"", "Product Attribute Values" },
            { "\"Catalog\".\"ProductRatings\"", "Product Ratings" },
            { "\"Catalog\".\"ProductReviews\"", "Product Reviews" },
            { "\"Catalog\".\"Banners\"", "Banners" },
            // Identity
            { "\"Identity\".\"AspNetRoles\"", "Roles" },
            { "\"Identity\".\"AspNetUsers\"", "Users" },
            { "\"Identity\".\"RefreshTokens\"", "Refresh Tokens" },
            { "\"Identity\".\"UserAuditLogs\"", "User Audit Logs" },
            // Customer
            { "\"Customer\".\"Clients\"", "Clients" },
            { "\"Customer\".\"ClientAddresses\"", "Client Addresses" },
            // Order
            { "\"Order\".\"OrderStatuses\"", "Order Statuses" },
            { "\"Order\".\"PaymentTypes\"", "Payment Types" },
            { "\"Order\".\"Orders\"", "Orders" },
            { "\"Order\".\"OrderDetails\"", "Order Details" },
            // Cart
            { "\"Cart\".\"ShoppingCarts\"", "Shopping Carts" },
            { "\"Cart\".\"CartItems\"", "Cart Items" },
            // Payment
            { "\"Payment\".\"Payments\"", "Payments" },
            { "\"Payment\".\"PaymentDetails\"", "Payment Details" },
            { "\"Payment\".\"PaymentTransactions\"", "Payment Transactions" },
            // Notification
            { "\"Notification\".\"Notifications\"", "Notifications" },
            { "\"Notification\".\"NotificationPreferences\"", "Notification Preferences" },
        };

        foreach (var (tableName, displayName) in tables)
        {
            try
            {
                await using var cmd = new NpgsqlCommand($"SELECT COUNT(*) FROM {tableName}", conn);
                var count = await cmd.ExecuteScalarAsync();
                Console.WriteLine($"  {displayName,-30}: {count,8} rows");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  {displayName,-30}: ERROR - {ex.Message}");
            }
        }

        Console.WriteLine("\n=== Verification Complete ===");
    }

    static async Task ShowColumns(NpgsqlConnection conn, string schemaTable)
    {
        var parts = schemaTable.Split('.');
        var schema = parts[0];
        var table = parts.Length > 1 ? parts[1] : parts[0];

        await using var cmd = new NpgsqlCommand(
            @"SELECT column_name, data_type, is_nullable, column_default
              FROM information_schema.columns 
              WHERE table_schema = @schema AND table_name = @table
              ORDER BY ordinal_position", conn);
        cmd.Parameters.AddWithValue("schema", schema);
        cmd.Parameters.AddWithValue("table", table);

        await using var reader = await cmd.ExecuteReaderAsync();
        Console.WriteLine($"\nColumns for {schema}.{table}:");
        while (await reader.ReadAsync())
        {
            var nullable = reader.GetString(2) == "YES" ? "NULL" : "NOT NULL";
            Console.WriteLine($"  {reader.GetString(0),-30} {reader.GetString(1),-20} {nullable}");
        }
    }

    static async Task ShowAllColumns(NpgsqlConnection conn)
    {
        var schemas = new[] { "Catalog", "Identity", "Customer", "Order", "Cart", "Payment", "Notification" };
        
        foreach (var schema in schemas)
        {
            await using var tableCmd = new NpgsqlCommand(
                @"SELECT table_name FROM information_schema.tables 
                  WHERE table_schema = @schema ORDER BY table_name", conn);
            tableCmd.Parameters.AddWithValue("schema", schema);
            
            var tables = new List<string>();
            await using (var reader = await tableCmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    tables.Add(reader.GetString(0));
                }
            }

            foreach (var table in tables)
            {
                await ShowColumns(conn, $"{schema}.{table}");
            }
        }
    }
}
