# Draft/Submit Workflow Documentation

## Overview

The new draft/submit workflow separates the return filing process into two distinct phases:
1. **Draft Phase**: SACCOs can upload forms incrementally and save as draft
2. **Submit Phase**: When ready, SACCOs submit their draft for processing

This approach provides several benefits:
- Incremental uploads without triggering consistency checks
- Ability to save progress and return later
- Clear separation between data entry and submission
- Better error handling and user experience

## Architecture

### Key Services

1. **UnifiedExcelImportService**
   - Centralized Excel import logic
   - Strategy pattern for different form types
   - Generic approach for easier maintenance

2. **DraftReturnService**
   - Handles draft creation and updates
   - Manages form uploads to draft returns
   - Uses reflection for generic entity handling

3. **ReturnSubmissionService**
   - Converts drafts to submitted returns
   - Runs consistency checks
   - Initiates workflow and notifications

### Data Flow

```
User Upload → UnifiedExcelImportService → DraftReturnService → Database (Draft)
                                                     ↓
                                            User Submits Draft
                                                     ↓
                                         ReturnSubmissionService
                                                     ↓
                                    Consistency Checks → Workflow → Final Return
```

## API Endpoints

### 1. Save Draft
```http
POST /api/returns/Draft
Content-Type: multipart/form-data

{
  "returnId": "string (optional - null for new draft)",
  "period": "2024",
  "formUploads": [
    {
      "formId": "form-123",
      "formFile": <file>,
      "expectedReturnId": "optional-id"
    }
  ]
}
```

**Response:**
```json
{
  "returnId": "draft-return-id",
  "message": "Draft saved successfully",
  "processedForms": [
    {
      "formId": "form-123",
      "formName": "Capital Adequacy",
      "success": true,
      "message": "Draft saved successfully"
    }
  ]
}
```

### 2. Submit Draft
```http
POST /api/returns/Submit/{draftReturnId}
```

**Response:**
```json
{
  "returnId": "final-return-id",
  "message": "Return submitted successfully",
  "isConsistent": true,
  "consistencyErrors": []
}
```

### 3. Get Draft
```http
GET /api/returns/Draft/{returnId}
```

**Response:**
```json
{
  "returnId": "draft-return-id",
  "period": "2024",
  "createdAt": "2024-01-15T10:30:00",
  "forms": [
    {
      "formType": "CapitalAdequacy",
      "formId": "form-123",
      "createdAt": "2024-01-15T10:30:00"
    }
  ]
}
```

### 4. Delete Draft
```http
DELETE /api/returns/Draft/{returnId}
```

## Implementation Details

### Adding New Form Types

To add a new form type:

1. **Add Import Strategy** to `UnifiedExcelImportService`:
```csharp
_importStrategies["NewFormType_DT"] = (file, logger) => 
    ExcelService.ImportNewFormTypeRows(file, logger);
```

2. **Create Mapping Method** in `DraftReturnService`:
```csharp
private object MapToNewFormTypeEntity(object importedData)
{
    if (importedData is NewFormTypeStatement form)
    {
        return new NewFormTypeReturn
        {
            Id = Guid.NewGuid().ToString(),
            StartDate = form.StartDate,
            EndDate = form.EndDate,
            IsDraft = true
            // Map other fields
        };
    }
    return null;
}
```

3. **Update Form Type Detection**:
```csharp
private string DetermineFormType(ReturnForm form)
{
    // Add new condition
    if (form.IsNewFormType) return "NewFormType";
    // ... existing conditions
}
```

### Database Schema

All child entities need the following properties:
```csharp
public bool IsDraft { get; set; } = false;
public bool IsCurrent { get; set; } = true;
public bool IsAmended { get; set; } = false;
public string? PreviousReturnId { get; set; }
```

The Return entity needs:
```csharp
public bool IsDraft { get; set; } = false;
```

### Consistency Checks

Consistency checks are only run during submission, not during draft saves. The checks include:
- Total assets consistency across forms
- Deposit totals matching
- Risk classification totals
- Other business rules as defined

### Error Handling

The system provides detailed error messages at each stage:
- **Import Errors**: Excel parsing issues, invalid data formats
- **Draft Save Errors**: Database issues, invalid form references
- **Submission Errors**: Missing required forms, consistency failures

## Migration Guide

### For Existing Code

1. **Replace Direct Processing** with draft/submit workflow:
```csharp
// Old approach
await _formProcessor.ProcessFormBatchAsync(dto, loggedInSacco, ...);

// New approach
var draftResult = await _draftReturnService.SaveDraftAsync(draftDto, ...);
if (draftResult.Success)
{
    var submitResult = await _returnSubmissionService.SubmitDraftReturnAsync(
        draftResult.ReturnId, loggedInSacco);
}
```

2. **Update Form Processing** to use UnifiedExcelImportService:
```csharp
// Old approach
var data = ExcelService.ImportCapitalAdequacyRows(file, logger);

// New approach
var data = await _excelImportService.ImportFormDataAsync<Form1Statement>(
    file, formMetadata);
```

### Database Migration

Run the following migration to add IsDraft columns:
```sql
ALTER TABLE Returns ADD IsDraft BIT NOT NULL DEFAULT 0;
ALTER TABLE DTCapitalAdequacyReturns ADD IsDraft BIT NOT NULL DEFAULT 0;
ALTER TABLE DTLiquidityReturns ADD IsDraft BIT NOT NULL DEFAULT 0;
-- Repeat for all child tables
```

## Best Practices

1. **Always validate draft completeness** before submission
2. **Implement proper cleanup** for abandoned drafts
3. **Log all operations** for audit trail
4. **Use transactions** for data consistency
5. **Provide clear user feedback** at each stage

## Testing

### Unit Tests
```csharp
[Test]
public async Task SaveDraft_WithValidData_ShouldSucceed()
{
    // Arrange
    var draftDto = new DraftReturnDto { Period = "2024" };
    
    // Act
    var result = await _draftReturnService.SaveDraftAsync(
        draftDto, "sacco-123", "DT");
    
    // Assert
    Assert.IsTrue(result.Success);
    Assert.IsNotNull(result.ReturnId);
}
```

### Integration Tests
- Test full workflow from draft to submission
- Test consistency check scenarios
- Test error handling and rollback

## Troubleshooting

### Common Issues

1. **"Draft return not found"**
   - Ensure draft exists and belongs to the logged-in SACCO
   - Check if draft hasn't been already submitted

2. **"Missing required forms"**
   - Verify all mandatory forms are uploaded to draft
   - Check form type configuration

3. **"Consistency errors"**
   - Review the detailed error messages
   - Verify data accuracy across forms

### Debug Tips

Enable detailed logging:
```json
{
  "Logging": {
    "LogLevel": {
      "Returns.Helpers": "Debug"
    }
  }
}
```

## Future Enhancements

1. **Auto-save functionality** for drafts
2. **Draft templates** for common scenarios
3. **Bulk operations** for multiple returns
4. **Advanced validation rules** engine
5. **Real-time collaboration** on drafts