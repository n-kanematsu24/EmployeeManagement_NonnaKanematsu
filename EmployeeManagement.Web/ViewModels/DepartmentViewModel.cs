using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Http;

namespace EmployeeManagement.Web.ViewModels
{
    public class DepartmentViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "部門名を入力してください")]
        [Display(Name = "部門名")]
        public string DeptName { get; set; } = string.Empty;

        [Display(Name = "Department Name (English)")]
        public string? DeptNameEn { get; set; }

        [Display(Name = "電話番号")]
        public string? Phone { get; set; }

        public List<EmployeeViewModel> Members { get; set; } = new();
        public string DeptCode => $"DEPT{Id:D3}";

        [Display(Name = "部門画像")]
        public IFormFile? ProfileImageFile { get; set; }
        public string? ImagePath { get; set; }

        // システム情報（表示用）
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedId { get; set; }
        public string? UpdatedByName { get; set; }  // null なら View 側で「システム管理者」と翻訳表示

        // 現在の言語に応じた部門名
        public string LocalizedDeptName
        {
            get
            {
                var culture = CultureInfo.CurrentUICulture.Name;
                if (!culture.StartsWith("ja") && !string.IsNullOrWhiteSpace(DeptNameEn))
                    return DeptNameEn;
                return DeptName;
            }
        }
    }
}
