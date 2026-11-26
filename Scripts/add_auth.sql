-- Практическая №11. Выполнить ОДИН РАЗ в SSMS в базе ShopDB1
-- (ПКМ по ShopDB1 -> «Создать запрос» -> F5).
-- Если ещё не выполняли скрипт add_stock_and_reservations.sql из прошлой работы — сначала выполните его.
--
--   1) Users.IsActive — флаг активности пользователя (вход разрешён только активным);
--   2) роли Admin, Manager и User, если их ещё нет (минимум две роли по заданию);
--   3) уникальность логина (как UNIQUE в методичке).
-- Первого администратора создаёт само приложение при старте (нужен хеш пароля) —
-- логин/пароль задаются в appsettings.json, секция "Seed".

USE ShopDB1;
GO

IF COL_LENGTH('dbo.Users', 'IsActive') IS NULL
BEGIN
    ALTER TABLE dbo.Users
        ADD IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT 1;
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = N'Admin')
    INSERT INTO dbo.Roles (RoleName, CreatedAt) VALUES (N'Admin', SYSUTCDATETIME());

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = N'Manager')
    INSERT INTO dbo.Roles (RoleName, CreatedAt) VALUES (N'Manager', SYSUTCDATETIME());

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = N'User')
    INSERT INTO dbo.Roles (RoleName, CreatedAt) VALUES (N'User', SYSUTCDATETIME());
GO

-- Логин должен быть уникальным. Если в таблице уже есть одинаковые логины — индекс не создаётся,
-- сначала удалите/переименуйте дубликаты и выполните этот блок ещё раз.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Users_Login' AND object_id = OBJECT_ID(N'dbo.Users'))
BEGIN
    IF EXISTS (SELECT Login FROM dbo.Users GROUP BY Login HAVING COUNT(*) > 1)
        PRINT N'В Users есть одинаковые логины — уникальный индекс не создан. Устраните дубликаты и повторите.';
    ELSE
        CREATE UNIQUE INDEX UX_Users_Login ON dbo.Users (Login);
END
GO

-- Проверка
SELECT Role_Id, RoleName FROM dbo.Roles;
SELECT User_Id, Login, IsActive FROM dbo.Users;
GO

-- ВАЖНО: пользователи, созданные раньше (PasswordHash = '123' и т.п.), хранят пароль открытым текстом —
-- войти под ними нельзя. Либо зарегистрируйте новых через /api/auth/register,
-- либо задайте пароль под админом: PATCH /api/users/{id}  { "password": "newpass123" }.
