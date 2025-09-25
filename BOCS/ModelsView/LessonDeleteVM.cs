namespace BOCS.ModelsView
{
    public class LessonDeleteVM
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = "";
        public string Title { get; set; } = "";
        public string? YoutubeId { get; set; }
        public int SortOrder { get; set; }
        public bool IsPublished { get; set; }
    }
}
