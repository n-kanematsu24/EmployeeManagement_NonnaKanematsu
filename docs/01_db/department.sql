CREATE TABLE department (
    id SERIAL PRIMARY KEY,
    dept_name VARCHAR(50),
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP,
    updated_at TIMESTAMP,
    updated_id INTEGER
);

ALTER TABLE department ADD COLUMN IF NOT EXISTS phone VARCHAR(20);

SELECT * FROM public.department
ORDER BY id ASC 

CREATE TABLE department (
    id SERIAL PRIMARY KEY,
    dept_name VARCHAR(50),
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP,
    updated_at TIMESTAMP,
    updated_id INTEGER
);

ALTER TABLE department ADD COLUMN IF NOT EXISTS phone VARCHAR(20);
ALTER TABLE department ADD COLUMN IF NOT EXISTS dept_name_en VARCHAR(50);

SELECT * FROM public.department
ORDER BY id ASC 

CREATE TABLE department (
    id SERIAL PRIMARY KEY,
    dept_name VARCHAR(50),
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP,
    updated_at TIMESTAMP,
    updated_id INTEGER
);

ALTER TABLE department ADD COLUMN IF NOT EXISTS phone VARCHAR(20);

SELECT * FROM public.department
ORDER BY id ASC 