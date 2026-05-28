CREATE TABLE employee (
    id SERIAL PRIMARY KEY,
    employee_no VARCHAR(10),
    last_name VARCHAR(30),
    first_name VARCHAR(30),
    birth_date DATE,
    phone VARCHAR(20),
    email VARCHAR(100),
    hire_date DATE,
    dept_id INTEGER REFERENCES department(id),
    status INTEGER,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP,
    updated_at TIMESTAMP,
    updated_id INTEGER
);

ALTER TABLE employee ADD COLUMN IF NOT EXISTS image_path VARCHAR(255);

SELECT * FROM public.employee
ORDER BY id ASC 

CREATE TABLE employee (
    id SERIAL PRIMARY KEY,
    employee_no VARCHAR(10),
    last_name VARCHAR(30),
    first_name VARCHAR(30),
    birth_date DATE,
    phone VARCHAR(20),
    email VARCHAR(100),
    hire_date DATE,
    dept_id INTEGER REFERENCES department(id),
    status INTEGER,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP,
    updated_at TIMESTAMP,
    updated_id INTEGER
);

ALTER TABLE employee ADD COLUMN IF NOT EXISTS image_path VARCHAR(255);

ALTER TABLE employee ADD COLUMN IF NOT EXISTS last_name_en VARCHAR(50);
ALTER TABLE employee ADD COLUMN IF NOT EXISTS first_name_en VARCHAR(50);

SELECT * FROM public.employee
ORDER BY id ASC 

CREATE TABLE employee (
    id SERIAL PRIMARY KEY,
    employee_no VARCHAR(10),
    last_name VARCHAR(30),
    first_name VARCHAR(30),
    birth_date DATE,
    phone VARCHAR(20),
    email VARCHAR(100),
    hire_date DATE,
    dept_id INTEGER REFERENCES department(id),
    status INTEGER,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP,
    updated_at TIMESTAMP,
    updated_id INTEGER
);

ALTER TABLE employee ADD COLUMN IF NOT EXISTS image_path VARCHAR(255);
ALTER TABLE employee ADD COLUMN IF NOT EXISTS last_name_en VARCHAR(50);
ALTER TABLE employee ADD COLUMN IF NOT EXISTS first_name_en VARCHAR(50);
ALTER TABLE department ADD COLUMN IF NOT EXISTS dept_name_en VARCHAR(50);

SELECT * FROM public.employee
ORDER BY id ASC 