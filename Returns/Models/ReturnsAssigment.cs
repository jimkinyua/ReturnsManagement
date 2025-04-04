using Returns.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class ReturnsAssigment: CommonFields
    {
        public string UserId { get; set; } = null!;
        [ForeignKey("Return")]
        public string ReturnId { get; set; } = null!;
        public string? Comments { get; set; }
        public Return Return { get; set; } = null!;
    }
}
