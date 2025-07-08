# Excel Import Architecture Refactoring

## Overview

The Excel import system has been completely refactored from a monolithic, hard-coded approach to a clean, metadata-driven architecture that eliminates code duplication and makes adding new forms trivial.

## Key Benefits

1. **Zero Duplication**: All common functionality (file validation, Excel reading, error handling) is centralized
2. **5-Line Form Addition**: Adding a new form now requires just a configuration class, not 200+ lines of code
3. **Testable**: Every component is interface-based and mockable
4. **Consistent**: All forms follow the same processing pipeline and error handling
5. **Maintainable**: Changes to common behavior happen in one place

## Architecture Components

### Core Interfaces

```csharp
IExcelImportService      // Main service for importing Excel files
IFormProcessor          // Processes specific form types
IFormConfiguration<T>   // Metadata for form structure
IRowMapper<T>          // Maps Excel rows to DTOs
IFormProcessorFactory  // Resolves processors by form type
```

### Processing Pipeline

1. **File Validation** → 2. **Excel Opening** → 3. **Header Reading** → 4. **Row Iteration** → 5. **DTO Mapping** → 6. **Validation** → 7. **Database Save**

## Adding a New Form

To add a new form, you only need to create:

### 1. Configuration Class (5-10 lines)

```csharp
public class NewFormConfiguration : BaseFormConfiguration<NewFormDto>
{
    public override string FormType => "NEW_FORM";
    public override int DataStartRow => 10;
    public override int DataEndRow => 50;
    public override IRowMapper<NewFormDto> RowMapper => new NewFormRowMapper();
}
```

### 2. Row Mapper (defines cell mappings)

```csharp
public class NewFormRowMapper : IRowMapper<NewFormDto>
{
    public NewFormDto MapRow(IXLRow row, int rowNumber)
    {
        return new NewFormDto
        {
            Field1 = ParseDecimal(row.Cell("D")),
            Field2 = row.Cell("E").Value.ToString(),
            // ... map other fields
        };
    }
    
    public bool ShouldSkipRow(IXLRow row, int rowNumber)
    {
        return string.IsNullOrWhiteSpace(row.Cell("A").Value.ToString());
    }
}
```

### 3. Processor (if special logic needed)

```csharp
public class NewFormProcessor : BaseFormProcessor<NewFormConfiguration, NewFormDto>
{
    public override string FormType => "NEW_FORM";
    
    protected override async Task SaveStatementAsync(
        NewFormDto statement, string returnId, string submissionId)
    {
        // Map DTO to entity and save
    }
}
```

### 4. Register in DI

In `ServiceCollectionExtensions.cs`:
```csharp
services.AddSingleton<IFormConfiguration<NewFormDto>, NewFormConfiguration>();
services.AddScoped<NewFormProcessor>();
```

## Usage Example

### Controller
```csharp
[HttpPost("{returnId}/forms")]
public async Task<IActionResult> ProcessForm(
    string returnId, 
    [FromForm] IFormFile file, 
    [FromForm] string formType)
{
    var result = await _returnSubmissionService.ProcessFormSubmissionAsync(
        file, returnId, formType, userId);
    
    return result.Status == SubmissionStatus.Success ? Ok(result) : BadRequest(result);
}
```

### Service Layer
```csharp
public async Task<SubmissionResultDto> ProcessFormSubmissionAsync(
    IFormFile file, string returnId, string formType, string userId)
{
    // Creates ReturnSubmission record
    // Calls IExcelImportService.ImportFormAsync
    // Updates submission with results
    // Returns unified SubmissionResultDto
}
```

## Configuration Reference

### Form Metadata Locations
```csharp
public class FormMetadataLocation
{
    public string SaccoCsNumberCell { get; set; } = "D3";
    public string PeriodCell { get; set; } = "D4";
    public string StartDateCell { get; set; } = "D5";
    public string EndDateCell { get; set; } = "F5";
}
```

### Common Validation Rules
- File size limits (configurable)
- File extension validation (.xlsx, .xls)
- Required header fields
- Date range validation
- Cross-field consistency checks

## Error Handling

All errors flow through a consistent pipeline:
1. File validation errors → `ValidationException`
2. Excel parsing errors → Logged and returned in `SubmissionResultDto`
3. Business rule violations → Collected in validation list
4. Database errors → Wrapped with context

## Testing

The new architecture is fully testable:

```csharp
[Fact]
public async Task ImportFormAsync_WithValidFile_ProcessesSuccessfully()
{
    // Arrange
    var file = CreateMockExcelFile();
    var processorMock = new Mock<IFormProcessor>();
    
    // Act
    var result = await _excelImportService.ImportFormAsync(file, "FORM_TYPE", "returnId");
    
    // Assert
    Assert.Equal(SubmissionStatus.Success, result.Status);
}
```

## Migration from Old System

1. Identify all form types in `ProcessFormByType`
2. Create configuration + mapper for each
3. Test with sample files
4. Update controllers to use new service
5. Remove old ExcelService methods

## Performance Considerations

- Uses streaming for large files
- Processes rows in batches
- Validates incrementally
- Supports async throughout

## Future Enhancements

1. Add form versioning support
2. Implement bulk import for multiple forms
3. Add preview/dry-run mode
4. Support for formula evaluation
5. Export templates generation