-- Seed initial users with BCrypt hashed passwords
-- Password for all users: "Admin123!" (change in production!)

-- Delete existing users first (careful in production!)
DELETE FROM "Users";

-- Insert admin user
-- Username: admin, Password: Admin123!
INSERT INTO "Users" ("Username", "PasswordHash", "Email", "Role", "CreatedAt", "IsActive")
VALUES (
    'admin',
    '$2a$11$8kYZ5oJZ6ZJZYzJZY5ZJZ.ZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZu',
    'admin@reasystem.com',
    'Director',
    NOW(),
    true
);

-- Insert vice principal
-- Username: viceprincipal, Password: Admin123!
INSERT INTO "Users" ("Username", "PasswordHash", "Email", "Role", "CreatedAt", "IsActive")
VALUES (
    'viceprincipal',
    '$2a$11$8kYZ5oJZ6ZJZYzJZY5ZJZ.ZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZu',
    'vice@reasystem.com',
    'VicePrincipal',
    NOW(),
    true
);

-- Insert a sample teacher
-- Username: teacher, Password: Admin123!
INSERT INTO "Users" ("Username", "PasswordHash", "Email", "Role", "CreatedAt", "IsActive")
VALUES (
    'teacher',
    '$2a$11$8kYZ5oJZ6ZJZYzJZY5ZJZ.ZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZu',
    'teacher@reasystem.com',
    'Teacher',
    NOW(),
    true
);

-- Note: The password hash above is a placeholder. 
-- After first deployment, run a migration script to set proper passwords.
-- Or better yet, create a setup endpoint that allows the first admin to set their password.