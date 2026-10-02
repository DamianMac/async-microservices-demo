ALTER TABLE dbo.Orders ADD
    CustomerFirstName  nvarchar(100)  NOT NULL CONSTRAINT DF_Orders_CustomerFirstName DEFAULT '',
    CustomerLastName   nvarchar(100)  NOT NULL CONSTRAINT DF_Orders_CustomerLastName DEFAULT '',
    CompletedAt        datetime2      NULL;
