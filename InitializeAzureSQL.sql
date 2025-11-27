-- Initialize TuitionDB for Azure SQL Server

-- Create Students table
CREATE TABLE Students (
    StudentId INT IDENTITY(1,1) PRIMARY KEY,
    StudentNo NVARCHAR(10) NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    CONSTRAINT UQ_Students_StudentNo UNIQUE (StudentNo)
);

-- Create Tuitions table
CREATE TABLE Tuitions (
    TuitionId INT IDENTITY(1,1) PRIMARY KEY,
    StudentId INT NOT NULL,
    Term NVARCHAR(50) NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    Balance DECIMAL(18,2) NOT NULL,
    PaidAmount DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(20) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL,
    CONSTRAINT FK_Tuitions_Students FOREIGN KEY (StudentId) REFERENCES Students(StudentId) ON DELETE CASCADE
);

-- Create Payments table
CREATE TABLE Payments (
    PaymentId INT IDENTITY(1,1) PRIMARY KEY,
    TuitionId INT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    PaymentDate DATETIME2 NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    TransactionReference NVARCHAR(100) NOT NULL,
    CONSTRAINT FK_Payments_Tuitions FOREIGN KEY (TuitionId) REFERENCES Tuitions(TuitionId) ON DELETE CASCADE
);

-- Create RateLimits table
CREATE TABLE RateLimits (
    RateLimitId INT IDENTITY(1,1) PRIMARY KEY,
    StudentNo NVARCHAR(10) NOT NULL,
    Endpoint NVARCHAR(200) NOT NULL,
    CallCount INT NOT NULL,
    Date DATETIME2 NOT NULL,
    LastCall DATETIME2 NOT NULL
);

-- Create Users table
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL,
    PasswordHash NVARCHAR(200) NOT NULL,
    Role NVARCHAR(20) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    CONSTRAINT UQ_Users_Username UNIQUE (Username)
);

-- Create indexes
CREATE INDEX IX_Tuitions_StudentId_Term ON Tuitions(StudentId, Term);
CREATE INDEX IX_Payments_TuitionId ON Payments(TuitionId);
CREATE INDEX IX_RateLimits_StudentNo_Endpoint_Date ON RateLimits(StudentNo, Endpoint, Date);
CREATE INDEX IX_Students_StudentNo ON Students(StudentNo);
CREATE INDEX IX_Users_Username ON Users(Username);

-- Insert seed data for Students
INSERT INTO Students (StudentNo, Name, Email, CreatedAt) VALUES
('20210001', 'Ahmet Yılmaz', 'ahmet.yilmaz@university.edu', '2024-01-01 00:00:00'),
('20210002', 'Ayşe Demir', 'ayse.demir@university.edu', '2024-01-01 00:00:00'),
('20210003', 'Mehmet Kaya', 'mehmet.kaya@university.edu', '2024-01-01 00:00:00'),
('20210004', 'Fatma Öz', 'fatma.oz@university.edu', '2024-01-01 00:00:00'),
('20210005', 'Ali Şahin', 'ali.sahin@university.edu', '2024-01-01 00:00:00');

-- Insert seed data for Tuitions
INSERT INTO Tuitions (StudentId, Term, TotalAmount, Balance, PaidAmount, Status, CreatedAt, UpdatedAt) VALUES
(1, '2024-Fall', 50000, 50000, 0, 'UNPAID', '2024-01-01 00:00:00', '2024-01-01 00:00:00'),
(2, '2024-Fall', 50000, 25000, 25000, 'PARTIAL', '2024-01-01 00:00:00', '2024-01-01 00:00:00'),
(3, '2024-Fall', 50000, 0, 50000, 'PAID', '2024-01-01 00:00:00', '2024-01-01 00:00:00'),
(4, '2024-Fall', 50000, 50000, 0, 'UNPAID', '2024-01-01 00:00:00', '2024-01-01 00:00:00');

-- Insert seed data for Users
-- Password hashes are for: Admin123! and Bank123!
INSERT INTO Users (Username, PasswordHash, Role, CreatedAt) VALUES
('admin', '$2a$11$YourHashedPasswordHere', 'Admin', GETUTCDATE()),
('bankapi', '$2a$11$YourHashedPasswordHere', 'BankingSystem', GETUTCDATE());

-- Note: The password hashes above are placeholders.
-- The actual hashes will be created by the application on first run if users don't exist.
