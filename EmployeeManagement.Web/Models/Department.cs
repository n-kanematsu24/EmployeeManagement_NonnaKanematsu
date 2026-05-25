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
    }
}