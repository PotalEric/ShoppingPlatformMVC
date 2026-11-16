SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.Product', N'U') IS NULL
	THROW 50000, 'The Product table is missing. No cart tables were created.', 1;

IF OBJECT_ID(N'dbo.DemoCarts', N'U') IS NOT NULL OR OBJECT_ID(N'dbo.DemoCartItems', N'U') IS NOT NULL
	THROW 50001, 'A demo cart table already exists. Inspect the schema before applying this script.', 1;

CREATE TABLE dbo.DemoCarts
(
	Id uniqueidentifier NOT NULL CONSTRAINT PK_DemoCarts PRIMARY KEY,
	CreatedAtUtc datetime2 NOT NULL
);

CREATE TABLE dbo.DemoCartItems
(
	CartId uniqueidentifier NOT NULL,
	ProductId int NOT NULL,
	Quantity int NOT NULL,
	CONSTRAINT PK_DemoCartItems PRIMARY KEY (CartId, ProductId),
	CONSTRAINT FK_DemoCartItems_DemoCarts FOREIGN KEY (CartId) REFERENCES dbo.DemoCarts(Id) ON DELETE CASCADE,
	CONSTRAINT CK_DemoCartItems_Quantity CHECK (Quantity BETWEEN 1 AND 100)
);

COMMIT TRANSACTION;