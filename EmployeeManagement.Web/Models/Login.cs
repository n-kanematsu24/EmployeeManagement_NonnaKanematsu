using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeManagement.Web.Models
{
    [Table("login")]
    public class Login
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("employee_no")]
        public string? EmployeeNo { get; set; }

        [Column("password")]
        public string? Password { get; set; }

        [Column("is_deleted")]
        public bool? IsDeleted { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}