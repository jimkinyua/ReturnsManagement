using Returns.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class Period : CommonFields
    {
        public string Name { get; set; } = null!;
        public ICollection<ReturnForm> ReturnForms { get; set; } = new List<ReturnForm>();

    }
}
