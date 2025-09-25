using System.ComponentModel.DataAnnotations;

namespace BOCS.Models
{
    public class CourseSubject
    {
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }
        public Course Course { get; set; } = default!;

        [Required, StringLength(120)]
        public string Title { get; set; } = "";

        public int SortOrder { get; set; } = 0;
        public bool IsPublished { get; set; } = true;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public ICollection<CourseLesson> Lessons { get; set; } = new List<CourseLesson>();
    }
}
