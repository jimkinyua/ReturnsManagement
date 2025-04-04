using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Returns.DTOs.Returns_Submission.DT
{
    public class NewReturnDTO
    {
        public string SaccoId { get; set; }
        public DateTime SubmissionDate { get; set; } = DateTime.Now;
        public ICollection<ReturnFormUploadDTO> FormUploads { get; set; } = new List<ReturnFormUploadDTO>();

    }
}
