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
Copy-Item src\Modules\Questions\DuelApp.Modules.Questions.Functions\local.settings.json.example src\Modules\Questions\DuelApp.Modules.Questions.Functions\local.settings.json
Set-Location src\Modules\Questions\DuelApp.Modules.Questions.Functions
func start
```

The Function listens on two queues in the local Service Bus emulator:

- `question-imports` receives BlobCreated events and imports validated JSON questions into the
  unpublished tables;
- `question-publications` receives publication commands and moves completed imports to the regular
  questions and answers tables in batches.

Verify that the emulator is available before starting the Function:

```powershell
Invoke-WebRequest http://127.0.0.1:5300/health
```

Use the corresponding connection string from `local.settings.json` when sending messages to either
queue. The publication queue accepts a serialized `PublishImportedQuestionsCommand`; normally this is
published by `POST /api/questions/imports/publish` in the Web API.

Azurite is used by the Functions host through `AzureWebJobsStorage`.

## Stop or reset

```powershell
docker compose -f local-dev/docker-compose.yml down
```

To remove the local PostgreSQL and Azurite data as well:

```powershell
docker compose -f local-dev/docker-compose.yml down -v
```
