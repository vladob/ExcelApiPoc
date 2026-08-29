# RegisterUZ single-IČO loader

Configure the database connection without committing credentials:

```powershell
dotnet user-secrets --project .\RegisterUz.Loader set `
  "ConnectionStrings:RegisterUZ" `
  "Server=SRVHPV;Database=RegisterUZ;User ID=...;Password=...;TrustServerCertificate=True"
```

Then run:

```powershell
dotnet run --project .\RegisterUz.Loader -- 00325554
```

The loader retrieves one accounting entity and follows the child identifiers
declared by its details. It validates all parent links and structured table
dimensions before saving the complete package in one SQL transaction.

No attachment binaries are downloaded. Attachment metadata is persisted.
