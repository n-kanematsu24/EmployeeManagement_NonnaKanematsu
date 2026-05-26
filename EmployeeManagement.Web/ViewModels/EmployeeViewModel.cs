using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EmployeeManagement.Web.ViewModels
{
    public class EmployeeViewModel
    {
        public int Id { get; set; }

        [Display(Name = "社員番号")]
        public string? EmployeeNo { get; set; }

        [Required(ErrorMessage = "姓を入力してください")]
        [Display(Name = "姓")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "名を入力してください")]
        [Display(Name = "名")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "メールアドレスを入力してください")]
        [EmailAddress(ErrorMessage = "正しいメールアドレス形式で入力してください")]
        [Display(Name = "メールアドレス")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "電話番号を入力してください")]
        [Display(Name = "電話番号")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "入社日")]
        public DateOnly? HireDate { get; set; }

        [Required(ErrorMessage = "生年月日を入力してください")]
        [Display(Name = "生年月日")]
        public DateOnly? BirthDate { get; set; }

        [Required(ErrorMessage = "部門を選択してください")]
        [Display(Name = "部門")]
        public int DeptId { get; set; }

        public string? DeptName { get; set; }

        [Required(ErrorMessage = "在籍状態を選択してください")]
        [Display(Name = "在籍状態")]
        public int Status { get; set; }

        // プロフィール画像
        [Display(Name = "プロフィール画像")]
        public IFormFile? ProfileImageFile { get; set; }

        public string? ProfileImagePath { get; set; }

        public string FullName => $"{LastName} {FirstName}".Trim();

        public string StatusText => Status switch
        {
            1 => "在籍",
            2 => "休職",
            3 => "退職",
            _ => "不明"
        };

        public string StatusColor => Status switch
        {
            1 => "#2D7A3A",
            2 => "#A07800",
            3 => "#888888",
            _ => "#999999"
        };

        public string StatusBg => Status switch
        {
            1 => "#EAF4EA",
            2 => "#FFF8E1",
            3 => "#F5F5F5",
            _ => "#F5F5F5"
        };
    }
}
