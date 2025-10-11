-- Create Students table
CREATE TABLE IF NOT EXISTS "Students" (
                                          "Id" SERIAL PRIMARY KEY,
                                          "FirstName" VARCHAR(100) NOT NULL,
    "LastName" VARCHAR(100) NOT NULL,
    "Email" VARCHAR(100),
    "PhoneNumber" VARCHAR(20),
    "DateOfBirth" TIMESTAMP NOT NULL,
    "Grade" VARCHAR(10) NOT NULL,
    "Section" VARCHAR(50),
    "EnrollmentDate" TIMESTAMP NOT NULL DEFAULT NOW(),
    "IsActive" BOOLEAN NOT NULL DEFAULT true
    );

-- Create Teachers table
CREATE TABLE IF NOT EXISTS "Teachers" (
                                          "Id" SERIAL PRIMARY KEY,
                                          "FirstName" VARCHAR(100) NOT NULL,
    "LastName" VARCHAR(100) NOT NULL,
    "Email" VARCHAR(100),
    "PhoneNumber" VARCHAR(20),
    "Subject" VARCHAR(50) NOT NULL,
    "HireDate" TIMESTAMP NOT NULL DEFAULT NOW(),
    "IsActive" BOOLEAN NOT NULL DEFAULT true
    );

-- Create AcademicRecords table
CREATE TABLE IF NOT EXISTS "AcademicRecords" (
                                                 "Id" SERIAL PRIMARY KEY,
                                                 "StudentId" INTEGER NOT NULL REFERENCES "Students"("Id"),
    "TeacherId" INTEGER NOT NULL REFERENCES "Teachers"("Id"),
    "Subject" VARCHAR(100) NOT NULL,
    "Grade" DECIMAL(4,2) NOT NULL CHECK ("Grade" >= 0 AND "Grade" <= 100),
    "Term" VARCHAR(10) NOT NULL,
    "SchoolYear" INTEGER NOT NULL,
    "RecordDate" TIMESTAMP NOT NULL DEFAULT NOW(),
    "Comments" VARCHAR(500)
    );

-- Create Payments table
CREATE TABLE IF NOT EXISTS "Payments" (
                                          "Id" SERIAL PRIMARY KEY,
                                          "StudentId" INTEGER NOT NULL REFERENCES "Students"("Id"),
    "Amount" DECIMAL(10,2) NOT NULL,
    "PaymentType" VARCHAR(50) NOT NULL,
    "PaymentDate" TIMESTAMP NOT NULL,
    "Status" VARCHAR(20) NOT NULL DEFAULT 'Pending',
    "Description" VARCHAR(500),
    "DueDate" TIMESTAMP NOT NULL
    );

-- Create Users table
CREATE TABLE IF NOT EXISTS "Users" (
                                       "Id" SERIAL PRIMARY KEY,
                                       "Username" VARCHAR(100) NOT NULL UNIQUE,
    "PasswordHash" VARCHAR(255) NOT NULL,
    "Email" VARCHAR(100) NOT NULL UNIQUE,
    "Role" VARCHAR(20) NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    "LastLogin" TIMESTAMP,
    "IsActive" BOOLEAN NOT NULL DEFAULT true
    );

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS "IX_Students_Grade_Section" ON "Students" ("Grade", "Section");
CREATE INDEX IF NOT EXISTS "IX_AcademicRecords_StudentId_Subject_Term" ON "AcademicRecords" ("StudentId", "Subject", "Term");
CREATE INDEX IF NOT EXISTS "IX_AcademicRecords_TeacherId_Term" ON "AcademicRecords" ("TeacherId", "Term");
CREATE INDEX IF NOT EXISTS "IX_Payments_StudentId_Status" ON "Payments" ("StudentId", "Status");