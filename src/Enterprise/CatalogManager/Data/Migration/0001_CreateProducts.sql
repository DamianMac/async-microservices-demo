CREATE TABLE dbo.Products (
    Id                  int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Products PRIMARY KEY,
    Sku                 nvarchar(50)   NOT NULL,
    Name                nvarchar(200)  NOT NULL,
    Category            nvarchar(50)   NOT NULL,
    Description         nvarchar(1000) NOT NULL,
    ImageUrl            nvarchar(300)  NULL,
    Price               decimal(10,2)  NULL,
    Status              nvarchar(20)   NOT NULL,
    CoffeeOrigin        nvarchar(100)  NULL,
    CoffeeRegion        nvarchar(200)  NULL,
    CoffeeProcess       nvarchar(50)   NULL,
    CoffeeRoast         nvarchar(50)   NULL,
    CoffeeWeightGrams   int            NULL,
    CoffeeTastingNotes  nvarchar(500)  NULL,
    CreatedAt           datetime2      NOT NULL,
    UpdatedAt           datetime2      NOT NULL
);

CREATE UNIQUE INDEX IX_Products_Sku ON dbo.Products (Sku);
