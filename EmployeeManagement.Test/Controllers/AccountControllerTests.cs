using EmployeeManagement.Web.Controllers;
using EmployeeManagement.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Test.Controllers
{
    [TestClass]
    public class AccountControllerTests
    {
        private static AccountController CreateController()
        {
            var controller = new AccountController();
            var httpContext = new DefaultHttpContext { Session = new TestSession() };
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            return controller;
        }

        [TestMethod]
        public void Login_正しい認証情報で社員一覧へリダイレクトされる()
        {
            var controller = CreateController();
            var model = new LoginViewModel { LoginId = "admin", Password = "admin123" };

            var result = controller.Login(model);

            var redirect = result as RedirectToActionResult;
            Assert.IsNotNull(redirect);
            Assert.AreEqual("Index", redirect.ActionName);
            Assert.AreEqual("Employee", redirect.ControllerName);
            Assert.AreEqual("true", controller.HttpContext.Session.GetString("logged_in"));
        }

        [TestMethod]
        public void Login_誤ったパスワードでログイン画面に戻りエラーが追加される()
        {
            var controller = CreateController();
            var model = new LoginViewModel { LoginId = "admin", Password = "wrongpassword" };

            var result = controller.Login(model);

            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.IsFalse(controller.ModelState.IsValid);
            Assert.IsTrue(controller.ModelState.ErrorCount > 0);
            Assert.IsNull(controller.HttpContext.Session.GetString("logged_in"));
        }

        [TestMethod]
        public void Login_LoginIdが空でModelState無効になりログイン画面に戻る()
        {
            var controller = CreateController();
            var model = new LoginViewModel { LoginId = "", Password = "admin123" };
            controller.ModelState.AddModelError("LoginId", "ログインIDを入力してください");

            var result = controller.Login(model);

            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.IsFalse(controller.ModelState.IsValid);
            Assert.IsNull(controller.HttpContext.Session.GetString("logged_in"));
        }

        [TestMethod]
        public void Logout_セッションがクリアされログイン画面へリダイレクトされる()
        {
            // Arrange
            var controller = CreateController();
            // 事前にログイン状態にしておく
            controller.HttpContext.Session.SetString("logged_in", "true");
            controller.HttpContext.Session.SetString("login_user", "admin");

            // Act
            var result = controller.Logout();

            // Assert
            var redirect = result as RedirectToActionResult;
            Assert.IsNotNull(redirect, "RedirectToActionResultが返るはず");
            Assert.AreEqual("Login", redirect.ActionName);
            Assert.IsNull(controller.HttpContext.Session.GetString("logged_in"),
                "セッションのlogged_inがクリアされているはず");
            Assert.IsNull(controller.HttpContext.Session.GetString("login_user"),
                "セッションのlogin_userもクリアされているはず");
        }
    }

    public class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();
        public IEnumerable<string> Keys => _store.Keys;
        public string Id => "test-session";
        public bool IsAvailable => true;
        public void Clear() => _store.Clear();
        public Task CommitAsync(CancellationToken token = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken token = default) => Task.CompletedTask;
        public void Remove(string key) => _store.Remove(key);
        public void Set(string key, byte[] value) => _store[key] = value;
        public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value!);
    }
}
