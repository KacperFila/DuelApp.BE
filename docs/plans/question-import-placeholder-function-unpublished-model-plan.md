# Plan: modele nieopublikowanych pytań i placeholder Azure Function

## Cel tego etapu

Przygotować podstawę pod przyszły, atomowy import pytań bez implementowania jego logiki.
Zakres obejmuje wyłącznie:

- trwały model danych dla pytań oczekujących na publikację;
- pusty, wdrażalny projekt Azure Functions;
- infrastrukturę potrzebną do uruchomienia tej Function App;
- pipeline wdrożeniowy dla nowego artefaktu.

Ten etap **nie odbiera wiadomości z Service Bus, nie pobiera blobów, nie parsuje JSON i nie
zapisuje pytań do docelowych tabel**.

## Nazewnictwo i granice modelu

Stosować nazwy `UnpublishedQuestion` i `UnpublishedAnswer` zamiast
`QuestionImportStagingQuestion` oraz `QuestionImportStagingAnswer`.

`Unpublished` opisuje znaczenie danych: są trwałe, należą do importu i nie są jeszcze dostępne
w głównym zbiorze pytań. Unika skojarzenia z obiektem nietrwałym (`Transient`) oraz z
edytowalnym szkicem (`Draft`).

Modele powinny być częścią modelu importu, odseparowaną od bieżącego agregatu `Question`:

```text
QuestionImport
 └── UnpublishedQuestion
       └── UnpublishedAnswer

Question / Answer
 └── wyłącznie dane opublikowane i widoczne w aplikacji
```

## Zmiany w modelu i bazie danych

1. Dodać encję `UnpublishedQuestion` w obszarze importów pytań.
   - `Id` — identyfikator, który będzie użyty przez przyszłą publikację jako identyfikator
     docelowego `Question`;
   - `QuestionImportId` — wymagane powiązanie z `QuestionImport`;
   - `SourcePosition` — pozycja pytania w niezmiennej wersji pliku;
   - `Title`;
   - kolekcja `UnpublishedAnswer`.
2. Dodać encję `UnpublishedAnswer`.
   - `Id` — identyfikator przewidziany dla przyszłego `Answer`;
   - `UnpublishedQuestionId` i nawigacja do `UnpublishedQuestion`;
   - `SourcePosition` — pozycja odpowiedzi w pytaniu;
   - `Content` i `IsCorrect`.
3. Dodać kolekcję `UnpublishedQuestions` do `QuestionImport`. Nie dodawać żadnej relacji z
   opublikowanymi `Question` ani `Answer`.
4. Dodać konfiguracje EF dla tabel:
   - `Questions.unpublished_questions`;
   - `Questions.unpublished_answers`.

   Relacje muszą być wymagane, a usunięcie `QuestionImport` ma kaskadowo usuwać jego
   nieopublikowane pytania i odpowiedzi. Nie stosować soft-delete w tym etapie.
5. Dodać indeksy i ograniczenia:
   - unikalny `(QuestionImportId, SourcePosition)` dla `UnpublishedQuestion`;
   - unikalny `(UnpublishedQuestionId, SourcePosition)` dla `UnpublishedAnswer`;
   - indeksy na kluczach obcych;
   - niepuste wartości tekstowe na poziomie modelu i mapowania EF.
6. Dodać `DbSet<UnpublishedQuestion>` i `DbSet<UnpublishedAnswer>` do `QuestionsDbContext`
   oraz wygenerować migrację EF.

W tym etapie nie dodawać repozytoriów, handlerów, mapowania z JSON ani kodu publikującego.
Tabele pozostają puste, dopóki kolejny etap nie wprowadzi procesora importu.

## Placeholder Azure Function

1. Utworzyć projekt `src/Functions/DuelApp.QuestionImports.Functions` jako .NET isolated
   worker z targetem `net10.0` i dodać go do `DuelApp.sln`.
2. Dodać jedynie podstawowe pliki hosta: projekt, `Program.cs`, `host.json`,
   `local.settings.json.example` oraz konfigurację logowania. Projekt ma uruchamiać host
   Functions i nie rejestrować warstwy Application ani Infrastructure pytań.
3. Nie dodawać jeszcze `ServiceBusTrigger`. W szczególności placeholder nie może odbierać i
   automatycznie potwierdzać komunikatów, ponieważ usunąłby zdarzenia zanim dostępna będzie
   implementacja importu.
