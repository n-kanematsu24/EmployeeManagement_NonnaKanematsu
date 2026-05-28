SELECT * FROM public."__EFMigrationsHistory"
ORDER BY "MigrationId" ASC 

CREATE TABLE login (
    id SERIAL PRIMARY KEY,
    employee_no VARCHAR(10) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 動作確認用のテストアカウント（パスワードは「password123」相当）
-- ※実運用ではハッシュ化必須。今回は学習のため平文で挿入
INSERT INTO login (employee_no, password) VALUES ('EMP001', 'password123');

SELECT * FROM public."__EFMigrationsHistory"
ORDER BY "MigrationId" ASC 

CREATE TABLE login (
    id SERIAL PRIMARY KEY,
    employee_no VARCHAR(10) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 動作確認用のテストアカウント（パスワードは「password123」相当）
-- ※実運用ではハッシュ化必須。今回は学習のため平文で挿入
INSERT INTO login (employee_no, password) VALUES ('EMP001', 'password123');

SELECT * FROM public."__EFMigrationsHistory"
ORDER BY "MigrationId" ASC 

CREATE TABLE login (
    id SERIAL PRIMARY KEY,
    employee_no VARCHAR(10) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 動作確認用のテストアカウント（パスワードは「password123」相当）
-- ※実運用ではハッシュ化必須。今回は学習のため平文で挿入
INSERT INTO login (employee_no, password) VALUES ('EMP001', 'password123');