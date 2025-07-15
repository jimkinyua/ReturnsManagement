# Admin Return Management System

## Overview

The Admin Return Management System provides comprehensive functionality for administrators to view, filter, and analyze submitted returns with advanced CAMELS analysis capabilities and approval workflows.

## Key Features

### 1. Filtered Return List (`GET /api/returns/admin/returns`)

**Purpose**: Retrieve a paginated list of submitted returns with advanced filtering options.

**Query Parameters**:
- `month` (optional): Filter by month (1-12), defaults to current month
- `year` (optional): Filter by year, defaults to current year
- `saccoId` (optional): Filter by specific SACCO
- `saccoType` (optional): Filter by SACCO type (DT/NWDT)
- `period` (optional): Filter by period (Monthly/Quarterly)
- `page` (optional): Page number, defaults to 1
- `pageSize` (optional): Items per page, defaults to 20
- `sortBy` (optional): Sort field (saccname, submittedat, returnfor), defaults to submittedat
- `sortOrder` (optional): Sort order (asc/desc), defaults to desc

**Response**:
```json
{
  "returns": [
    {
      "id": "return-id",
      "returnsFor": "2024",
      "saccoId": "sacco-id",
      "saccoName": "SACCO Name",
      "submittedAt": "2024-01-15T10:30:00Z",
      "daysLateBy": 5,
      "lateNessStatus": "Late",
      "isConsistent": true,
      "consistentErrorMessage": null,
      "totalReturns": 7,
      "totalLateReturns": 2,
      "versionNumber": 1,
      "previousVersionIds": []
    }
  ],
  "totalCount": 150,
  "page": 1,
  "pageSize": 20,
  "totalPages": 8,
  "hasNextPage": true,
  "hasPreviousPage": false,
  "totalReturns": 150,
  "totalLateReturns": 45,
  "totalConsistentReturns": 120,
  "totalInconsistentReturns": 30
}
```

### 2. Return Details (`GET /api/returns/admin/returns/{returnId}/details`)

**Purpose**: Get comprehensive details of a specific return including all form data, analysis results, and workflow information.

**Response**:
```json
{
  "id": "return-id",
  "saccoId": "sacco-id",
  "saccoName": "SACCO Name",
  "saccoType": "DepositTaking",
  "returnFor": "2024-01-01T00:00:00Z",
  "submittedAt": "2024-01-15T10:30:00Z",
  "period": "Monthly",
  "versionNumber": 1,
  "isActiveVersion": true,
  "amendmentDate": null,
  "previousVersionId": null,
  "isNotConsistent": false,
  "consistentErrorMessage": null,
  "lateNessStatus": "On Time",
  "totalReturns": 7,
  "totalLateReturns": 0,
  
  // Form data (DT)
  "capitalAdequacy": { /* DT Capital Adequacy data */ },
  "liquidityReturn": { /* DT Liquidity data */ },
  "riskClassifications": [ /* DT Risk Classification data */ ],
  "investmentReturn": { /* DT Investment data */ },
  "financialPosition": { /* DT Financial Position data */ },
  "comprehensiveIncome": { /* DT Comprehensive Income data */ },
  "depositReturns": [ /* DT Deposit data */ ],
  
  // Form data (NWDT)
  "nwdtCapitalAdequacy": { /* NWDT Capital Adequacy data */ },
  "nwdtLiquidityReturn": { /* NWDT Liquidity data */ },
  "nwdtRiskClassifications": [ /* NWDT Risk Classification data */ ],
  "nwdtInvestmentReturn": { /* NWDT Investment data */ },
  "nwdtFinancialPosition": { /* NWDT Financial Position data */ },
  "nwdtComprehensiveIncome": { /* NWDT Comprehensive Income data */ },
  "nwdtDepositReturns": [ /* NWDT Deposit data */ ],
  
  // Other returns
  "otherReturns": [ /* Other return data */ ],
  "sectoralLendingReports": [ /* Sectoral lending data */ ],
  "dailyLiquidityReturns": [ /* Daily liquidity data */ ],
  "managementReturns": [ /* Management return data */ ],
  
  // Analysis results
  "camelsAnalysis": {
    "capitalRating": 3,
    "assetQualityRating": 2,
    "managementRating": 4,
    "earningsRating": 3,
    "liquidityRating": 2,
    "sensitivityRating": 3,
    "overallRating": 3,
    "riskLevel": "Medium"
  },
  "hasCamelsAnalysis": true,
  "camelsAnalysisDate": "2024-01-16T14:30:00Z",
  "camelsAnalysisBy": "admin-user-id",
  
  // Workflow information
  "currentWorkflowState": {
    "workflowInstanceId": "workflow-id",
    "returnId": "return-id",
    "currentStepId": "step-id",
    "currentApproverId": "approver-id",
    "status": "Pending",
    "rating": 3,
    "isFirst": false,
    "isLast": true,
    "nextSteps": []
  },
  "workflowComments": [
    {
      "comment": "Analysis completed",
      "userId": "user-id",
      "approverName": "John Doe",
      "status": "Approved",
      "createdAt": "2024-01-16T15:00:00Z"
    }
  ],
  
  // Additional information requests
  "additionalInfoRequests": [ /* Additional info request data */ ],
  
  // File attachments
  "attachments": [ /* Attachment data */ ]
}
```

