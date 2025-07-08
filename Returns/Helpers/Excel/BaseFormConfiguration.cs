using ClosedXML.Excel;
using Returns.Interfaces;
using Returns.Helpers.Excel.Mappers;

namespace Returns.Helpers.Excel
{
    public abstract class BaseFormConfiguration<T> : IFormConfiguration<T> where T : class, new()
    {
        public abstract string FormType { get; }
        public virtual string SheetName => string.Empty; // Use first sheet by default
        public abstract int DataStartRow { get; }
        public abstract int DataEndRow { get; }
        public abstract IRowMapper<T> RowMapper { get; }
        public virtual bool ValidateConsistency => true;
        
        public virtual FormMetadataLocation MetadataLocations { get; } = new FormMetadataLocation();
    }

    // Common base class for form DTOs
    public abstract class BaseFormStatement
    {
        public string SaccoCsNumber { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<ValidationError> ValidationErrors { get; set; } = new List<ValidationError>();
    }

    public class ValidationError
    {
        public string Field { get; set; }
        public string Message { get; set; }
        public int? RowNumber { get; set; }
    }
}