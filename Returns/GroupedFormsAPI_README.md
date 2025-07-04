# Grouped Forms Due API Documentation

## Overview
The grouped forms API endpoints provide a way to view expected returns grouped by their frequency (e.g., Monthly, Quarterly, Annual, etc.). This makes it easier to visualize returns in a hierarchical manner and enables batch submission of returns by frequency type.

## New DTOs

### GroupedFormsDueDTO
Represents a group of forms that share the same frequency.

```csharp
public class GroupedFormsDueDTO
{
    public string FrequencyCode { get; set; }      // MTH, QTR, FY, WK, BWK, DAY
    public string FrequencyName { get; set; }      // Monthly, Quarterly, Annual, etc.
    public int TotalFormsInGroup { get; set; }
    public int FiledCount { get; set; }
    public int DueCount { get; set; }
    public int LateCount { get; set; }
    public int WaivedCount { get; set; }
    public List<FormsDueByMonthDTO> Forms { get; set; }
}
```

### FormsDueGroupedResponseDTO
The main response object containing summary statistics and grouped data.

```csharp
public class FormsDueGroupedResponseDTO
{
    public int TotalExpectedReturns { get; set; }
    public int TotalFiled { get; set; }
    public int TotalDue { get; set; }
    public int TotalLate { get; set; }
    public int TotalWaived { get; set; }
    public List<GroupedFormsDueDTO> GroupedByFrequency { get; set; }
}
```

## New Endpoints

### 1. Get Forms Due By Month (Grouped)
**Endpoint:** `GET /api/returns/GetFormsDueByMonthGrouped`

**Parameters:**
- `year` (required): The year (e.g., 2024)
- `month` (required): The month (1-12)
- `saccoTypeId` (optional): Filter by sacco type

**Example Request:**
```
GET /api/returns/GetFormsDueByMonthGrouped?year=2024&month=3
```

**Example Response:**
```json
{
    "totalExpectedReturns": 15,
    "totalFiled": 5,
    "totalDue": 7,
    "totalLate": 2,
    "totalWaived": 1,
    "groupedByFrequency": [
        {
            "frequencyCode": "MTH",
            "frequencyName": "Monthly",
            "totalFormsInGroup": 8,
            "filedCount": 3,
            "dueCount": 4,
            "lateCount": 1,
            "waivedCount": 0,
            "forms": [
                {
                    "formId": "123",
                    "formName": "Capital Adequacy Return",
                    "formCode": "CAR",
                    "periodId": "456",
                    "periodName": "March 2024",
                    "periodStartDate": "2024-03-01",
                    "periodEndDate": "2024-03-31",
                    "filingDeadline": "2024-04-15",
                    "status": "Due",
                    "isLate": false,
                    "templateUrl": "http://example.com/templates/car.xlsx",
                    "saccoTypeId": "DT"
                }
                // ... more forms
            ]
        },
        {
            "frequencyCode": "QTR",
            "frequencyName": "Quarterly",
            "totalFormsInGroup": 5,
            "filedCount": 2,
            "dueCount": 2,
            "lateCount": 1,
            "waivedCount": 0,
            "forms": [
                // ... quarterly forms
            ]
        }
        // ... other frequency groups
    ]
}
```

### 2. Get Forms Due By Year (Grouped)
**Endpoint:** `GET /api/returns/GetFormsDueByYearGrouped`

**Parameters:**
- `year` (required): The year (e.g., 2024)
- `saccoTypeId` (optional): Filter by sacco type

**Example Request:**
```
GET /api/returns/GetFormsDueByYearGrouped?year=2024
```

**Response Structure:** Same as above but includes all forms due within the specified year.

## Frequency Order
The groups are ordered by frequency in a logical progression:
1. Daily (DAY)
2. Weekly (WK)
3. Bi-weekly (BWK)
4. Monthly (MTH)
5. Quarterly (QTR)
6. Annual/Fiscal Year (FY)

## UI Implementation Benefits

### 1. Visual Hierarchy
- Display returns in collapsible sections by frequency
- Show summary statistics for each frequency group
- Provide visual indicators for completion status

### 2. Batch Operations
- Allow users to select all returns within a frequency group
- Enable bulk submission of returns (e.g., "Submit all quarterly returns")
- Facilitate bulk status updates

### 3. Filtering and Sorting
- Users can focus on specific frequency types
- Sort within groups by deadline, status, or form name
- Filter to show only pending returns within each group

### 4. Status Overview
- Quick view of how many returns are filed/due/late per frequency
- Progress indicators for each frequency group
- Priority highlighting for overdue returns

## Example UI Usage

```javascript
// Fetch grouped returns for current month
const response = await fetch('/api/returns/GetFormsDueByMonthGrouped?year=2024&month=3');
const data = await response.json();

// Display grouped data
data.groupedByFrequency.forEach(group => {
    console.log(`${group.frequencyName} Returns (${group.totalFormsInGroup} total)`);
    console.log(`- Filed: ${group.filedCount}`);
    console.log(`- Due: ${group.dueCount}`);
    console.log(`- Late: ${group.lateCount}`);
    
    // Display individual forms within the group
    group.forms.forEach(form => {
        console.log(`  - ${form.formName} (${form.status})`);
    });
});
```

## Migration Notes
- The existing endpoints (`GetFormsDueByMonth` and `GetFormsDueByYear`) remain unchanged
- New grouped endpoints complement the existing functionality
- No breaking changes to current API consumers
- UI can gradually adopt the grouped view while maintaining backward compatibility