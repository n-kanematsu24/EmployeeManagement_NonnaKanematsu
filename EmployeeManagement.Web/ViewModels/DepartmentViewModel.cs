using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EmployeeManagement.Web.ViewModels
{
    public class DepartmentViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "部門名を入力してください")]
        [Display(Name = "部門名")]
        public string DeptName { get; set; } = string.Empty;

        [Display(Name = "電話番号")]
        public string? Phone { get; set; }

        public List<EmployeeViewModel> Members { get; set; } = new();
        public string DeptCode => $"DEPT{Id:D3}";

        [Display(Name = "部門画像")]
        public IFormFile? ProfileImageFile { get; set; }
        public string? ImagePath { get; set; }
    }
}
