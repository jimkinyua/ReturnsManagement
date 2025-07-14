using Returns.Models.Common;

namespace Returns.Models
{
    public class RatingDefination: CommonFields
    {
        public string RatingName { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string SaccoType { get; set; } = null!;
        public List<RatingForm> RatingForms { get; set; } = new List<RatingForm>();
    }
}
