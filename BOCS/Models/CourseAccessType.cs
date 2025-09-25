using System.ComponentModel.DataAnnotations;

namespace BOCS.Models
{
    public enum CourseAccessType
    {
        [Display(Name = "Full")] Full = 1,
        [Display(Name = "Half")] Half = 2
    }

    public enum PaymentMethodType
    {
        [Display(Name = "bKash")] BKash = 1,
        [Display(Name = "Dutch-Bangla Bank Rocket")] Rocket = 2,
        [Display(Name = "Nagad")] Nagad = 3,
        [Display(Name = "Upay")] Upay = 4
    }
}
