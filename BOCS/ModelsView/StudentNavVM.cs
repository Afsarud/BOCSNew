namespace BOCS.ModelsView
{
    public class StudentNavVM
    {
        public int InProgressCount { get; set; }
    }
    public class MyCourseItemVM
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; } = string.Empty;
        public decimal PriceBdt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsApproved { get; set; }
        public bool IsArchived { get; set; }
    }
}
