using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Returns.Models.Common
{
    public class CommonFields
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
