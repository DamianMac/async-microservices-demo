CREATE TABLE dbo.Customers (
    Id            int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Customers PRIMARY KEY,
    Email         nvarchar(256) NOT NULL,
    PasswordHash  nvarchar(200) NOT NULL,
    FirstName     nvarchar(100) NOT NULL,
    LastName      nvarchar(100) NOT NULL,
    CreatedAt     datetime2     NOT NULL,
    UpdatedAt     datetime2     NOT NULL
);

CREATE UNIQUE INDEX IX_Customers_Email ON dbo.Customers (Email);

CREATE TABLE dbo.DeliveryAddresses (
    Id             int IDENTITY(1,1) NOT NULL CONSTRAINT PK_DeliveryAddresses PRIMARY KEY,
    CustomerId     int           NOT NULL CONSTRAINT FK_DeliveryAddresses_Customers REFERENCES dbo.Customers (Id) ON DELETE CASCADE,
    Label          nvarchar(50)  NOT NULL,
    RecipientName  nvarchar(200) NOT NULL,
    Line1          nvarchar(200) NOT NULL,
    Line2          nvarchar(200) NULL,
    Suburb         nvarchar(100) NOT NULL,
    State          nvarchar(50)  NOT NULL,
    Postcode       nvarchar(10)  NOT NULL,
    Country        nvarchar(100) NOT NULL,
    Phone          nvarchar(30)  NULL,
    IsDefault      bit           NOT NULL,
    CreatedAt      datetime2     NOT NULL,
    UpdatedAt      datetime2     NOT NULL
);

CREATE INDEX IX_DeliveryAddresses_CustomerId ON dbo.DeliveryAddresses (CustomerId);

CREATE TABLE dbo.PaymentCards (
    Id              int IDENTITY(1,1) NOT NULL CONSTRAINT PK_PaymentCards PRIMARY KEY,
    CustomerId      int           NOT NULL CONSTRAINT FK_PaymentCards_Customers REFERENCES dbo.Customers (Id) ON DELETE CASCADE,
    CardholderName  nvarchar(200) NOT NULL,
    Brand           nvarchar(20)  NOT NULL,
    Last4           nchar(4)      NOT NULL,
    ExpiryMonth     int           NOT NULL,
    ExpiryYear      int           NOT NULL,
    IsDefault       bit           NOT NULL,
    CreatedAt       datetime2     NOT NULL
);

CREATE INDEX IX_PaymentCards_CustomerId ON dbo.PaymentCards (CustomerId);
