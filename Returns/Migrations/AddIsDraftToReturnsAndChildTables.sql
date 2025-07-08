-- Migration: Add IsDraft column to Returns and all child tables
-- Purpose: Support draft/submit workflow

-- Add IsDraft to Returns table
IF NOT EXISTS (SELECT * FROM sys.columns 
              WHERE object_id = OBJECT_ID(N'[dbo].[Returns]') 
              AND name = 'IsDraft')
BEGIN
    ALTER TABLE [dbo].[Returns] 
    ADD [IsDraft] BIT NOT NULL DEFAULT 0;
END
GO

-- Add IsDraft to DT tables
IF NOT EXISTS (SELECT * FROM sys.columns 
              WHERE object_id = OBJECT_ID(N'[dbo].[DTCapitalAdequacyReturns]') 
              AND name = 'IsDraft')
BEGIN
    ALTER TABLE [dbo].[DTCapitalAdequacyReturns] 
    ADD [IsDraft] BIT NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns 
              WHERE object_id = OBJECT_ID(N'[dbo].[DTLiquidityReturns]') 
              AND name = 'IsDraft')
BEGIN
    ALTER TABLE [dbo].[DTLiquidityReturns] 
    ADD [IsDraft] BIT NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns 
              WHERE object_id = OBJECT_ID(N'[dbo].[DTRiskClassificationReturns]') 
              AND name = 'IsDraft')
BEGIN
    ALTER TABLE [dbo].[DTRiskClassificationReturns] 
    ADD [IsDraft] BIT NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns 
              WHERE object_id = OBJECT_ID(N'[dbo].[DTInvestmentReturns]') 
              AND name = 'IsDraft')
BEGIN
    ALTER TABLE [dbo].[DTInvestmentReturns] 
    ADD [IsDraft] BIT NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns 
              WHERE object_id = OBJECT_ID(N'[dbo].[DTFinancialPositionReturns]') 
              AND name = 'IsDraft')
BEGIN
    ALTER TABLE [dbo].[DTFinancialPositionReturns] 
    ADD [IsDraft] BIT NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns 
              WHERE object_id = OBJECT_ID(N'[dbo].[DTComprehensiveIncomeReturns]') 
              AND name = 'IsDraft')
BEGIN
    ALTER TABLE [dbo].[DTComprehensiveIncomeReturns] 
    ADD [IsDraft] BIT NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns 
              WHERE object_id = OBJECT_ID(N'[dbo].[DepositReturns]') 
              AND name = 'IsDraft')
BEGIN
    ALTER TABLE [dbo].[DepositReturns] 
    ADD [IsDraft] BIT NOT NULL DEFAULT 0;
END
GO

-- Add IsDraft to NWDT tables
IF NOT EXISTS (SELECT * FROM sys.columns 
              WHERE object_id = OBJECT_ID(N'[dbo].[NWDTCapitalAdequacyReturns]') 
              AND name = 'IsDraft')
BEGIN
    ALTER TABLE [dbo].[NWDTCapitalAdequacyReturns] 
    ADD [IsDraft] BIT NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns 
              WHERE object_id = OBJECT_ID(N'[dbo].[NWDTLiquidityReturns]') 
              AND name = 'IsDraft')
BEGIN
    ALTER TABLE [dbo].[NWDTLiquidityReturns] 
    ADD [IsDraft] BIT NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns 
              WHERE object_id = OBJECT_ID(N'[dbo].[NWDTDepositReturns]') 
              AND name = 'IsDraft')
BEGIN
    ALTER TABLE [dbo].[NWDTDepositReturns] 
    ADD [IsDraft] BIT NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns 
              WHERE object_id = OBJECT_ID(N'[dbo].[NWDTRiskClassificationReturns]') 
              AND name = 'IsDraft')
BEGIN
    ALTER TABLE [dbo].[NWDTRiskClassificationReturns] 
    ADD [IsDraft] BIT NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns 
              WHERE object_id = OBJECT_ID(N'[dbo].[NWDTInvestmentReturns]') 
              AND name = 'IsDraft')
BEGIN
    ALTER TABLE [dbo].[NWDTInvestmentReturns] 
    ADD [IsDraft] BIT NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns 
              WHERE object_id = OBJECT_ID(N'[dbo].[NWDTFinancialPositionReturns]') 
              AND name = 'IsDraft')
BEGIN
    ALTER TABLE [dbo].[NWDTFinancialPositionReturns] 
    ADD [IsDraft] BIT NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns 
              WHERE object_id = OBJECT_ID(N'[dbo].[NWDTComprehensiveIncomeReturns]') 
              AND name = 'IsDraft')
BEGIN
    ALTER TABLE [dbo].[NWDTComprehensiveIncomeReturns] 
    ADD [IsDraft] BIT NOT NULL DEFAULT 0;
END
GO

-- Add IsDraft to other return tables
IF NOT EXISTS (SELECT * FROM sys.columns 
              WHERE object_id = OBJECT_ID(N'[dbo].[OtherReturns]') 
              AND name = 'IsDraft')
BEGIN
    ALTER TABLE [dbo].[OtherReturns] 
    ADD [IsDraft] BIT NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns 
              WHERE object_id = OBJECT_ID(N'[dbo].[SectoralLendingReports]') 
              AND name = 'IsDraft')
BEGIN
    ALTER TABLE [dbo].[SectoralLendingReports] 
    ADD [IsDraft] BIT NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns 
              WHERE object_id = OBJECT_ID(N'[dbo].[DailyLiquidityReturns]') 
              AND name = 'IsDraft')
BEGIN
    ALTER TABLE [dbo].[DailyLiquidityReturns] 
    ADD [IsDraft] BIT NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns 
              WHERE object_id = OBJECT_ID(N'[dbo].[InsiderLendingHeaders]') 
              AND name = 'IsDraft')
BEGIN
    ALTER TABLE [dbo].[InsiderLendingHeaders] 
    ADD [IsDraft] BIT NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns 
              WHERE object_id = OBJECT_ID(N'[dbo].[ManagementReturns]') 
              AND name = 'IsDraft')
BEGIN
    ALTER TABLE [dbo].[ManagementReturns] 
    ADD [IsDraft] BIT NOT NULL DEFAULT 0;
END
GO

-- Create indexes for better performance when querying drafts
CREATE NONCLUSTERED INDEX IX_Returns_IsDraft 
ON [dbo].[Returns] ([IsDraft], [SaccoId], [Period])
WHERE [IsDraft] = 1;
GO

CREATE NONCLUSTERED INDEX IX_DTCapitalAdequacyReturns_IsDraft 
ON [dbo].[DTCapitalAdequacyReturns] ([IsDraft], [ReturnSubmissionId])
WHERE [IsDraft] = 1;
GO

-- Add similar indexes for other tables as needed

-- Update existing records to mark them as non-drafts (already submitted)
UPDATE [dbo].[Returns] SET [IsDraft] = 0 WHERE [IsDraft] IS NULL;
UPDATE [dbo].[DTCapitalAdequacyReturns] SET [IsDraft] = 0 WHERE [IsDraft] IS NULL;
UPDATE [dbo].[DTLiquidityReturns] SET [IsDraft] = 0 WHERE [IsDraft] IS NULL;
-- Continue for all tables...

PRINT 'Migration completed: IsDraft column added to all return tables';