﻿CREATE TABLE IF NOT EXISTS public.items
(
    key character varying(50) COLLATE pg_catalog."default" NOT NULL UNIQUE,
    value TEXT NOT NULL,
    expiration_period int NOT NULL,
    expiration_date TIMESTAMP,

    CONSTRAINT pkey_items  PRIMARY KEY (key)
)
