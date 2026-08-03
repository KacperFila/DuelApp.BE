# Plan: odczyt wiadomości z Azure Service Bus

## Cel

Aplikacja ma utrzymywać połączenie z kolejką Azure Service Bus `question-imports`
i odczytywać przychodzące wiadomości. Na tym etapie wiadomość nie uruchamia importu
pytań ani żadnej logiki biznesowej.

## Poza zakresem

Plan świadomie nie obejmuje:

- parsowania `EventGridSchema` ani walidacji body wiadomości;
- pobierania plików z Blob Storage;
- tworzenia lub aktualizacji rekordów w PostgreSQL;
- zmian encji, repozytoriów i migracji EF;
- idempotencji importu, retry procesu biznesowego i obsługi plików;
- zmian endpointu `POST /api/questions`.

## Implementacja

1. **Konfiguracja połączenia**
   - Dodać `QuestionImportsServiceBusOptions` w `Questions.Infrastructure` z:
     `FullyQualifiedNamespace`, opcjonalnym `ConnectionString` dla developmentu
     oraz `QueueName` domyślnie równym `question-imports`.
   - Walidować, że skonfigurowano dokładnie jeden sposób uwierzytelniania i że
     nazwa kolejki nie jest pusta.
   - W środowisku Azure tworzyć `ServiceBusClient` z `DefaultAzureCredential`.
     Lokalnie umożliwić przekazanie connection stringa przez User Secrets lub
     zmienne środowiskowe.

2. **Hosted service nasłuchujący kolejki**
   - Dodać `QuestionImportMessageListener : BackgroundService` w warstwie
     Infrastructure.
   - Utworzyć `ServiceBusProcessor` dla kolejki `question-imports` z
     `AutoCompleteMessages = false` i uruchomić go w `ExecuteAsync`.
   - Listener ma logować metadane każdej odebranej wiadomości: `MessageId`,
     `SequenceNumber`, `DeliveryCount` i długość body. Nie logować pełnej treści
     wiadomości.
   - Po udanym odczycie jawnie wywoływać `CompleteMessageAsync`, aby wiadomość
     nie była dostarczana ponownie. Nie przekazywać body dalej.
   - Dla błędów transportowych logować wyjątek i pozwolić Service Bus ponowić
     dostarczenie; listener nie wykonuje własnego retry ani nie wysyła wiadomości
     do DLQ.
   - W `StopAsync`/końcu `ExecuteAsync` zatrzymać processor i zwolnić zasoby.

3. **Rejestracja i infrastruktura środowiskowa**
   - Zarejestrować `ServiceBusClient` jako singleton, a listener jako hosted
     service tylko, jeśli konfiguracja Service Bus jest obecna.
   - W Terraform przekazać do aplikacji `FullyQualifiedNamespace` oraz nazwę
     kolejki. Zachować istniejące przypisanie roli `Azure Service Bus Data
     Receiver` dla managed identity aplikacji.
   - Uzupełnić `local-dev/README.md` o wymagane zmienne środowiskowe / User
     Secrets. Bez konfiguracji kolejki aplikacja musi uruchamiać się normalnie,
     lecz bez listenera.

## Pliki objęte planem

- `src/Modules/Questions/DuelApp.Modules.Questions.Infrastructure/Configuration/QuestionImportsServiceBusOptions.cs`
- `src/Modules/Questions/DuelApp.Modules.Questions.Infrastructure/Messaging/QuestionImportMessageListener.cs`
- `src/Modules/Questions/DuelApp.Modules.Questions.Infrastructure/Extensions.cs`
- `infra/staging/main.tf`
- `local-dev/README.md`

## Kryteria akceptacji

- Aplikacja z poprawną konfiguracją loguje uruchomienie listenera.
- Wiadomość z kolejki jest odebrana, jej metadane są zalogowane i wiadomość zostaje
  zakończona (`Complete`).
- Aplikacja nie pobiera bloba, nie parsuje body i nie modyfikuje bazy danych.
- Aplikacja bez konfiguracji Service Bus uruchamia się bez próby połączenia z Azure.
