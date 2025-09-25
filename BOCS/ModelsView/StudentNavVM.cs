namespace BOCS.ModelsView
{
    public class StudentNavVM
    {
        public int InProgressCount { get; set; }
    }
    public class MyCourseItemVM
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = "";
        public string? ThumbnailUrl { get; set; }
        public decimal PriceBdt { get; set; }
    }
}
