using BOCS.Models;

namespace BOCS.ModelsView
{
    public class CourseMiniVM
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public bool IsActive { get; set; }
        public int LessonCount { get; set; }
        public int SubjectCount { get; set; }
        public CourseType CourseType { get; set; } 
    }
}
