using System.ComponentModel.DataAnnotations;

namespace BOCS.ModelsView
{
    public class SubjectItemVM
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public int SortOrder { get; set; }
        public bool IsPublished { get; set; }
        public int LessonCount { get; set; }
    }
    public class SubjectManageVM
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = "";
        public List<SubjectItemVM> Subjects { get; set; } = new();
    }

    public class SubjectCreateVM
    {
        public int CourseId { get; set; }
        [Required, StringLength(120)]
        public string Title { get; set; } = "";
        public int SortOrder { get; set; } = 0;
        public bool IsPublished { get; set; } = true;
    }

    public class SubjectDeleteVM
    {
        public int CourseId { get; set; }
        public int Id { get; set; }
        public string Title { get; set; } = "";
    }
}
