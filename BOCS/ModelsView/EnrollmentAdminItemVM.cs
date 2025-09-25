using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BOCS.ModelsView
{
    public class EnrollmentAdminItemVM
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = "";
        public string StudentId { get; set; } = "";
        public string StudentName { get; set; } = "";
        public string StudentEmail { get; set; } = "";
        public string PaymentMethod { get; set; } = "";
        public string? TransactionId { get; set; }
        public string? SenderNumber { get; set; }
        public string? MobileNumber { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
        //20092025
        public int CourseDurationDays { get; set; }

        [Column(TypeName = "date")]
        [DataType(DataType.Date)]
        [Display(Name = "Start date")]
        public DateTime StartDate { get; set; }

        [Column(TypeName = "date")]
        [DataType(DataType.Date)]
        [Display(Name = "End date")]
        public DateTime EndDate { get; set; }
        //End
        public bool IsApproved { get; set; }
        public bool IsArchived { get; set; }
        public bool Tic { get; set; }
    }
}