### 3. CAMELS Analysis Submission (`POST /api/returns/admin/returns/{returnId}/camels-analysis`)

**Purpose**: Submit CAMELS analysis for a return with approval workflow.

**Request Body**:
```json
{
  "returnId": "return-id",
  "analysis": {
    "capitalRating": 3,
    "assetQualityRating": 2,
    "managementRating": 4,
    "earningsRating": 3,
    "liquidityRating": 2,
    "sensitivityRating": 3,
    "overallRating": 3,
    "riskLevel": "Medium"
  },
  "comments": "Analysis completed based on financial data review",
  "recommendation": "Approve",
  "requestedChanges": []
}
```

**Response**:
```json
{
  "workflowInstanceId": "workflow-id",
  "status": "Pending",
  "message": "CAMELS analysis submitted successfully",
  "createdAt": "2024-01-16T15:00:00Z",
  "createdBy": "admin-user-id"
}
```

### 4. CAMELS Analysis Status (`GET /api/returns/admin/returns/{returnId}/camels-status`)

**Purpose**: Get the current status of CAMELS analysis for a return.

**Response**:
```json
{
  "returnId": "return-id",
  "hasAnalysis": true,
  "analysisDate": "2024-01-16T14:30:00Z",
  "analyzedBy": "admin-user-id",
  "approvalStatus": "Pending",
  "approvalDate": null,
  "approvedBy": null,
  "comments": "Analysis completed based on financial data review",
  "analysis": {
    "capitalRating": 3,
    "assetQualityRating": 2,
    "managementRating": 4,
    "earningsRating": 3,
    "liquidityRating": 2,
    "sensitivityRating": 3,
    "overallRating": 3,
    "riskLevel": "Medium"
  }
}
```

## Workflow Integration

### CAMELS Analysis Approval Process

1. **Analysis Submission**: Admin submits CAMELS analysis via `POST /api/returns/admin/returns/{returnId}/camels-analysis`
2. **Workflow Creation**: System creates approval workflow if recommendation is not "Approve"
3. **Approval Process**: Other users can approve/reject the analysis through existing workflow endpoints
4. **Status Tracking**: Use `GET /api/returns/admin/returns/{returnId}/camels-status` to track approval status

### Workflow Endpoints Integration

The system integrates with existing workflow endpoints:
- `GET /api/returns/ApprovalRequests` - Get pending approval requests
- `POST /api/returns/ApproveRequest` - Approve a workflow step
- `POST /api/returns/RecommendForEnforcement` - Recommend for enforcement
- `POST /api/returns/RejectWithReservations` - Reject with reservations

## Usage Examples

### Example 1: Get Returns for Current Month
```bash
GET /api/returns/admin/returns?month=1&year=2024&page=1&pageSize=20
```

### Example 2: Get Returns for Specific SACCO
```bash
GET /api/returns/admin/returns?saccoId=sacco-123&sortBy=saccname&sortOrder=asc
```

### Example 3: Get Late Returns
```bash
GET /api/returns/admin/returns?page=1&pageSize=50&sortBy=submittedat&sortOrder=desc
```

### Example 4: Submit CAMELS Analysis
```bash
POST /api/returns/admin/returns/return-123/camels-analysis
Content-Type: application/json

{
  "returnId": "return-123",
  "analysis": {
    "capitalRating": 3,
    "assetQualityRating": 2,
    "managementRating": 4,
    "earningsRating": 3,
    "liquidityRating": 2,
    "sensitivityRating": 3,
    "overallRating": 3,
    "riskLevel": "Medium"
  },
  "comments": "Analysis completed",
  "recommendation": "Approve"
}
```

## Error Handling

The system provides comprehensive error handling:

- **401 Unauthorized**: User not authenticated
- **404 Not Found**: Return not found
- **400 Bad Request**: Invalid request parameters
- **500 Internal Server Error**: Server-side errors

## Security Considerations

1. **Authentication**: All endpoints require valid authentication
2. **Authorization**: Admin role required for all endpoints
3. **Data Validation**: All input data is validated
4. **Audit Trail**: All actions are logged for audit purposes

## Database Schema Updates

The system uses the existing `SaccoAnalysis` table with updated schema:

```sql
-- Updated SaccoAnalysis table
CREATE TABLE SaccoAnalysis (
    Id NVARCHAR(450) PRIMARY KEY,
    ReturnId NVARCHAR(450) NOT NULL,
    AnalysisData NVARCHAR(MAX), -- JSON serialized CAMELS analysis
    Status NVARCHAR(50) DEFAULT 'Pending',
    Comments NVARCHAR(MAX),
    ApprovedBy NVARCHAR(450),
    ApprovalDate DATETIME2,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(450),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(450)
);
```

## Future Enhancements

1. **Bulk Operations**: Support for bulk CAMELS analysis submission
2. **Advanced Filtering**: More sophisticated filtering options
3. **Export Functionality**: Export filtered results to Excel/PDF
4. **Dashboard Integration**: Real-time dashboard with analytics
5. **Notification System**: Email/SMS notifications for status changes