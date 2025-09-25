using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BOCS.Models
{
    public class CourseLesson
    {
        public int Id { get; set; }

        [ForeignKey(nameof(Course))]
        public int CourseId { get; set; }
        public Course Course { get; set; } = default!;

        [Required, StringLength(200)]
        public string Title { get; set; } = "";

        [Required, StringLength(20)]
        public string YoutubeId { get; set; } = "";

        [StringLength(512)]
        public string? YoutubeUrlRaw { get; set; } 

        public int SortOrder { get; set; } = 0;
        
        public bool IsPublished { get; set; } = true;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        // ✅ নতুন
        public int? SubjectId { get; set; }
        public bool IsPlay { get; set; } = false;
        public CourseSubject? Subject { get; set; }
    }
}
