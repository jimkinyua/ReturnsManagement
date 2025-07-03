using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class QuarterDates : CommonFields
    {
        public int StartMonth { get; set; }
        public int StartDay { get; set; }
        public int EndMonth { get; set; }
        public int EndDay { get; set; }
        public int DeadlineMonth { get; set; }
        public int DeadlineDay { get; set; }
        [ForeignKey("Period")]
        public string PeriodId { get; set; } = null!;
        public ReturnPeriods Period { get; set; } = null!;

    }
}
