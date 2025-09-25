using System.ComponentModel.DataAnnotations;

namespace BOCS.ModelsView
{
    public class LessonCreateVM
    {
        public int CourseId { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; } = "";

        [Display(Name = "YouTube URL or ID")]
        [Required, StringLength(512)]
        public string YoutubeUrlOrId { get; set; } = "";

        [Display(Name = "Sort order")]
        //public int SortOrder { get; set; } = 0;
        public int SortOrder { get; set; }

        [Display(Name = "Published")]
        public bool IsPublished { get; set; } = true;
        // ✅ নতুন subject
        public int? SubjectId { get; set; }
    }
}
