using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class OtherReturn: FormBase
    {
        public string FormName { get; set; } = null!;
        public string FileUrl { get; set; } = null!;
        public string SaccoId { get; set; } = null!;
        public string SaccoType { get; set; } = null!;
        public string SaccoName { get; set; } = "";
        [ForeignKey("ReturnId")]
        public string ReturnId { get; set; } = null!;
        public virtual Return Return { get; set; } = null!;

    }
}
