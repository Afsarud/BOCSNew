using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

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
        public int SortOrder { get; set; }

        [Display(Name = "Published")]
        public bool IsPublished { get; set; } = true;
        public int? SubjectId { get; set; }

        [Display(Name = "Lesson Images")]
        public IFormFileCollection? LessonImages { get; set; }

        [Display(Name = "Lesson Documents")]
        public IFormFileCollection? LessonDocuments { get; set; }
        public List<AttachmentDisplayVM> ExistingImages { get; set; } = new();
        public List<AttachmentDisplayVM> ExistingDocuments { get; set; } = new();
    }

    public class AttachmentDisplayVM
    {
        public int Id { get; set; }
        public int CourseLessonId { get; set; }
        public string FileName { get; set; } = "";
        public string FilePath { get; set; } = "";
        public string FileSize { get; set; } = "";
    }
}
