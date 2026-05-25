using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeManagement.Web.Models
{
    [Table("employee")]
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Column("employee_no")]
        public string? EmployeeNo { get; set; }

        [Column("last_name")]
        public string? LastName { get; set; }

        [Column("first_name")]
        public string? FirstName { get; set; }

        [Column("email")]
        public string? Email { get; set; }
    }
}
