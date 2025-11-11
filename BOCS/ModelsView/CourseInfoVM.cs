namespace BOCS.ModelsView
{
    public class CourseInfoVM
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string? ThumbnailUrl { get; set; }
        public string? LatestYoutubeId { get; set; }
        public int firstLessionId { get; set; } = 0;
        public int DurationDays { get; set; } = 180;
        public int PriceBdt { get; set; } = 15000;
        public string? CreatedBy { get; set; } = "Admin";

        public List<OutlineGroupVM> Outlines { get; set; } = new();
        public List<string> LessonIds { get; set; } = new();
        public List<AttachmentDisplayVM> CourseImages { get; set; } = new();
        public List<AttachmentDisplayVM> CourseDocuments { get; set; } = new();
    }
    public class OutlineGroupVM
    {
        public string Title { get; set; } = "";
        public List<OutlineItemVM> Items { get; set; } = new();
    }

    public class OutlineItemVM
    {
        public string Label { get; set; } = ""; 
        public string YoutubeId { get; set; } = ""; 
        public string LessionId { get; set; } = ""; 
    }
}
