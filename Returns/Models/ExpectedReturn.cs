using Returns.Helpers.Enums;
using Returns.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class ExpectedReturn
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = null!;  // PK, GUID

        [ForeignKey("Period")]
        public string PeriodId { get; set; } = null!;  // FK → ReturnPeriods
        public ReturnPeriods Period { get; set; } = null!;

        [ForeignKey("ReturnForm")]
        public string ReturnFormId { get; set; } = null!;  // FK → ReturnForm
        public ReturnForm ReturnForm { get; set; } = null!;

        public DateTime FilingDeadline { get; set; }
        public ExpectedStatus Status { get; set; }  // enum { Due, Late, Filed, Waived }
        public bool IsActive { get; set; } = true;
    }
}