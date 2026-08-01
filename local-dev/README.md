# Local development

## Start dependencies

1. From the repository root, start PostgreSQL, Keycloak, pgAdmin and Azurite:

```powershell
docker compose -f local-dev/docker-compose.yml up -d
```

2. Initialize the Blob Storage containers used by the API:

```powershell
.\local-dev\Initialize-Azurite.ps1
```

## Run the API

```powershell
dotnet run --project src\Bootstrapper\DuelApp.Bootstrapper --launch-profile DuelApp.Bootstrapper
```

The application runs in `Development`. Migrations are applied automatically, Azure storage uses Azurite emulation.

## Local endpoints

| Service | Address |
| --- | --- |
| API and Swagger | http://localhost:5000/swagger |
| PostgreSQL | localhost:5444 |
| Keycloak | http://localhost:8080 |
| pgAdmin | http://localhost:5050 |
| Azurite Blob Storage | http://127.0.0.1:10000 |

## Browse blobs with Azure Storage Explorer

Install [Azure Storage Explorer](https://azure.microsoft.com/products/storage/storage-explorer/) on the host machine.

```

## Stop or reset

```powershell
docker compose -f local-dev/docker-compose.yml down
```

To remove the local PostgreSQL and Azurite data as well:

```powershell
docker compose -f local-dev/docker-compose.yml down -v
```
