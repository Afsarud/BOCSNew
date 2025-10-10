namespace BOCS.Models
{
    public class LessonFile
    {
        public int Id { get; set; }
        public int LessonId { get; set; }
        public string FileType { get; set; } = "";// "Image" or "Document"
        public string FileName { get; set; } = "";
        public string FilePath { get; set; } = "";
        public CourseLesson Lesson { get; set; } = new();
    }
}
