using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagementSystem.Models
{
    [Table("Classes")]
    public class Class
    {
        [Key]
        [StringLength(10)]
        public string ClassId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string ClassName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Major { get; set; } = string.Empty;


        [Required]
        public int SemesterId { get; set; }

        [ForeignKey("SemesterId")]
        public virtual Semester Semester { get; set; } = null!;

        [Column(TypeName = "bit")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
        public virtual ICollection<AnnouncementDetail> AnnouncementDetails { get; set; } = new List<AnnouncementDetail>();
    }
}
