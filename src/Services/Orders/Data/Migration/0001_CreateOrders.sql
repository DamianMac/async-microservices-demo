CREATE TABLE dbo.Products (
    Sku        nvarchar(50)  NOT NULL CONSTRAINT PK_Products PRIMARY KEY,
    Name       nvarchar(200) NOT NULL,
    Price      decimal(10,2) NOT NULL,
    Currency   nchar(3)      NOT NULL,
    UpdatedAt  datetime2     NOT NULL
);

CREATE TABLE dbo.Orders (
    Id                     uniqueidentifier NOT NULL CONSTRAINT PK_Orders PRIMARY KEY,
    CustomerId             int              NOT NULL,
    CustomerEmail          nvarchar(256)    NOT NULL,
    Status                 nvarchar(20)     NOT NULL,
    DeliveryRecipientName  nvarchar(200)    NOT NULL,
    DeliveryLine1          nvarchar(200)    NOT NULL,
    DeliveryLine2          nvarchar(200)    NULL,
    DeliverySuburb         nvarchar(100)    NOT NULL,
    DeliveryState          nvarchar(50)     NOT NULL,
    DeliveryPostcode       nvarchar(10)     NOT NULL,
    DeliveryCountry        nvarchar(100)    NOT NULL,
    DeliveryPhone          nvarchar(30)     NULL,
    PaymentCardId          int              NOT NULL,
    CardBrand              nvarchar(20)     NOT NULL,
    CardLast4              nchar(4)         NOT NULL,
    Subtotal               decimal(10,2)    NOT NULL,
    Shipping               decimal(10,2)    NOT NULL,
    Total                  decimal(10,2)    NOT NULL,
    Currency               nchar(3)         NOT NULL,
    PlacedAt               datetime2        NOT NULL,
    UpdatedAt              datetime2        NOT NULL
);

CREATE INDEX IX_Orders_CustomerId ON dbo.Orders (CustomerId);

CREATE TABLE dbo.OrderLines (
    Id         int IDENTITY(1,1) NOT NULL CONSTRAINT PK_OrderLines PRIMARY KEY,
    OrderId    uniqueidentifier  NOT NULL CONSTRAINT FK_OrderLines_Orders REFERENCES dbo.Orders (Id) ON DELETE CASCADE,
    Sku        nvarchar(50)      NOT NULL,
    Name       nvarchar(200)     NOT NULL,
    UnitPrice  decimal(10,2)     NOT NULL,
    Quantity   int               NOT NULL,
    LineTotal  decimal(10,2)     NOT NULL
);

CREATE INDEX IX_OrderLines_OrderId ON dbo.OrderLines (OrderId);
