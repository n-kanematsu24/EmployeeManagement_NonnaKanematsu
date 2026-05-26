using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Web.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "ログインIDを入力してください")]
        [Display(Name = "ログインID")]
        public string LoginId { get; set; } = string.Empty;

        [Required(ErrorMessage = "パスワードを入力してください")]
        [DataType(DataType.Password)]
        [Display(Name = "パスワード")]
        public string Password { get; set; } = string.Empty;
    }
}
