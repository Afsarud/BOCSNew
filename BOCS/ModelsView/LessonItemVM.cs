
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
    }
    public class TickDto
    {
        public List<int> Ids { get; set; } = new();
        public bool Value { get; set; }     // true = 1, false = 0
    }
}
