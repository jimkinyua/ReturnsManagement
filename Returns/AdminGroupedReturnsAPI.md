# Admin Grouped Returns API

This API allows administrators to view returns grouped by rating definitions, enabling them to see related forms that need to be read together.

## Overview

The system groups returns based on `RatingDefinition` and `RatingForm` tables, which define which forms need to be analyzed together (e.g., for CAMELS analysis).

## API Endpoints

### 1. Get Grouped Returns (with filtering)

**GET** `/api/returns/admin/grouped-returns`

**Query Parameters:**
- `Year` (optional): Filter by year (e.g., 2025)
- `Month` (optional): Filter by month (1-12)
- `SaccoType` (optional): Filter by SACCO type (e.g., "DepositTaking", "NWDT")
- `Frequency` (optional): Filter by frequency (e.g., "Monthly", "Quarterly")
- `PeriodId` (optional): Filter by specific period ID
- `RatingDefinitionId` (optional): Filter by specific rating definition
- `IsComplete` (optional): Filter by completion status (true/false)
- `Page` (optional): Page number for pagination (default: 1)
- `PageSize` (optional): Items per page (default: 20)

**Example Request:**
```
GET /api/returns/admin/grouped-returns?Year=2025&Month=3&SaccoType=DepositTaking&Frequency=Monthly&Page=1&PageSize=10
```

**Response:**
```json
[
  {
    "groupId": "rating-def-id-1",
    "ratingName": "CAMELS Analysis DT",
    "description": "Capital Adequacy, Asset Quality, Management, Earnings, Liquidity, Sensitivity",
    "saccoType": "DepositTaking",
    "saccoId": "sacco-123",
    "saccoName": "ABC SACCO",
    "periodId": "period-456",
    "periodName": "Q1 2025",
    "year": 2025,
    "frequency": "Quarterly",
    "startDate": "2025-01-01T00:00:00",
    "endDate": "2025-03-31T00:00:00",
    "submittedAt": "2025-04-15T10:30:00",
    "status": "On Time",
    "isComplete": true,
    "totalRequiredForms": 7,
    "submittedForms": 7,
    "forms": [
      {
        "formId": "form-1",
        "formCode": "CA",
        "formName": "Capital Adequacy",
        "submissionId": "sub-1",
        "submittedAt": "2025-04-10T09:00:00",
        "status": "Submitted",
        "isSubmitted": true,
        "isLate": false,
        "daysLate": 0
      }
    ]
  }
]
```

### 2. Get Grouped Return Details

**GET** `/api/returns/admin/grouped-returns/{groupId}/{periodId}/{saccoId}`

**Example Request:**
```
GET /api/returns/admin/grouped-returns/rating-def-id-1/period-456/sacco-123
```

**Response:** Same as above but with complete form details for the specific group.

### 3. Get Filter Options

#### Years
**GET** `/api/returns/admin/filter-options/years`

**Response:**
```json
["2025", "2024", "2023"]
```

#### SACCO Types
**GET** `/api/returns/admin/filter-options/sacco-types`

**Response:**
```json
["DepositTaking", "NWDT"]
```

#### Frequencies
**GET** `/api/returns/admin/filter-options/frequencies`

**Response:**
```json
["Monthly", "Quarterly", "Annually"]
```

## How It Works

1. **Grouping Logic**: Returns are grouped based on `RatingDefinition` which contains `RatingForm` entries that define which forms need to be read together.

2. **Filtering**: The API supports filtering by:
   - **Year**: Filter by reporting year
   - **Month**: Filter by specific month (1-12)
   - **SaccoType**: Filter by SACCO type (DepositTaking/NWDT)
   - **Frequency**: Filter by period frequency (Monthly/Quarterly/etc.)
   - **PeriodId**: Filter by specific period
   - **RatingDefinitionId**: Filter by specific rating definition
   - **IsComplete**: Filter by completion status

3. **Completion Status**: A group is considered complete when all required forms (defined in `RatingForm`) have been submitted.

4. **Status Tracking**: Each form shows:
   - Whether it's submitted
   - If it's late
   - Days late
   - Current status

## Usage Examples

### View all CAMELS analysis returns for Q1 2025
```
GET /api/returns/admin/grouped-returns?Year=2025&Month=3&Frequency=Quarterly
```

### View incomplete returns for Deposit Taking SACCOs
```
GET /api/returns/admin/grouped-returns?SaccoType=DepositTaking&IsComplete=false
```

### View specific rating definition returns
```
GET /api/returns/admin/grouped-returns?RatingDefinitionId=rating-def-id-1
```

## Frontend Integration

The frontend can use this API to:

1. **Display grouped returns** in a table/grid format
2. **Show completion status** with visual indicators
3. **Enable filtering** with dropdown menus populated from filter options
4. **Drill down** to see detailed form information
5. **Track late submissions** with appropriate highlighting

## Benefits

1. **Organized View**: Returns are grouped logically based on rating definitions
2. **Efficient Analysis**: Related forms are presented together for easier analysis
3. **Flexible Filtering**: Multiple filter options for different use cases
4. **Status Tracking**: Clear visibility of completion and late submission status
5. **Scalable**: Pagination support for large datasets