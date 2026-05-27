using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeManagement.Web.Models
{
    [Table("employee")]
    public class Employee
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("employee_no")]
        public string? EmployeeNo { get; set; }

        [Column("last_name")]
        public string? LastName { get; set; }

        [Column("first_name")]
        public string? FirstName { get; set; }

        [Column("last_name_en")]
        public string? LastNameEn { get; set; }

        [Column("first_name_en")]
        public string? FirstNameEn { get; set; }

        [Column("birth_date")]
        public DateOnly? BirthDate { get; set; }

        [Column("phone")]
        public string? Phone { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("hire_date")]
        public DateOnly? HireDate { get; set; }

        [Column("dept_id")]
        public int? DeptId { get; set; }

        [Column("status")]
        public int? Status { get; set; }

        [Column("is_deleted")]
        public bool? IsDeleted { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("updated_id")]
        public int? UpdatedId { get; set; }

        [ForeignKey("DeptId")]
        public Department? Department { get; set; }
    }
}
