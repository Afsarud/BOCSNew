
namespace BOCS.ModelsView
{
    public class LessonItemVM
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string YoutubeId { get; set; } = "";
        public int SortOrder { get; set; }
        public bool IsPublished { get; set; }
        public bool IsPlay { get; set; }
        public DateTime? CreatedAtUtc { get; set; }
        public int ImageCount { get; set; }
        public int FileCount { get; set; }
        public string SubjectName { get; set; } = string.Empty;
    }
    public class TickDto
    {
        public List<int> Ids { get; set; } = new();
        public bool Value { get; set; }     // true = 1, false = 0
    }
}
