CREATE TABLE products (
    sku            varchar(50)   NOT NULL CONSTRAINT pk_products PRIMARY KEY,
    name           varchar(200)  NOT NULL,
    category       varchar(50)   NOT NULL,
    description    varchar(1000) NOT NULL,
    origin         varchar(100)  NULL,
    region         varchar(200)  NULL,
    process        varchar(50)   NULL,
    roast          varchar(50)   NULL,
    tasting_notes  varchar(500)  NULL,
    -- Weighted so matches in the name rank above matches in the description.
    search_vector  tsvector GENERATED ALWAYS AS (
        setweight(to_tsvector('english', coalesce(name, '')), 'A') ||
        setweight(to_tsvector('english', coalesce(category, '') || ' ' || coalesce(origin, '') || ' ' || coalesce(region, '')), 'B') ||
        setweight(to_tsvector('english', coalesce(tasting_notes, '') || ' ' || coalesce(process, '') || ' ' || coalesce(roast, '')), 'C') ||
        setweight(to_tsvector('english', coalesce(description, '')), 'D')
    ) STORED
);

CREATE INDEX ix_products_search_vector ON products USING GIN (search_vector);
