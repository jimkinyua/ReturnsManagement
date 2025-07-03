-- Period Management Tables Creation Script
-- Run this script to create the frequency_catalog, reporting_years, and updated periods tables

-- 1. Create FrequencyCatalogs table
CREATE TABLE FrequencyCatalogs (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Code NVARCHAR(10) NOT NULL,
    Name NVARCHAR(50) NOT NULL,
    IntervalDays INT NOT NULL,
    DefaultDeadlineOffset INT NOT NULL,
    LabelStrategy NVARCHAR(20) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    CONSTRAINT UQ_FrequencyCatalogs_Code UNIQUE (Code)
);

-- Create index on IsActive
CREATE INDEX IX_FrequencyCatalogs_IsActive ON FrequencyCatalogs (IsActive);

-- 2. Create ReportingYears table
CREATE TABLE ReportingYears (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Year INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    CONSTRAINT UQ_ReportingYears_Year UNIQUE (Year)
);

-- 3. Backup existing Periods table data (if needed)
-- SELECT * INTO Periods_Backup FROM Periods;

-- 4. Drop existing foreign key constraints on Periods table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ReturnForms_Periods_PeriodId')
    ALTER TABLE ReturnForms DROP CONSTRAINT FK_ReturnForms_Periods_PeriodId;

-- 5. Drop and recreate Periods table with new structure
DROP TABLE IF EXISTS Periods;

CREATE TABLE Periods (
    Id NVARCHAR(450) PRIMARY KEY DEFAULT NEWID(),
    YearId INT NOT NULL,
    FrequencyId INT NOT NULL,
    SequenceNo INT NULL,
    Name NVARCHAR(50) NOT NULL,
    StartDate DATETIME2 NOT NULL,
    EndDate DATETIME2 NOT NULL,
    FilingDeadline DATETIME2 NOT NULL,
    IsLocked BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    CONSTRAINT FK_Periods_ReportingYears_YearId FOREIGN KEY (YearId) 
        REFERENCES ReportingYears(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Periods_FrequencyCatalogs_FrequencyId FOREIGN KEY (FrequencyId) 
        REFERENCES FrequencyCatalogs(Id) ON DELETE NO ACTION
);

-- Create indexes on Periods
CREATE UNIQUE INDEX IX_Periods_YearId_FrequencyId_SequenceNo 
    ON Periods (YearId, FrequencyId, SequenceNo) 
    WHERE SequenceNo IS NOT NULL;
CREATE INDEX IX_Periods_FrequencyId_StartDate ON Periods (FrequencyId, StartDate);
CREATE INDEX IX_Periods_YearId_Name ON Periods (YearId, Name);

-- 6. Recreate foreign key from ReturnForms to Periods
ALTER TABLE ReturnForms 
    ADD CONSTRAINT FK_ReturnForms_Periods_PeriodId 
    FOREIGN KEY (PeriodId) REFERENCES Periods(Id);

-- 7. Insert seed data for FrequencyCatalogs
INSERT INTO FrequencyCatalogs (Code, Name, IntervalDays, DefaultDeadlineOffset, LabelStrategy, IsActive, CreatedBy) VALUES
    ('DAY', 'Daily', 1, 1, 'DATE', 1, 'System'),
    ('WK', 'Weekly', 7, 3, 'ISO_WEEK', 1, 'System'),
    ('BWK', 'Bi-Weekly', 14, 5, 'BI_WEEK', 1, 'System'),
    ('MTH', 'Monthly', 30, 15, 'MONTH', 1, 'System'),
    ('QTR', 'Quarterly', 90, 30, 'QUARTER', 1, 'System'),
    ('SEMI', 'Semi-Annual', 180, 45, 'SEMI_ANNUAL', 1, 'System'),
    ('FY', 'Annual', 365, 60, 'YEAR', 1, 'System');

-- 8. Sample: Insert a reporting year (optional)
-- INSERT INTO ReportingYears (Year, IsActive, CreatedBy) VALUES (2025, 1, 'System');

PRINT 'Period Management tables created successfully!';