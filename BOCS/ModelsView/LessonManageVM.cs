
namespace BOCS.ModelsView
{
    public class LessonManageVM
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = "";
        public List<LessonItemVM> Lessons { get; set; } = new();
    }
}
