-- Seed data for testing
INSERT INTO "Users" ("Username", "PasswordHash", "Email", "Role", "CreatedAt", "IsActive")
VALUES
    ('admin', '$2a$11$exampleHash', 'admin@reasystem.com', 'Director', NOW(), true),
    ('viceprincipal', '$2a$11$exampleHash', 'vice@reasystem.com', 'VicePrincipal', NOW(), true);

-- Sample teachers
INSERT INTO "Teachers" ("FirstName", "LastName", "Email", "PhoneNumber", "Subject", "HireDate", "IsActive")
VALUES
    ('Maria', 'Gonzalez', 'maria.gonzalez@school.com', '+1234567890', 'Mathematics', NOW(), true),
    ('John', 'Smith', 'john.smith@school.com', '+1234567891', 'Science', NOW(), true);

-- Sample students
INSERT INTO "Students" ("FirstName", "LastName", "Email", "PhoneNumber", "DateOfBirth", "Grade", "Section", "EnrollmentDate", "IsActive")
VALUES
    ('Carlos', 'Rodriguez', 'carlos@student.com', '+1234567892', '2015-03-15', '5th', 'A', NOW(), true),
    ('Ana', 'Martinez', 'ana@student.com', '+1234567893', '2014-07-22', '6th', 'B', NOW(), true);