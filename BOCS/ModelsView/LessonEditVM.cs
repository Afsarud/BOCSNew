namespace BOCS.ModelsView
{
    public class LessonEditVM
    {
        public int CourseId { get; set; }
        public int Id { get; set; }

        public string Title { get; set; } = default!;
        public string YoutubeUrlOrId { get; set; } = default!;
        public int SortOrder { get; set; }
        public bool IsPublished { get; set; }

        public int? SubjectId { get; set; }
    }
}
