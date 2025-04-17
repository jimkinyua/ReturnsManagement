using Returns.Models.Common;

namespace Returns.Models
{
    public class Category:CommonFields
    {
        public string CategoryName { get; set; } = null!;
        public string CategoryCode { get; set; } = null!;
        public ICollection<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
    }
}
