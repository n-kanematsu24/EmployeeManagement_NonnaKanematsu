using System.ComponentModel.DataAnnotations;
using System.Globalization;
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

        [Display(Name = "Last Name (English)")]
        public string? LastNameEn { get; set; }

        [Display(Name = "First Name (English)")]
        public string? FirstNameEn { get; set; }

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
        public string? DeptNameEn { get; set; }

        [Required(ErrorMessage = "在籍状態を選択してください")]
        [Display(Name = "在籍状態")]
        public int Status { get; set; }

        [Display(Name = "プロフィール画像")]
        public IFormFile? ProfileImageFile { get; set; }
        public string? ProfileImagePath { get; set; }

        // 更新日時（並び替え用）
        public DateTime? UpdatedAt { get; set; }

        // 最終更新者ID（adminの場合はnull）
        public int? UpdatedId { get; set; }

        // 最終更新者名（表示用：admin/null の場合は「システム管理者」）
        public string? UpdatedByName { get; set; }  // null なら View 側で「システム管理者」と翻訳表示

        // 日本語フルネーム
        public string FullName => $"{LastName} {FirstName}".Trim();

        // 英語フルネーム
        public string? FullNameEn =>
            !string.IsNullOrWhiteSpace(FirstNameEn)
                ? $"{FirstNameEn} {LastNameEn}".Trim()
                : null;

        // 現在の言語に応じた名前
        public string LocalizedFullName
        {
            get
            {
                var culture = CultureInfo.CurrentUICulture.Name;
                if (!culture.StartsWith("ja") && FullNameEn != null)
                    return FullNameEn;
                return FullName;
            }
        }

        // 現在の言語に応じた部門名
        public string? LocalizedDeptName
        {
            get
            {
                var culture = CultureInfo.CurrentUICulture.Name;
                if (!culture.StartsWith("ja") && !string.IsNullOrWhiteSpace(DeptNameEn))
                    return DeptNameEn;
                return DeptName;
            }
        }

        public string StatusCode => Status switch
        {
            1 => "Active",
            2 => "Leave",
            3 => "Resigned",
            _ => "Unknown"
        };

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
