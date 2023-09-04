CREATE DATABASE "BooksStore"
WITH
OWNER = postgres
ENCODING = 'UTF8'
CONNECTION LIMIT = -1
IS_TEMPLATE = False;

CREATE TABLE public."Books"
(
    "Id" integer NOT NULL,
    "Title" text NOT NULL,
    PRIMARY KEY ("Id")
);

ALTER TABLE IF EXISTS public."Books"
OWNER to postgres;
