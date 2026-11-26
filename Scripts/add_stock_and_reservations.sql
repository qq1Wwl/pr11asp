-- Выполнить ОДИН РАЗ в SSMS в базе ShopDB1.
-- Существующие таблицы и данные не трогаем — только добавляем то, что нужно новой бизнес-операции:
--   1) Products.Stock        — сколько единиц товара свободно (его уменьшает резервирование);
--   2) Reservations          — новая сущность «факт резервирования» (кто / что / когда).

USE ShopDB1;
GO

IF COL_LENGTH('dbo.Products', 'Stock') IS NULL
BEGIN
    ALTER TABLE dbo.Products
        ADD Stock INT NOT NULL CONSTRAINT DF_Products_Stock DEFAULT 0;
END
GO

IF OBJECT_ID(N'dbo.Reservations', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Reservations
    (
        Reservation_Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Reservations PRIMARY KEY,
        User_Id        INT       NOT NULL,
        Product_Id     INT       NOT NULL,
        Quantity       INT       NOT NULL,
        ReservedAt     DATETIME2 NOT NULL,
        ReleasedAt     DATETIME2 NULL,
        CONSTRAINT FK_Reservations_Users    FOREIGN KEY (User_Id)    REFERENCES dbo.Users (User_Id),
        CONSTRAINT FK_Reservations_Products FOREIGN KEY (Product_Id) REFERENCES dbo.Products (Product_Id),
        CONSTRAINT CK_Reservations_Quantity CHECK (Quantity > 0)
    );

    CREATE INDEX IX_Reservations_Product_Id ON dbo.Reservations (Product_Id);
    CREATE INDEX IX_Reservations_User_Id    ON dbo.Reservations (User_Id);
END
GO

-- По желанию, для проверки: выставить остаток у уже существующих товаров
-- UPDATE dbo.Products SET Stock = 10 WHERE Stock = 0;
-- GO
