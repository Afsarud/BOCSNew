using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BOCS.Models
{
    public class CourseLessonAttachment
    {
        public int Id { get; set; }

        [ForeignKey(nameof(CourseLesson))]
        public int CourseLessonId { get; set; }
        public CourseLesson CourseLesson { get; set; } = default!;
        public AttachmentType AttachmentType { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public string RelativePath { get; set; } = "";
        public string AttatchmentName { get; set; } = "";
        public int Order { get; set; } = 0; 
    }
    public enum AttachmentType
    {
        Image=1,
        Document=2
    }
}
