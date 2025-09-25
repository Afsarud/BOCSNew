namespace BOCS.ModelsView
{
    public class CourseListItemVM
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string? Summary { get; set; }
        public string? TeacherName { get; set; }
        public string? ThumbnailUrl { get; set; }
        public bool IsEnrolled { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsNew { get; set; }  // চাইলে “New” ব্যাজের জন্য
    }
}
