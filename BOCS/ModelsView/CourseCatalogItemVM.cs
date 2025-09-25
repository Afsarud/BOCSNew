using BOCS.Models;

namespace BOCS.ModelsView
{
    public class CourseCatalogItemVM
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string? ThumbnailUrl { get; set; }
        public int DurationDays { get; set; }
        public int PriceBdt { get; set; }
        public int NotificationCount { get; set; } = 0; // চাইলে দেখাবে
        public CourseType CourseType { get; set; }
    }
}
