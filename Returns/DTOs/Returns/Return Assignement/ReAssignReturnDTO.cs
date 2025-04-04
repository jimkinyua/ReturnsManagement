using System.ComponentModel.DataAnnotations;

namespace Returns.DTOs.Returns.Return_Assignement
{
    public class ReAssignReturnDTO
    {
        [Required]
        public string ReturnId { get; set; } = string.Empty;
        [Required]
        public string AssignedUserId { get; set; } = string.Empty;
    }
}
