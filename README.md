CrossApp — наскрізний проєкт з крос-платформного програмування
Предметна область: Склад

Сутності:

Товар (Product)
Партія (StockBatch)
Склад (Warehouse)
Переміщення (Movement)

Призначення застосунку: Застосунок веде облік товарних залишків по партіях, фіксує рух товару між складами та дозволяє відстежувати історію переміщень.

## Інваріанти доменної моделі (Product)

- Id, Sku, Name — обов'язкові, не можуть бути порожніми (ArgumentException)
- Початковий залишок (Quantity) не може бути від'ємним (ArgumentOutOfRangeException)
- Кількість приходу/видачі має бути більшою за нуль (ArgumentOutOfRangeException)
- Не можна видати більше товару, ніж є в наявності (InvalidOperationException)

## Сервісний шар (лаб 5)

`ICatalogStore` — контракт сховища (Core/Abstractions)
`InMemoryCatalogStore` — реалізація в пам'яті`FileCatalogStore` — реалізація через JSON-файл (data/catalog.json)
`CatalogService` — бізнес-операції (Add, Receive), залежить лише від інтерфейсу

## Події (лаб 6)

- `CatalogService.Changed` — подія `EventHandler<CatalogChangedEventArgs>`, піднімається після успішної зміни (Added / Received / Issued / Removed)
- Core не пише в консоль і лог: підписники (консоль + `logs/app.log`) живуть у Cli
- `FindAll(Func<Product, bool>)` — пошук за довільним критерієм

## Відкладене виконання (лаб 7)

Запит `source.Where(p => p.Quantity < 10)` лише описує умову відбору, але не виконується в момент оголошення. Реальний прохід по колекції відбувається під час `Count()`, `ToList()` або `foreach`. Тому перший `Count()` дав 3 (до змін), а другий 4: між ними у джерело додали товар, і запит при повторному виконанні його врахував. Виклик `ToList()` зафіксував результат у знімку, тому подальше `source.Clear()` вже не змінило його (4 елементи).


## Асинхронний імпорт і тести (лаб 8)

- Імпорт: `dotnet run --project src/Cli -- import data/import.json`
- Скасування: додати `--cancel` (або Ctrl+C під час виконання) → «Імпорт скасовано.»
- Зіпсовані дані: `import data/import-bad.json` → «Дані відхилено: Запис 2: ...»
- Тести: `dotnet test`
- `DomainException` = порушення бізнес-правила; `ArgumentException` = поганий аргумент


## Конфігурація (лаб 9)

Порядок джерел (пізніше перекриває раніше): appsettings.json → appsettings.{Env}.json → user-secrets → змінні середовища → аргументи.

| Ключ | Змінна середовища | За замовчуванням |
|---|---|---|
| Storage:DataPath | CROSSAPP_Storage__DataPath або CROSSAPP_DATA_PATH | data |
| Storage:FileName | CROSSAPP_Storage__FileName | items.json |
| Logging:MinimumLevel | CROSSAPP_Logging__MinimumLevel | Information (Dev: Debug, Prod: Warning) |
| EnvironmentName | CROSSAPP_ENVIRONMENT вибирає профіль | Development |
| Integration:ApiKey | CROSSAPP_Integration__ApiKey | не в файлах: user-secrets або env |

PowerShell: `$env:CROSSAPP_DATA_PATH = "C:\temp\data"`
bash: `CROSSAPP_DATA_PATH=/tmp/data dotnet run --project src/Cli -- export`

Секрет: `dotnet user-secrets set "Integration:ApiKey" "..." --project src/Cli`
Команди: `export [шлях]`, `import [шлях]`, `report-top|low|groups|sum`; `--file` вмикає файлове сховище.

## HTTP API (лаб 10)

Запуск: `dotnet run --project src/Api` (порт 5080).
Scalar UI: http://localhost:5080/scalar/v1

| Метод | Шлях | Коди |
|---|---|---|
| GET | /api/products | 200 |
| GET | /api/products/{id} | 200, 404 |
| POST | /api/products | 201, 400 |
| PUT | /api/products/{id} | 200, 400, 404 |
| DELETE | /api/products/{id} | 204, 404 |

```bash
curl -i -X POST http://localhost:5080/api/products -H "Content-Type: application/json" \
  -d '{"sku":"A-100","name":"Цемент М400","unit":"мішок","quantity":40}'
```

## База даних (лаб 11)

SQLite, файл `src/Api/data/app.db` (у .gitignore). Рядок підключення: `ConnectionStrings:AppDb` в appsettings.json.

Міграції:
- створити: `dotnet ef migrations add <Назва> --project src/Infrastructure --startup-project src/Api`
- застосувати: `dotnet ef database update --project src/Infrastructure --startup-project src/Api`
- скинути БД: видалити `src/Api/data/app.db` і застосувати міграції знову

У Development міграції застосовуються автоматично при старті Api.
