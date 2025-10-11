-- Clear existing data (optional)
DELETE FROM "AcademicRecords";
DELETE FROM "Payments";
DELETE FROM "Students";
DELETE FROM "Teachers";
DELETE FROM "Users";

-- Insert sample users
INSERT INTO "Users" ("Username", "PasswordHash", "Email", "Role", "IsActive") VALUES
                                                                                  ('admin', '$2a$11$exampleHash', 'admin@reasystem.com', 'Director', true),
                                                                                  ('viceprincipal', '$2a$11$exampleHash', 'vice@reasystem.com', 'VicePrincipal', true);

-- Insert sample teachers
INSERT INTO "Teachers" ("FirstName", "LastName", "Email", "Subject", "IsActive") VALUES
                                                                                     ('Maria', 'Gonzalez', 'maria.gonzalez@school.com', 'Mathematics', true),
                                                                                     ('John', 'Smith', 'john.smith@school.com', 'Science', true),
                                                                                     ('Laura', 'Johnson', 'laura.johnson@school.com', 'English', true);

-- Insert sample students
INSERT INTO "Students" ("FirstName", "LastName", "Email", "DateOfBirth", "Grade", "Section", "IsActive") VALUES
                                                                                                             ('Carlos', 'Rodriguez', 'carlos@student.com', '2015-03-15', '5th', 'A', true),
                                                                                                             ('Ana', 'Martinez', 'ana@student.com', '2014-07-22', '6th', 'B', true),
                                                                                                             ('Luis', 'Garcia', 'luis@student.com', '2015-08-10', '5th', 'A', true),
                                                                                                             ('Sofia', 'Lopez', 'sofia@student.com', '2014-11-30', '6th', 'B', true);

-- Insert sample academic records
INSERT INTO "AcademicRecords" ("StudentId", "TeacherId", "Subject", "Grade", "Term", "SchoolYear", "Comments") VALUES
                                                                                                                   (1, 1, 'Mathematics', 85.5, '2024-Q1', 2024, 'Good performance'),
                                                                                                                   (2, 2, 'Science', 92.0, '2024-Q1', 2024, 'Excellent work'),
                                                                                                                   (3, 1, 'Mathematics', 78.0, '2024-Q1', 2024, 'Needs improvement'),
                                                                                                                   (4, 2, 'Science', 88.5, '2024-Q1', 2024, 'Very good');