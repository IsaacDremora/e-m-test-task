ТЗ было выполнено с использованием .NET 8 minimal api, Postgres + entityframework (через миграции), Serilog, xUnit
В проекте e-m-test-task находится WebAPI со всеми необходимыми эндпоинтами
За создание сущности заказа используется post-запрос с эндпоинтом "/deliver-Order/{id}"
За фильтрацию заказов используется get-запрос с эндпоинтом "/delivery-Order/{districtId}"
Также добавлено логгирование основных операций через Serilog. Логгер сохраняет логи в таблицу в бд (logs), а также сохраняет в logs.txt

в проекте e-m-test-task в терминале dotnet ef database update
dotnet run

в проекте e-m-test-task.tests юнит тесты для web api. К сожалению, тесты дописать не успел.
