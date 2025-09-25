using BOCS.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BOCS.ModelsView
{
    public class EnrollmentCreateVM
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = "";
        public int CoursePriceBdt { get; set; }

        [Display(Name = "Course access")]
        [Required]
        public CourseAccessType Access { get; set; } = CourseAccessType.Full;

        [Display(Name = "Payment method")]
        [Required]
        public PaymentMethodType PaymentMethod { get; set; } = PaymentMethodType.BKash;

        [Display(Name = "Payment transaction id")]
        [Required, StringLength(64)]
        public string TransactionId { get; set; } = "";

        [Display(Name = "Sender number")]
        [Required, StringLength(20)]
        public string SenderNumber { get; set; } = "";

        [Display(Name = "Your mobile number")]
        [Required, StringLength(20)]
        public string MobileNumber { get; set; } = "";
        public int CourseDurationDays { get; set; }

        [Column(TypeName = "date")] 
        [DataType(DataType.Date)]
        [Display(Name = "Start date")]
        public DateTime StartDate { get; set; }

        [Column(TypeName = "date")] 
        [DataType(DataType.Date)]
        [Display(Name = "End date")]
        public DateTime EndDate { get; set; }
    }
}