4. Nie dodawać triggera HTTP ani timerowego wyłącznie po to, aby wywołać pusty kod. Wystarczy
   wdrażalna Function App bez funkcji biznesowych; kondycję środowiska zapewnią standardowe
   logi hosta i Application Insights.
5. W kolejnym etapie funkcja będzie zawierać cienki adapter `ServiceBusTrigger`, który wywoła
   procesor aplikacyjny. Ten kontrakt i procesor są celowo poza zakresem bieżącej zmiany.

## Infrastruktura Azure

1. Utworzyć w Terraform odrębną Linux Function App w planie Flex Consumption, z osobnym
   planem `FC1`, kontenerem Blob na pakiet wdrożeniowy i Application Insights połączonym z
   istniejącym Log Analytics Workspace.
2. Projekt bazowy używa `net10.0`. Linux Function App dla tej wersji ma działać w Flex
   Consumption, nie w klasycznym Consumption planie. Obecny AzureRM `~> 3.0` nie zawiera
   zasobu `azurerm_function_app_flex_consumption`; plan wymaga aktualizacji providera do 4.x
   oraz przeglądu pełnego `terraform plan` przed wdrożeniem.
3. Skonfigurować ustawienia wymagane przez host Functions:
   - `FUNCTIONS_WORKER_RUNTIME=dotnet-isolated`;
   - wersję runtime .NET zgodną z `net10.0`;
   - prywatny storage i kontener przeznaczone na deployment Function App;
   - telemetrykę Application Insights;
   - tagi `environment=staging`, `project=duelapp`, `component=question-imports`.
4. Nadać Function App własną system-assigned managed identity. Na tym etapie nie przyznawać
   uprawnień do Service Bus, Storage importów, PostgreSQL ani Key Vault, bo placeholder nie
   wykonuje operacji na danych. Te role zostaną dodane wraz z faktycznym triggerem i procesorem
   importu, zgodnie z zasadą najmniejszych uprawnień.
5. Nie modyfikować istniejącej kolejki, Event Grid ani listenera API w tym etapie. Usunięcie
   listenera API i przejęcie konsumpcji kolejki przez Function App nastąpi dopiero razem z
   pełnym procesorem importu, aby nie tworzyć okresu bez konsumenta.

## CI/CD i lokalne uruchamianie

1. Rozszerzyć workflow walidacji o restore i build projektu Functions oraz skonfigurować SDK
   .NET 10. Obecny workflow wskazuje .NET 9, więc należy go zaktualizować, aby budował cały
   solution konsekwentnie.
2. Rozszerzyć workflow staging o osobny artefakt Functions: publish projektu, spakowanie
   zawartości katalogu publish i wdrożenie do Function App po wykonaniu migracji.
3. Nie wiązać publikacji Function App z obrazem Docker API. To dwa niezależne artefakty i dwa
   niezależne kroki wdrożenia.
4. Opisać w `local-dev/README.md` wymagania uruchomienia Functions Core Tools oraz wskazać,
   że placeholder nie wymaga żadnych sekretów biznesowych ani emulatora Service Bus.

## Pliki objęte planem

- `DuelApp.sln`;
- nowy katalog `src/Functions/DuelApp.QuestionImports.Functions/`;
- `src/Modules/Questions/DuelApp.Modules.Questions.Domain/Questions/Entities/QuestionImport.cs`;
- nowe encje `UnpublishedQuestion` i `UnpublishedAnswer`;
- `src/Modules/Questions/DuelApp.Modules.Questions.Infrastructure/QuestionsDbContext.cs`;
- konfiguracje EF, migracja i snapshot w `Questions.Infrastructure/EF/`;
- `infra/staging/provider.tf` i `infra/staging/main.tf`;
- `.github/workflows/pr-validation.yml` i `.github/workflows/staging-deploy.yml`;
- `local-dev/README.md`.

## Kryteria akceptacji

- W kodzie ani planowanej migracji nie występują nazwy `QuestionImportStagingQuestion` ani
  `QuestionImportStagingAnswer`.
- Migracja tworzy tabele i wymagane ograniczenia dla `UnpublishedQuestion` oraz
  `UnpublishedAnswer` bez zmian w tabelach opublikowanych pytań.
- Endpointy API nadal zwracają wyłącznie rekordy z `questions` i `answers`; modele
  nieopublikowane nie mają jeszcze ścieżki publikacji.
- Function App jest wdrażalna i raportuje telemetrię, lecz nie ma triggera pobierającego
  wiadomości ani dostępu do danych importu.
- Istniejący odbiór Service Bus przez API pozostaje niezmieniony do czasu kolejnego etapu.
