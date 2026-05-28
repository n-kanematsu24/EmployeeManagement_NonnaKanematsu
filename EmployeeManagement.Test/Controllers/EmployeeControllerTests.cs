using EmployeeManagement.Web.Controllers;
using EmployeeManagement.Web.Infrastructures.Repositories;
using EmployeeManagement.Web.Models;
using EmployeeManagement.Web.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace EmployeeManagement.Test.Controllers
{
    [TestClass]
    public class EmployeeControllerTests
    {
        private static (EmployeeController controller, Mock<IEmployeeRepository> empRepo, Mock<IDepartmentRepository> deptRepo)
            CreateController(bool loggedIn = true)
        {
            var empRepo = new Mock<IEmployeeRepository>();
            var deptRepo = new Mock<IDepartmentRepository>();
            var env = new Mock<IWebHostEnvironment>();
            env.Setup(e => e.WebRootPath).Returns("/tmp");

            var controller = new EmployeeController(empRepo.Object, deptRepo.Object, env.Object);
            var httpContext = new DefaultHttpContext { Session = new TestSession() };
            if (loggedIn)
            {
                httpContext.Session.SetString("logged_in", "true");
            }
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // TempData の準備
            var tempDataProvider = new Mock<ITempDataProvider>();
            controller.TempData = new TempDataDictionary(httpContext, tempDataProvider.Object);

            return (controller, empRepo, deptRepo);
        }

        [TestMethod]
        public async Task Index_ログイン中なら全社員のViewModelリストがViewに渡される()
        {
            var (controller, empRepo, _) = CreateController(loggedIn: true);
            var employees = new List<Employee>
            {
                new() { Id = 1, EmployeeNo = "EMP001", LastName = "山田", FirstName = "太郎",
                        Department = new Department { Id = 1, DeptName = "営業部" } },
                new() { Id = 2, EmployeeNo = "EMP002", LastName = "佐藤", FirstName = "花子",
                        Department = new Department { Id = 2, DeptName = "開発部" } }
            };
            empRepo.Setup(r => r.GetAllWithDepartmentAsync()).ReturnsAsync(employees);

            var result = await controller.Index();

            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            var model = viewResult.Model as List<EmployeeViewModel>;
            Assert.IsNotNull(model);
            Assert.AreEqual(2, model.Count);
        }

        [TestMethod]
        public async Task Index_未ログインならログイン画面へリダイレクトされる()
        {
            var (controller, empRepo, _) = CreateController(loggedIn: false);

            var result = await controller.Index();

            var redirect = result as RedirectToActionResult;
            Assert.IsNotNull(redirect);
            Assert.AreEqual("Login", redirect.ActionName);
            Assert.AreEqual("Account", redirect.ControllerName);
            empRepo.Verify(r => r.GetAllWithDepartmentAsync(), Times.Never);
        }

        [TestMethod]
        public async Task Detail_ログイン中で存在するIDなら該当社員のViewが返る()
        {
            var (controller, empRepo, _) = CreateController(loggedIn: true);
            var employee = new Employee
            {
                Id = 1, EmployeeNo = "EMP001", LastName = "山田", FirstName = "太郎",
                Email = "yamada@example.com",
                Department = new Department { Id = 1, DeptName = "営業部" }
            };
            empRepo.Setup(r => r.GetWithDepartmentAsync(1)).ReturnsAsync(employee);

            var result = await controller.Detail(1);

            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            var model = viewResult.Model as EmployeeViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Id);
            Assert.AreEqual("山田", model.LastName);
        }

        [TestMethod]
        public async Task Detail_存在しないIDならNotFoundが返る()
        {
            var (controller, empRepo, _) = CreateController(loggedIn: true);
            empRepo.Setup(r => r.GetWithDepartmentAsync(99999)).ReturnsAsync((Employee?)null);

            var result = await controller.Detail(99999);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Create_POST_正常な入力で社員が登録されIndexへリダイレクト()
        {
            // Arrange
            var (controller, empRepo, deptRepo) = CreateController(loggedIn: true);
            deptRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Department>());

            var model = new EmployeeViewModel
            {
                EmployeeNo = "EMP010",
                LastName = "鈴木",
                FirstName = "一郎",
                Email = "suzuki@example.com",
                Phone = "090-0000-0000",
                BirthDate = new DateOnly(1985, 5, 10),
                DeptId = 1,
                Status = 1
            };

            // Act
            var result = await controller.Create(model);

            // Assert
            var redirect = result as RedirectToActionResult;
            Assert.IsNotNull(redirect, "RedirectToActionResult が返るはず");
            Assert.AreEqual("Index", redirect.ActionName);

            // Repository.AddAsync が呼ばれたことを検証
            empRepo.Verify(r => r.AddAsync(It.Is<Employee>(e =>
                e.LastName == "鈴木" && e.FirstName == "一郎"
            )), Times.Once, "AddAsync が正しい引数で1回呼ばれるはず");

            // TempData に成功メッセージが入っているはず
            Assert.IsNotNull(controller.TempData["Success"]);
        }

        [TestMethod]
        public async Task Create_POST_姓が空でModelState無効ならViewが返りAddAsyncは呼ばれない()
        {
            // Arrange
            var (controller, empRepo, deptRepo) = CreateController(loggedIn: true);
            deptRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Department>());

            var model = new EmployeeViewModel
            {
                EmployeeNo = "EMP010",
                LastName = "",   // 空
                FirstName = "一郎",
                Email = "suzuki@example.com",
                Phone = "090-0000-0000",
                BirthDate = new DateOnly(1985, 5, 10),
                DeptId = 1,
                Status = 1
            };
            // [Required] の効果を再現
            controller.ModelState.AddModelError("LastName", "姓を入力してください");

            // Act
            var result = await controller.Create(model);

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult, "ViewResult (登録画面に戻る) のはず");
            Assert.IsFalse(controller.ModelState.IsValid);

            // Repository は呼ばれない
            empRepo.Verify(r => r.AddAsync(It.IsAny<Employee>()), Times.Never,
                "ModelState無効時は AddAsync が呼ばれないはず");
        }

        [TestMethod]
        public async Task Create_POST_メール形式不正でModelState無効ならViewが返る()
        {
            // Arrange
            var (controller, empRepo, deptRepo) = CreateController(loggedIn: true);
            deptRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Department>());

            var model = new EmployeeViewModel
            {
                EmployeeNo = "EMP010",
                LastName = "鈴木",
                FirstName = "一郎",
                Email = "not-an-email",   // 不正な形式
                Phone = "090-0000-0000",
                BirthDate = new DateOnly(1985, 5, 10),
                DeptId = 1,
                Status = 1
            };
            // [EmailAddress] の効果を再現
            controller.ModelState.AddModelError("Email", "正しいメールアドレス形式で入力してください");

            // Act
            var result = await controller.Create(model);

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult, "ViewResult (登録画面に戻る) のはず");
            Assert.IsTrue(controller.ModelState["Email"]?.Errors.Count > 0,
                "Email にエラーが追加されているはず");
            empRepo.Verify(r => r.AddAsync(It.IsAny<Employee>()), Times.Never);
        }

        [TestMethod]
        public async Task Create_POST_姓が30文字ちょうどでも正常に登録される()
        {
            // Arrange
            var (controller, empRepo, deptRepo) = CreateController(loggedIn: true);
            deptRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Department>());

            var thirtyCharLastName = new string('山', 30);  // "山"を30回繰り返した文字列
            Assert.AreEqual(30, thirtyCharLastName.Length, "テストデータ自体が30文字であることを確認");

            var model = new EmployeeViewModel
            {
                EmployeeNo = "EMP010",
                LastName = thirtyCharLastName,
                FirstName = "一郎",
                Email = "suzuki@example.com",
                Phone = "090-0000-0000",
                BirthDate = new DateOnly(1985, 5, 10),
                DeptId = 1,
                Status = 1
            };

            // Act
            var result = await controller.Create(model);

            // Assert
            var redirect = result as RedirectToActionResult;
            Assert.IsNotNull(redirect, "正常登録されてIndexへリダイレクトするはず");
            Assert.AreEqual("Index", redirect.ActionName);

            // 30文字のLastNameでAddAsyncが呼ばれたことを検証
            empRepo.Verify(r => r.AddAsync(It.Is<Employee>(e =>
                e.LastName == thirtyCharLastName && e.LastName!.Length == 30
            )), Times.Once);
        }

        [TestMethod]
        public async Task Create_POST_プロフィール画像が添付されていればファイル保存処理が走る()
        {
            // Arrange
            var (controller, empRepo, deptRepo) = CreateController(loggedIn: true);
            deptRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Department>());

            // AddAsync が呼ばれたら Id=42 が採番されたことにする
            empRepo.Setup(r => r.AddAsync(It.IsAny<Employee>()))
                   .Callback<Employee>(e => e.Id = 42)
                   .Returns(Task.CompletedTask);

            // IFormFile のモック作成（テスト用の擬似画像ファイル）
            var formFile = new Mock<IFormFile>();
            var content = "fake image content";
            var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            formFile.Setup(f => f.Length).Returns(ms.Length);
            formFile.Setup(f => f.FileName).Returns("photo.png");
            formFile.Setup(f => f.OpenReadStream()).Returns(ms);
            formFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                    .Returns<Stream, CancellationToken>((stream, _) => ms.CopyToAsync(stream));

            // 保存先ディレクトリをテスト用一時パスに切り替え
            var tempDir = Path.Combine(Path.GetTempPath(), "emp_test_" + Guid.NewGuid());
            Directory.CreateDirectory(tempDir);
            var env = new Mock<IWebHostEnvironment>();
            env.Setup(e => e.WebRootPath).Returns(tempDir);
            // Controller を作り直して env を差し替え
            var newController = new EmployeeController(empRepo.Object, deptRepo.Object, env.Object);
            newController.ControllerContext = controller.ControllerContext;
            newController.TempData = controller.TempData;

            var model = new EmployeeViewModel
            {
                EmployeeNo = "EMP010",
                LastName = "鈴木",
                FirstName = "一郎",
                Email = "suzuki@example.com",
                Phone = "090-0000-0000",
                BirthDate = new DateOnly(1985, 5, 10),
                DeptId = 1,
                Status = 1,
                ProfileImageFile = formFile.Object
            };

            try
            {
                // Act
                var result = await newController.Create(model);

                // Assert
                var redirect = result as RedirectToActionResult;
                Assert.IsNotNull(redirect);

                // 画像ファイルが保存されたパスを確認
                var expectedPath = Path.Combine(tempDir, "img", "employees", "emp_42.png");
                Assert.IsTrue(File.Exists(expectedPath),
                    $"画像が {expectedPath} に保存されているはず");

                // CopyToAsync が呼ばれたことを検証
                formFile.Verify(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()),
                                Times.Once);
            }
            finally
            {
                // 後片付け：テストで作ったテンポラリディレクトリを削除
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, recursive: true);
            }
        }

        [TestMethod]
        public async Task Edit_POST_正常な入力で社員が更新されDetailへリダイレクトされる()
        {
            // Arrange
            var (controller, empRepo, deptRepo) = CreateController(loggedIn: true);
            deptRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Department>());

            // 既存社員データを返すように設定
            var existing = new Employee
            {
                Id = 1,
                EmployeeNo = "EMP001",
                LastName = "山田",
                FirstName = "太郎",
                Email = "yamada@example.com",
                Status = 1,
                IsDeleted = false
            };
            empRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);

            // 編集後のデータ
            var model = new EmployeeViewModel
            {
                Id = 1,
                EmployeeNo = "EMP001",
                LastName = "佐藤",            // 姓を変更
                FirstName = "太郎",
                Email = "sato@example.com",   // メールも変更
                Phone = "090-0000-0000",
                BirthDate = new DateOnly(1985, 5, 10),
                DeptId = 1,
                Status = 1
            };

            // Act
            var result = await controller.Edit(1, model);

            // Assert
            var redirect = result as RedirectToActionResult;
            Assert.IsNotNull(redirect, "RedirectToActionResult が返るはず");
            Assert.AreEqual("Detail", redirect.ActionName, "Detail へリダイレクトするはず");
            Assert.AreEqual(1, redirect.RouteValues?["id"], "Detail に id=1 が渡されるはず");

            // UpdateAsync が呼ばれて、姓とメールが変更されていることを検証
            empRepo.Verify(r => r.UpdateAsync(It.Is<Employee>(e =>
                e.LastName == "佐藤" && e.Email == "sato@example.com" && e.Id == 1
            )), Times.Once);

            // 成功メッセージが入っているはず
            Assert.IsNotNull(controller.TempData["Success"]);
        }

        [TestMethod]
        public async Task Delete_POST_存在する社員IDで論理削除されIndexへリダイレクトされる()
        {
            // Arrange
            var (controller, empRepo, _) = CreateController(loggedIn: true);
            var existing = new Employee
            {
                Id = 1,
                EmployeeNo = "EMP001",
                LastName = "山田",
                FirstName = "太郎",
                IsDeleted = false
            };
            empRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);

            // Act
            var result = await controller.Delete(1);

            // Assert
            var redirect = result as RedirectToActionResult;
            Assert.IsNotNull(redirect, "RedirectToActionResult が返るはず");
            Assert.AreEqual("Index", redirect.ActionName);

            // DeleteAsync が id=1 で呼ばれたことを検証
            empRepo.Verify(r => r.DeleteAsync(1), Times.Once,
                "DeleteAsync(1) が1回呼ばれるはず");

            // 成功メッセージが入っているはず
            Assert.IsNotNull(controller.TempData["Success"]);
            // メッセージに名前が含まれていることを確認
            Assert.IsTrue(controller.TempData["Success"]!.ToString()!.Contains("山田"),
                "成功メッセージに社員名が含まれているはず");
        }
    }
}
