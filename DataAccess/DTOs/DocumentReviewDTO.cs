using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DTOs
{
    public class DocumentReviewDto
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string Comments { get; set; }
        public DateTime CreateAt { get; set; }
        public string ReviewedBy { get; set; }
        public string ReviewedAt { get; set; }
    }

    public class DocumentDetailDto
    {
        public string Type { get; set; }
        public int DocumentId { get; set; }
        public string Code { get; set; }
        public string Comments { get; set; }
        public string ImageFront { get; set; }
        public string ImageBack { get; set; }
        public string Status { get; set; }
    }


    public class UpdateDocumentReviewDto
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string Comments { get; set; }
        public int ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}
