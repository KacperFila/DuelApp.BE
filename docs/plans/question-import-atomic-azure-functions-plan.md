# Plan: atomowy import pytań przez Azure Function

## Cel i decyzje architektoniczne

Import ma być uruchamiany przez osobną Azure Function z `ServiceBusTrigger` dla kolejki
`question-imports`. API pozostaje odpowiedzialne wyłącznie za przyjęcie pliku i utworzenie
rekordu importu.

Wymaganie „cały plik albo żaden” będzie spełnione przez **staging w PostgreSQL**. Pytania z
pojedynczych paczek nie trafią bezpośrednio do tabel `questions` i `answers`, więc nie będą
widoczne dla endpointu odczytu. Dopiero po poprawnym odczytaniu i zwalidowaniu całego pliku
jedna krótka transakcja przeniesie komplet danych do tabel produkcyjnych i oznaczy import jako
`Completed`.

Nie należy utrzymywać jednej transakcji podczas całego pobierania bloba. Taka transakcja
trzymałaby blokady i połączenie z bazą przez cały import. Transakcje paczek dotyczą wyłącznie
tabel stagingowych i mogą bezpiecznie pozostawać po błędzie, bo są niewidoczne dla aplikacji.

```text
Blob Storage ── BlobCreated/Event Grid ──> Service Bus ──> Azure Function
                                                          │
                                                          ├─ stream JSON i staging paczkami
                                                          │
                                                          └─ jedna transakcja publikacji
                                                               ├─ questions
                                                               ├─ answers
                                                               └─ QuestionImport = Completed
```

## Poza zakresem

- częściowa publikacja poprawnych pytań z wadliwego pliku;
- ładowanie treści pliku do pamięci w całości;
- przetwarzanie body Service Bus jako nośnika pliku;
- `BackgroundService` w API;
- automatyczne ponowne przetwarzanie wiadomości z DLQ (na początek wystarczy obserwacja i
  ręczne ponowienie po usunięciu przyczyny).

## Kontrakt danych i identyfikacja importu

1. Utrzymać JSON jako tablicę `GeneratedQuestion`. Każde pytanie i każda odpowiedź muszą
   przejść walidację przed dodaniem do stagingu; błąd walidacji oznacza błąd całego importu.
2. Z wiadomości Event Grid odczytywać wyłącznie referencję do bloba: jego ścieżkę, `eTag` i
   identyfikator zdarzenia. Nie logować treści ani pełnego adresu SAS.
3. Istniejący unikalny klucz `QuestionImport(BlobName, BlobETag)` traktować jako tożsamość
   niezmiennej wersji importu. Normalizować zapis `ETag`, aby wartość z API i Event Grid była
   porównywana w identycznej postaci.
4. Jeśli funkcja odbierze zdarzenie przed zapisaniem `QuestionImport` przez API, ma zgłosić
   błąd przejściowy. Wiadomość nie zostanie wtedy potwierdzona i Service Bus ponowi dostawę;
   nie wolno uznawać takiego zdarzenia za obsłużone.
5. Zmienić `IQuestionImportFileStorage.OpenReadAsync`, aby przyjmowało oczekiwany `ETag` i
   otwierało blob warunkowo (`If-Match`). Chroni to import przed odczytaniem innej wersji pliku.

## Model danych i atomowa widoczność

1. Dodać encje i konfiguracje EF dla `QuestionImportStagingQuestion` oraz
   `QuestionImportStagingAnswer`.
   - Każdy rekord zawiera `QuestionImportId`, pozycję w pliku oraz docelowe identyfikatory
     `QuestionId` i `AnswerId`.
   - Utworzyć unikalne indeksy `(QuestionImportId, SourcePosition)` dla pytań i
     `(QuestionImportId, QuestionSourcePosition, AnswerSourcePosition)` dla odpowiedzi.
   - Wartość pozycji jest stabilna dla wersji bloba, dlatego stanowi klucz idempotencji przy
     ponownym odczycie strumienia.
2. Rozszerzyć `QuestionImport` o dane operacyjne potrzebne do diagnostyki: co najmniej
   `LastAttemptAtUtc`, `AttemptCount` i `LastErrorMessage`. Pozostawić istniejące liczniki;
   `ProcessedQuestionsCount` oznacza liczbę pytań zapisanych w stagingu, a
   `TotalQuestionsCount` jest znany po zakończeniu odczytu.
3. Dodać repozytorium stagingu z jedną odpowiedzialnością: zapisać paczkę atomowo i pominąć
   rekordy już obecne dla tego importu. Nie używać ogólnego `BulkUploadAsync` do stagingu ani
   do publikacji.
