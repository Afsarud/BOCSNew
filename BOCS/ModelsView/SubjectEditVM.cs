using System.ComponentModel.DataAnnotations;

namespace BOCS.ModelsView
{
    public class SubjectEditVM
    {
        [Required]
        public int CourseId { get; set; }

        [Required]
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; } = default!;

        [Range(0, 9999)]
        public int SortOrder { get; set; }

        public bool IsPublished { get; set; }
    }
}
