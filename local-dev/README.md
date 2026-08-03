# Local development

## Start dependencies

1. From the repository root, start PostgreSQL, Keycloak, pgAdmin, Azurite and the Service Bus emulator:

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
| Service Bus emulator health endpoint | http://127.0.0.1:5300/health |
| Service Bus emulator AMQP endpoint | `localhost:5672` |

## Browse blobs with Azure Storage Explorer

Install [Azure Storage Explorer](https://azure.microsoft.com/products/storage/storage-explorer/) on the host machine.

## Run the question imports Function

Install Azure Functions Core Tools, copy the local configuration template and start the host:

```powershell
Copy-Item src\Functions\DuelApp.QuestionImports.Functions\local.settings.json.example src\Functions\DuelApp.QuestionImports.Functions\local.settings.json
Set-Location src\Functions\DuelApp.QuestionImports.Functions
func start
```

The Function listens on the `question-imports` queue in the local Service Bus emulator. It logs only
the message metadata and automatically completes a successfully handled message. It does not read
Blob Storage, call the database or perform an import.

Verify that the emulator is available before starting the Function:

```powershell
Invoke-WebRequest http://127.0.0.1:5300/health
```

Send a message to `question-imports` using any Service Bus SDK client configured with the
`QuestionImportsServiceBus` connection string from `local.settings.json`. The Functions host should
log the message ID, sequence number, delivery count and body length.

Azurite is used by the Functions host through `AzureWebJobsStorage`.

## Stop or reset

```powershell
docker compose -f local-dev/docker-compose.yml down
```

To remove the local PostgreSQL and Azurite data as well:

```powershell
docker compose -f local-dev/docker-compose.yml down -v
```