4. Dodać repozytorium publikacji. W jednej transakcji ma ono:
   - zablokować rekord `QuestionImport` i potwierdzić, że status nie jest `Completed`;
   - wstawić wszystkie pytania i odpowiedzi z tabel stagingowych do produkcyjnych tabel;
   - ustawić status `Completed`, liczniki i `CompletedAtUtc`.

   Gdy transakcja się nie powiedzie, nie zostaną zapisane ani pytania, ani odpowiedzi, ani
   zmiana statusu. Gdy ponowiona funkcja wejdzie po udanym commicie, repozytorium rozpozna
   `Completed` i zakończy pracę bez duplikatów.
5. Pozostawiać staging nieudanego importu do czasu retencji. Usuwać go tylko w osobnym,
   idempotentnym procesie porządkowym po zdefiniowanym okresie retencji; nie usuwać go przed
   zakończeniem decyzji o statusie importu.

## Logika aplikacyjna

1. Dodać w warstwie Application `IQuestionImportProcessor` i
   `QuestionImportProcessor`. Funkcja wywołuje wyłącznie ten interfejs; nie zawiera logiki
   domenowej ani dostępu do EF.
2. Procesor wykonuje kolejno:
   - znajduje import po `(BlobName, BlobETag)`;
   - kończy bez zmian, jeśli import ma status `Completed`;
   - ustawia próbę i status `Processing` w krótkiej transakcji;
   - otwiera blob jako strumień oraz odczytuje tablicę JSON przez
     `JsonSerializer.DeserializeAsyncEnumerable`;
   - waliduje i zapisuje paczki o konfigurowalnym, niewielkim rozmiarze;
   - po odczycie całego pliku uruchamia repozytorium publikacji.
3. Po każdym commicie paczki czyścić tracking `DbContext`, by pamięć zależała od rozmiaru
   paczki, a nie liczby pytań w pliku. Nie stosować `DownloadContentAsync`, `ReadToEndAsync`
   ani `ToListAsync` na całym pliku.
4. Wyjątek przejściowy (błąd Storage, bazy, konflikt czasowy lub utrata blokady) zapisuje
   diagnostykę i jest propagowany do triggera. Wtedy Service Bus ponawia wiadomość, a staging
   oraz unikalne klucze pozwalają bezpiecznie wznowić import od początku strumienia.
5. Błąd formatu lub walidacji jest nieodwracalny: zapisać `Failed` z bezpiecznym opisem i
   zakończyć funkcję bez wyjątku. Taki plik nie będzie ponawiany i nigdy nie będzie widoczny w
   tabelach produkcyjnych.
6. Dla ostatniej dozwolonej próby dostawy zapisać status `Failed`, następnie zgłosić wyjątek,
   aby wiadomość trafiła do DLQ zgodnie z istniejącym `max_delivery_count = 5`.

## Azure Function

1. Utworzyć projekt `src/Functions/DuelApp.QuestionImports.Functions` jako .NET isolated
   worker, targetujący `net10.0`, i dodać go do `DuelApp.sln`.
   - Użyć `Microsoft.Azure.Functions.Worker`, `Worker.Sdk` i rozszerzenia
     `Microsoft.Azure.Functions.Worker.Extensions.ServiceBus` zgodnych z .NET 10.
   - W `Program.cs` zarejestrować wyłącznie procesor importu oraz istniejące implementacje
     infrastruktury pytań. Nie rejestrować modułów HTTP ani `QuestionsService`.
2. Dodać `ProcessQuestionImportFunction` z `ServiceBusTrigger` wskazującym kolejkę
   `question-imports` i konfigurację połączenia opartą o managed identity. Metoda funkcji ma
   być cienkim adapterem: parsuje zdarzenie Event Grid, tworzy referencję importu i przekazuje
   ją do `IQuestionImportProcessor`.
3. W `host.json` ustawić `prefetchCount` na `0`, początkowo `maxConcurrentCalls` na `1` oraz
   `maxAutoLockRenewalDuration` większe niż maksymalny dopuszczalny czas importu. Limit rozmiaru
   uploadu i czas wykonania funkcji muszą być ustalone razem, tak aby pojedynczy import mieścił
   się w tym oknie.
4. Pozostawić automatyczne potwierdzanie wiadomości. Funkcja kończy się sukcesem wyłącznie po
   `Completed`, importcie wcześniej ukończonym albo trwałym błędzie walidacji. W przypadku
   błędu przejściowego rzuca wyjątek; runtime nie potwierdzi wiadomości.

