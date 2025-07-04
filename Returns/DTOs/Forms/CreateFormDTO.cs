using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Returns.Helpers.Enums;

namespace Returns.DTOs.Forms
{
    public class CreateFormDTO : IValidatableObject
    {
        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string DisplayName { get; set; } = null!;  // This is the Code

        [Required]
        public string SaccoTypeId { get; set; } = null!;

        [Required]
        public FormCategory Category { get; set; }

        public IFormFile? Template { get; set; }  // Required unless Category is Other

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Category != FormCategory.Other && Template == null)
            {
                yield return new ValidationResult("Template is required unless the form category is 'Other'.", new[] { nameof(Template) });
            }
        }
    }
}