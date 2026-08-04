# DuelApp Service Bus Inspector

The inspector lists queue statistics and peeks at messages without receiving or completing them.

Start the local Service Bus emulator before using it:

```powershell
docker compose -f local-dev/docker-compose.yml up -d mssql servicebus-emulator
```

Inspect the default `question-imports` queue:

```powershell
dotnet run --project tools/DuelApp.ServiceBusInspector
```

Inspect up to 25 messages from another queue:

```powershell
dotnet run --project tools/DuelApp.ServiceBusInspector -- --queue another-queue --count 25
```

Inspect the dead-letter queue:

```powershell
dotnet run --project tools/DuelApp.ServiceBusInspector -- --dead-letter
```

For a non-local Service Bus, provide both connection strings explicitly. The administration endpoint for the local emulator uses port `5300`; message peeking uses the default AMQP endpoint.
