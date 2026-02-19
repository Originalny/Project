# ProductCrud — Каталог товаров

Веб-приложение с CRUD-функционалом для управления товарами интернет-магазина.

## Технологии

- **Язык**: C# (.NET 10)
- **Фреймворк**: ASP.NET Core MVC
- **База данных**: SQLite (Entity Framework Core)
- **UI**: Bootstrap 5, jQuery Validation
- **Тестирование**: xUnit, Moq
- **Логирование**: Serilog (консоль + файл)

## Архитектура

Приложение построено по шаблону **MVC** с выделенным сервисным слоем:

- `Models/` — модели данных (Product, ProductListViewModel)
- `Data/` — контекст базы данных (AppDbContext)
- `Services/` — бизнес-логика (IProductService / ProductService)
- `Controllers/` — обработка HTTP-запросов
- `Views/` — представления Razor

## Функционал

- **Список товаров** с пагинацией, поиском, фильтрацией по категории и сортировкой
- **Создание** товара с валидацией (название 3-50 символов, описание до 255, цена 0.01-999999.99)
- **Редактирование** существующего товара
- **Удаление** с подтверждением
- Автоматическое заполнение дат создания и обновления
- Seed-данные при первом запуске

## Структура сущности Product

| Поле | Тип | Описание |
|------|-----|----------|
| Id | GUID | Уникальный идентификатор |
| Name | string (3-50) | Название товара |
| Description | string? (до 255) | Описание |
| Price | decimal | Цена |
| Category | string (до 50) | Категория |
| CreatedAt | DateTime | Дата создания (автоматически) |
| UpdatedAt | DateTime | Дата обновления (автоматически) |

## Запуск

```bash
cd ProductCrud.Web
dotnet run
```

## Тестирование

```bash
cd ProductCrud.Tests
dotnet test
```

## Скриншоты работы
### Запуск
<img width="842" height="596" alt="image" src="https://github.com/user-attachments/assets/9f0e6dc3-9a5c-4b36-b8f0-3f3d1339f837" />

### Тесты
<img width="799" height="294" alt="image" src="https://github.com/user-attachments/assets/03c7309b-1737-4930-9a0c-6f5152ffc926" />

### Веб-страница
<img width="1345" height="710" alt="image" src="https://github.com/user-attachments/assets/2b8e6d9a-8dd4-41c2-aea7-ee8b223eff0a" />

### Добавление товара
<img width="755" height="633" alt="image" src="https://github.com/user-attachments/assets/08e8391e-11ce-49a6-a447-ebc0aa57d336" />
<img width="1348" height="822" alt="image" src="https://github.com/user-attachments/assets/e374e7fd-b9e3-470d-8b66-4edcd7f268d7" />

### Редактирование
<img width="719" height="637" alt="image" src="https://github.com/user-attachments/assets/76fc8552-044c-4950-81b6-40facae29407" />
<img width="1369" height="821" alt="image" src="https://github.com/user-attachments/assets/6785f90d-5363-4f5d-a398-51fdfde24cc2" />

### Удаление
<img width="1320" height="827" alt="image" src="https://github.com/user-attachments/assets/4f26b2f8-3a90-40b6-bc40-0d226897d905" />
<img width="1349" height="764" alt="image" src="https://github.com/user-attachments/assets/aa96d74e-2337-4e81-b13a-17fd5c766436" />

### Поиск по фильтрам
<img width="1347" height="575" alt="image" src="https://github.com/user-attachments/assets/a5a03dbd-f956-4259-9611-2ba772b708e3" />
