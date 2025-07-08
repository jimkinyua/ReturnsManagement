using Returns.Helpers;
using Returns.Helpers.Interfaces;
using Returns.Models;
using Returns.DTOs.Forms;
using Microsoft.AspNetCore.Mvc;

namespace Returns.Examples
{
    /// <summary>
    /// Example demonstrating the refactored draft/submit workflow
    /// following Single Responsibility Principle
    /// </summary>
    public class DraftSubmitExample
    {
        private readonly IExcelImportService _excelImportService;
        private readonly DraftReturnService _draftReturnService;
        private readonly ReturnSubmissionService _returnSubmissionService;

        public DraftSubmitExample(
            IExcelImportService excelImportService,
            DraftReturnService draftReturnService,
            ReturnSubmissionService returnSubmissionService)
        {
            _excelImportService = excelImportService;
            _draftReturnService = draftReturnService;
            _returnSubmissionService = returnSubmissionService;
        }

        /// <summary>
        /// Example of how the system now works with separation of concerns
        /// </summary>
        public async Task<IActionResult> ProcessReturnWorkflow(DraftReturnDto dto, string saccoId)
        {
            // Step 1: Save as Draft (Single Responsibility: Draft Management)
            // The DraftReturnService handles all draft-related operations
            var draftResult = await _draftReturnService.SaveDraftAsync(dto, saccoId, "DT");
            
            if (!draftResult.Success)
            {
                return new BadRequestObjectResult(new { error = draftResult.Message });
            }

            // User can save multiple times, upload different forms incrementally
            // Each service has a single responsibility:
            // - UnifiedExcelImportService: Extract data from Excel
            // - DraftReturnService: Manage draft state
            // - No consistency checks at this stage

            // Step 2: Submit Draft (Single Responsibility: Submission & Validation)
            // The ReturnSubmissionService handles submission logic
            var submitResult = await _returnSubmissionService.SubmitDraftReturnAsync(
                draftResult.ReturnId, saccoId);

            if (!submitResult.Success)
            {
                return new BadRequestObjectResult(new 
                { 
                    error = submitResult.Message,
                    consistencyErrors = submitResult.ConsistencyErrors
                });
            }

            return new OkObjectResult(new 
            { 
                returnId = submitResult.ReturnId,
                message = "Return submitted successfully"
            });
        }

        /// <summary>
        /// Example showing how generics are used to update parent-child relationships
        /// </summary>
        public class GenericExampleExplanation
        {
            /* 
            The refactored system uses generics in several ways:

            1. UnifiedExcelImportService:
               - ImportFormDataAsync<T> allows type-safe extraction
               - Strategy pattern eliminates if-else chains

            2. DraftReturnService:
               - Uses reflection to handle any entity type
               - Automatically sets ReturnId on child entities
               - No need to know specific types at compile time

            3. Generic Parent-Child Update:
               Instead of:
               ```
               if (formType == "CapitalAdequacy")
               {
                   capitalAdequacy.ReturnSubmissionId = returnId;
               }
               else if (formType == "Liquidity")
               {
                   liquidity.ReturnSubmissionId = returnId;
               }
               ```

               We now have:
               ```
               UpdateReturnIdGeneric(entity, returnId);
               ```

               This method uses reflection to find and update the return ID
               regardless of the entity type.

            4. Benefits:
               - Single place to update logic
               - Easy to add new form types
               - Follows Open/Closed Principle
               - Type safety where possible
               - Flexibility through reflection where needed
            */
        }

        /// <summary>
        /// Example of adding a new form type with minimal changes
        /// </summary>
        public class AddingNewFormType
        {
            /*
            To add a new form type (e.g., "CashFlowStatement"):

            1. Add to UnifiedExcelImportService constructor:
               _importStrategies["CashFlow_DT"] = (file, logger) => 
                   ExcelService.ImportCashFlowRows(file, logger);

            2. Add mapping in DraftReturnService:
               private object MapToCashFlowEntity(object importedData)
               {
                   if (importedData is CashFlowStatement cashFlow)
                   {
                       return new DTCashFlowReturn
                       {
                           Id = Guid.NewGuid().ToString(),
                           StartDate = cashFlow.StartDate,
                           EndDate = cashFlow.EndDate,
                           IsDraft = true
                           // Map fields...
                       };
                   }
                   return null;
               }

            3. That's it! The generic infrastructure handles:
               - Setting ReturnId on the entity
               - Adding to correct DbSet
               - Draft/Submit workflow
               - Consistency checks (if configured)

            No changes needed in controllers or other services!
            */
        }

        /// <summary>
        /// Example showing the flow with actual usage
        /// </summary>
        public async Task<object> CompleteFlowExample(
            IFormFile capitalAdequacyFile,
            IFormFile liquidityFile,
            string saccoId,
            string period)
        {
            // Phase 1: Create draft and upload first form
            var draftDto = new DraftReturnDto
            {
                Period = period,
                FormUploads = new List<DraftFormUpload>
                {
                    new DraftFormUpload
                    {
                        FormId = "capital-adequacy-form-id",
                        FormFile = capitalAdequacyFile
                    }
                }
            };

            var draft1 = await _draftReturnService.SaveDraftAsync(draftDto, saccoId, "DT");
            var returnId = draft1.ReturnId;

            // Phase 2: Add another form to existing draft
            var updateDto = new DraftReturnDto
            {
                ReturnId = returnId, // Existing draft
                Period = period,
                FormUploads = new List<DraftFormUpload>
                {
                    new DraftFormUpload
                    {
                        FormId = "liquidity-form-id",
                        FormFile = liquidityFile
                    }
                }
            };

            var draft2 = await _draftReturnService.SaveDraftAsync(updateDto, saccoId, "DT");

            // Phase 3: Submit when ready
            var submitResult = await _returnSubmissionService.SubmitDraftReturnAsync(
                returnId, saccoId);

            return new
            {
                Success = submitResult.Success,
                ReturnId = submitResult.ReturnId,
                IsConsistent = submitResult.IsConsistent,
                Workflow = "Initiated if successful"
            };
        }
    }

    /// <summary>
    /// Comparison showing before and after refactoring
    /// </summary>
    public class BeforeAndAfterComparison
    {
        /*
        BEFORE (Tightly Coupled):
        - FileReturnsAsync did everything: Excel import, validation, saving, workflow
        - FormProcessingService mixed concerns: import, mapping, database operations
        - Hard to test individual components
        - Adding new forms required changes in multiple places

        AFTER (Single Responsibility):
        - UnifiedExcelImportService: Only handles Excel data extraction
        - DraftReturnService: Only manages draft state
        - ReturnSubmissionService: Only handles submission and validation
        - Each service can be tested independently
        - Adding new forms requires minimal changes

        GENERIC APPROACH:
        - Uses ImportFormDataAsync<T> for type-safe imports where possible
        - Uses reflection for dynamic entity handling
        - Parent-child relationships updated generically
        - No need for form-specific code in main workflow

        BENEFITS:
        1. Easier to understand (each service has one job)
        2. Easier to test (mock dependencies)
        3. Easier to extend (add new forms with minimal changes)
        4. Better error handling (each layer handles its own errors)
        5. More maintainable (changes are localized)
        */
    }
}