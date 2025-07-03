# Period Management System Setup

## Overview

The Period Management System provides a flexible framework for managing reporting periods with different frequencies (Daily, Weekly, Monthly, Quarterly, Semi-Annual, Annual). This system consists of three core tables:

1. **FrequencyCatalogs** - Defines available reporting frequencies
2. **ReportingYears** - Manages which years are available for reporting
3. **Periods** - Contains the actual reporting periods generated based on year and frequency

## Database Setup

### Option 1: Using Entity Framework Migrations (Requires .NET SDK)

```bash
cd /workspace/Returns
dotnet ef migrations add AddPeriodManagementTables
dotnet ef database update
```

### Option 2: Manual SQL Script

Run the SQL script located at: `/workspace/Returns/Migrations/SQL/PeriodManagementTables.sql`

This script will:
- Create the FrequencyCatalogs table
- Create the ReportingYears table
- Update the Periods table structure
- Seed the FrequencyCatalogs with default values
- Maintain existing foreign key relationships

## API Endpoints

### Reporting Years Management

#### Get All Years
```
GET /api/ReportingYears
```
Returns all reporting years with their period counts.

#### Get Specific Year
```
GET /api/ReportingYears/{id}
```
Returns details of a specific year.

#### Create New Year
```
POST /api/ReportingYears
Content-Type: application/json

{
  "year": 2025,
  "isActive": true
}
```
Creates a new reporting year. The year must be unique.

#### Toggle Year Active Status
```
PUT /api/ReportingYears/{id}/toggle-active
```
Toggles the active status of a year.

#### Delete Year
```
DELETE /api/ReportingYears/{id}
```
Deletes a year (only if it has no periods).

### Frequency Catalog Management

#### Get Active Frequencies
```
GET /api/FrequencyCatalog
```
Returns all active frequency catalogs.

#### Get All Frequencies (Including Inactive)
```
GET /api/FrequencyCatalog/all
```
Returns all frequency catalogs.

#### Get Frequency by ID
```
GET /api/FrequencyCatalog/{id}
```

#### Get Frequency by Code
```
GET /api/FrequencyCatalog/by-code/{code}
```
Example: `/api/FrequencyCatalog/by-code/MTH` for Monthly

#### Toggle Frequency Active Status
```
PUT /api/FrequencyCatalog/{id}/toggle-active
```

## Frequency Catalog Reference

| Code | Name        | Interval Days | Deadline Offset | Label Strategy |
|------|-------------|---------------|-----------------|----------------|
| DAY  | Daily       | 1             | 1               | DATE           |
| WK   | Weekly      | 7             | 3               | ISO_WEEK       |
| BWK  | Bi-Weekly   | 14            | 5               | BI_WEEK        |
| MTH  | Monthly     | 30            | 15              | MONTH          |
| QTR  | Quarterly   | 90            | 30              | QUARTER        |
| SEMI | Semi-Annual | 180           | 45              | SEMI_ANNUAL    |
| FY   | Annual      | 365           | 60              | YEAR           |

## Usage Example

### Creating a New Reporting Year

1. First, ensure the frequency catalogs are seeded (this happens automatically on application startup).

2. Create a new reporting year:
```bash
curl -X POST https://your-api/api/ReportingYears \
  -H "Content-Type: application/json" \
  -d '{"year": 2025, "isActive": true}'
```

3. The system is now ready to generate periods for 2025. In the next phase, you'll implement the period generation logic that will:
   - Take the year and selected frequencies
   - Generate appropriate periods based on the frequency intervals
   - Calculate filing deadlines automatically

## Next Steps

After setting up the year management, the next implementation phase will include:

1. **Period Generation Service** - Automatically generates periods based on year and frequency
2. **Period Management API** - CRUD operations for periods
3. **Period Assignment** - Linking periods to specific forms and SACCOs
4. **Period Validation** - Ensuring no overlapping periods and proper date boundaries

## Notes

- The `CreatedBy` field will capture the user who created the record (defaults to "System" for seeded data)
- Years must be unique in the system
- Frequencies are developer-managed; admins can only toggle their active status
- The system maintains backward compatibility with existing ReturnForms relationships