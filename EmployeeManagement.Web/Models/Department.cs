using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeManagement.Web.Models
{
    [Table("department")]
    public class Department
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("dept_name")]
        public string? DeptName { get; set; }

        [Column("phone")]
        public string? Phone { get; set; }

        [Column("is_deleted")]
        public bool? IsDeleted { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("updated_id")]
        public int? UpdatedId { get; set; }

        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
