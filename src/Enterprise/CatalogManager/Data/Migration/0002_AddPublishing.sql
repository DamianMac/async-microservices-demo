ALTER TABLE dbo.Products ADD PublishedAt datetime2 NULL;

CREATE TABLE dbo.CatalogPublications (
    Id            int IDENTITY(1,1) NOT NULL CONSTRAINT PK_CatalogPublications PRIMARY KEY,
    PublishedAt   datetime2 NOT NULL,
    ProductCount  int       NOT NULL
);
