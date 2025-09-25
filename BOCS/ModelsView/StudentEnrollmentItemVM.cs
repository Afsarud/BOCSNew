namespace BOCS.ModelsView
{
    public class StudentEnrollmentItemVM
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = "";
        public string? ThumbnailUrl { get; set; }
        public decimal PriceBdt { get; set; }

        public bool IsApproved { get; set; }
        public bool IsArchived { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
