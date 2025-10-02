using BOCS.Models;
using BOCS.ModelsView;

namespace BOCS.Services
{
    public class FileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string _uploadPath;

        public FileUploadService(IWebHostEnvironment environment)
        {
            _environment = environment;
            _uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "lesson-attachments");
            
            // Create directory if it doesn't exist
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<List<CourseLessonAttachment>> SaveFilesAsync(
            IFormFileCollection? files, 
            int lessonId, 
            AttachmentType attachmentType)
        {
            var attachments = new List<CourseLessonAttachment>();

            if (files == null || files.Count == 0)
                return attachments;

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    // Generate unique filename
                    var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                    var filePath = Path.Combine(_uploadPath, fileName);

                    // Save file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Create attachment record
                    var attachment = new CourseLessonAttachment
                    {
                        CourseLessonId = lessonId,
                        AttachmentType = attachmentType,
                        AttatchmentName = file.FileName,
                        RelativePath = $"/uploads/lesson-attachments/{fileName}",
                        CreatedAtUtc = DateTime.UtcNow,
                        Order = 0
                    };

                    attachments.Add(attachment);
                }
            }

            return attachments;
        }

        public void DeleteFile(string relativePath)
        {
            var fullPath = Path.Combine(_environment.WebRootPath, relativePath.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        public string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        public List<AttachmentDisplayVM> GetAttachmentDisplayVMs(List<CourseLessonAttachment> attachments)
        {
            return attachments.Select(a => new AttachmentDisplayVM
            {
                Id = a.Id,
                FileName = a.AttatchmentName,
                FilePath = a.RelativePath,
                FileSize = GetFileSizeFromPath(a.RelativePath)
            }).ToList();
        }

        private string GetFileSizeFromPath(string relativePath)
        {
            var fullPath = Path.Combine(_environment.WebRootPath, relativePath.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                var fileInfo = new FileInfo(fullPath);
                return FormatFileSize(fileInfo.Length);
            }
            return "Unknown";
        }
    }
}
