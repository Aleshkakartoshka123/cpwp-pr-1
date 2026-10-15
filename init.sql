CREATE TABLE IF NOT EXISTS categories (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS cars (
    id SERIAL PRIMARY KEY,
    brand VARCHAR(100) NOT NULL,
    model VARCHAR(100) NOT NULL,
    category_id INT REFERENCES categories(id) ON DELETE SET NULL,
    year INT NOT NULL,
    color VARCHAR(50),
    vin VARCHAR(50) UNIQUE,
    quantity INT NOT NULL DEFAULT 0,
    price NUMERIC(12,2) NOT NULL DEFAULT 0,
    min_quantity INT NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS operations (
    id SERIAL PRIMARY KEY,
    car_id INT NOT NULL REFERENCES cars(id) ON DELETE CASCADE,
    type VARCHAR(10) NOT NULL CHECK (type IN ('in','out')),
    quantity INT NOT NULL CHECK (quantity > 0),
    client_name VARCHAR(150),
    amount NUMERIC(14,2) NOT NULL DEFAULT 0,
    date TIMESTAMP NOT NULL DEFAULT NOW()
);

INSERT INTO categories (name) VALUES
    ('Седан'),
    ('Внедорожник'),
    ('Хэтчбек'),
    ('Купе'),
    ('Пикап')
ON CONFLICT DO NOTHING;