## Infrastruktura i wdrożenie

1. Utworzyć osobną Linux Azure Function App w planie Flex Consumption, z dedykowanym planem,
   kontenerem Storage na pakiety wdrożeniowe i Application Insights. To odseparuje skalowanie
   importów od API; dla `net10.0` na Linuxie nie należy używać klasycznego Consumption planu.
2. Obecny provider AzureRM `~> 3.0` nie obsługuje zasobu Flex Consumption. Zaktualizować go do
   wersji 4.x i przed wdrożeniem przejrzeć pełny `terraform plan`, ponieważ jest to zmiana
   głównej wersji providera. Alternatywą, jeśli aktualizacja musi zostać odłożona, jest użycie
   dostawcy AzAPI tylko dla Function App.
3. Przypisać Function App odrębną managed identity i minimalne role:
   - `Azure Service Bus Data Receiver` na kolejce `question-imports`;
   - `Storage Blob Data Reader` na storage importów;
   - `Key Vault Secrets User` dla connection stringa PostgreSQL.

   Nie używać identity API ani roli `Storage Blob Data Contributor`, ponieważ worker wyłącznie
   odczytuje pliki.
4. Przekazać do Function App ustawienia połączenia z Service Bus przez FQDN namespace oraz
   konfigurację `QuestionImports` (nazwa kolejki, rozmiar paczki i limit pliku). Connection
   string PostgreSQL przekazać przez Key Vault reference. Dodać konfigurację lokalną w
   `local.settings.json.example` i opis w `local-dev/README.md`; plik z sekretami nie trafia do
   repozytorium.
5. Rozszerzyć workflow wdrożeniowy o build i publikację projektu Function po wykonaniu
   migracji. API i Function mają osobne artefakty oraz osobne kroki wdrożenia; migracje muszą
   być wdrożone przed opublikowaniem funkcji.
6. Usunąć z API obecny `QuestionImportMessageListener`, rejestrację `ServiceBusClient` i jego
   ustawienia Terraform, ponieważ po przejściu na Function API nie może konsumować tej samej
   kolejki.

## Pliki objęte planem

- nowy katalog `src/Functions/DuelApp.QuestionImports.Functions/`;
- `DuelApp.sln`;
- `src/Modules/Questions/DuelApp.Modules.Questions.Application/` — procesor i kontrakty;
- `src/Modules/Questions/DuelApp.Modules.Questions.Domain/Questions/Entities/` — staging i
  rozszerzenie importu;
- `src/Modules/Questions/DuelApp.Modules.Questions.Infrastructure/EF/` — konfiguracje,
  repozytoria i migracja;
- `src/Modules/Questions/DuelApp.Modules.Questions.Infrastructure/Services/QuestionImportFileStorage.cs`;
- `src/Modules/Questions/DuelApp.Modules.Questions.Infrastructure/Extensions.cs` — usunięcie
  listenera API i rejestracje potrzebne workerowi;
- `infra/staging/provider.tf` oraz `infra/staging/main.tf`;
- `.github/workflows/staging-deploy.yml`;
- `local-dev/README.md`.

## Kryteria akceptacji

- Błąd dowolnej paczki nie powoduje pojawienia się żadnego pytania ani odpowiedzi z tego pliku
  w endpointach aplikacji.
- Po sukcesie wszystkie pytania i odpowiedzi z pliku stają się widoczne w tym samym commicie
  publikacji.
- Powtórzone zdarzenie Event Grid oraz ponowienie Service Bus nie tworzą duplikatów.
- Import przetwarza blob strumieniowo, a zużycie pamięci jest ograniczone przez rozmiar paczki.
- Błąd przejściowy powoduje ponowienie wiadomości; błąd walidacji kończy import jako `Failed`;
  po ostatniej nieudanej próbie wiadomość trafia do DLQ.
- API nie uruchamia listenera Service Bus ani nie wykonuje importu w swoim procesie.

## Weryfikacja wdrożeniowa

- Wykonać `terraform plan` po zmianie wersji providera i potwierdzić, że nie modyfikuje
  niepowiązanych zasobów.
- Wdrożyć migracje przed Function App, wysłać plik większy niż pojedyncza paczka i potwierdzić
  status `Completed` oraz pełną widoczność danych.
- Celowo przerwać import po co najmniej jednej paczce, potwierdzić brak widocznych pytań,
  następnie dopuścić ponowienie i potwierdzić pojedynczą publikację całego pliku.
