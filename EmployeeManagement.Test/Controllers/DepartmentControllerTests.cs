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
    public class DepartmentControllerTests
    {
        private static (DepartmentController controller,
                        Mock<IDepartmentRepository> deptRepo,
                        Mock<IEmployeeRepository> empRepo)
            CreateController(bool loggedIn = true)
        {
            var deptRepo = new Mock<IDepartmentRepository>();
            var empRepo = new Mock<IEmployeeRepository>();
            var env = new Mock<IWebHostEnvironment>();
            env.Setup(e => e.WebRootPath).Returns("/tmp");

            var controller = new DepartmentController(deptRepo.Object, empRepo.Object, env.Object);
            var httpContext = new DefaultHttpContext { Session = new TestSession() };
            if (loggedIn)
            {
                httpContext.Session.SetString("logged_in", "true");
            }
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var tempDataProvider = new Mock<ITempDataProvider>();
            controller.TempData = new TempDataDictionary(httpContext, tempDataProvider.Object);

            return (controller, deptRepo, empRepo);
        }

        [TestMethod]
        public async Task Index_ログイン中なら全部門のViewModelリストがViewに渡される()
        {
            // Arrange
            var (controller, deptRepo, _) = CreateController(loggedIn: true);
            var depts = new List<Department>
            {
                new() { Id = 1, DeptName = "営業部", IsDeleted = false },
                new() { Id = 2, DeptName = "開発部", IsDeleted = false },
                new() { Id = 3, DeptName = "経理部", IsDeleted = false }
            };
            deptRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(depts);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult, "ViewResult が返るはず");
            var model = viewResult.Model as List<DepartmentViewModel>;
            Assert.IsNotNull(model, "Model は List<DepartmentViewModel> のはず");
            Assert.AreEqual(3, model.Count);
            Assert.AreEqual("営業部", model[0].DeptName);
        }

        [TestMethod]
        public async Task Detail_該当部門と所属社員一覧が表示される()
        {
            // Arrange
            var (controller, deptRepo, empRepo) = CreateController(loggedIn: true);
            var dept = new Department { Id = 1, DeptName = "営業部", IsDeleted = false };
            deptRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dept);

            var allEmps = new List<Employee>
            {
                new() { Id = 10, EmployeeNo = "EMP010", LastName = "山田", FirstName = "太郎", DeptId = 1 },
                new() { Id = 11, EmployeeNo = "EMP011", LastName = "佐藤", FirstName = "花子", DeptId = 1 },
                new() { Id = 20, EmployeeNo = "EMP020", LastName = "鈴木", FirstName = "一郎", DeptId = 2 }
            };
            empRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(allEmps);

            // Act
            var result = await controller.Detail(1);

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            var model = viewResult.Model as DepartmentViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual("営業部", model.DeptName);

            // 部門ID=1 に所属する社員のみがメンバーに含まれているはず
            Assert.AreEqual(2, model.Members.Count, "営業部のメンバーは2名のはず");
            Assert.IsTrue(model.Members.All(m => m.DeptId == 1),
                "全メンバーが DeptId=1 のはず");
            Assert.IsFalse(model.Members.Any(m => m.EmployeeNo == "EMP020"),
                "他部門の社員は含まれないはず");
        }

        [TestMethod]
        public async Task Create_POST_正常な入力で部門が登録されIndexへリダイレクト()
        {
            // Arrange
            var (controller, deptRepo, _) = CreateController(loggedIn: true);
            var model = new DepartmentViewModel
            {
                DeptName = "新規部門",
                Phone = "03-1111-2222"
            };

            // Act
            var result = await controller.Create(model);

            // Assert
            var redirect = result as RedirectToActionResult;
            Assert.IsNotNull(redirect, "RedirectToActionResult が返るはず");
            Assert.AreEqual("Index", redirect.ActionName);

            // AddAsync が正しい引数で呼ばれたことを検証
            deptRepo.Verify(r => r.AddAsync(It.Is<Department>(d =>
                d.DeptName == "新規部門" && d.Phone == "03-1111-2222"
            )), Times.Once);
        }

        [TestMethod]
        public async Task Create_POST_部門名が空でModelState無効ならViewが返りAddAsyncは呼ばれない()
        {
            // Arrange
            var (controller, deptRepo, _) = CreateController(loggedIn: true);
            var model = new DepartmentViewModel
            {
                DeptName = "",   // 空
                Phone = "03-1111-2222"
            };
            // [Required] の効果を再現
            controller.ModelState.AddModelError("DeptName", "部門名を入力してください");

            // Act
            var result = await controller.Create(model);

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult, "ViewResult (登録画面に戻る) のはず");
            Assert.IsFalse(controller.ModelState.IsValid);

            // AddAsync は呼ばれていないはず
            deptRepo.Verify(r => r.AddAsync(It.IsAny<Department>()), Times.Never,
                "ModelState無効時は AddAsync が呼ばれないはず");
        }

        [TestMethod]
        public async Task Edit_POST_部門名が更新されDetailへリダイレクトされる()
        {
            // Arrange
            var (controller, deptRepo, _) = CreateController(loggedIn: true);
            var existing = new Department
            {
                Id = 1,
                DeptName = "営業部",
                Phone = "03-1111-1111",
                IsDeleted = false
            };
            deptRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);

            var model = new DepartmentViewModel
            {
                Id = 1,
                DeptName = "営業統括部",       // 部門名を変更
                Phone = "03-2222-2222"        // 電話番号も変更
            };

            // Act
            var result = await controller.Edit(1, model);

            // Assert
            var redirect = result as RedirectToActionResult;
            Assert.IsNotNull(redirect, "RedirectToActionResult が返るはず");
            Assert.AreEqual("Detail", redirect.ActionName);
            Assert.AreEqual(1, redirect.RouteValues?["id"]);

            // UpdateAsync が変更後の値で呼ばれたことを検証
            deptRepo.Verify(r => r.UpdateAsync(It.Is<Department>(d =>
                d.DeptName == "営業統括部" && d.Phone == "03-2222-2222" && d.Id == 1
            )), Times.Once);
        }

        [TestMethod]
        public async Task Delete_POST_存在する部門IDで論理削除されIndexへリダイレクトされる()
        {
            // Arrange
            var (controller, deptRepo, _) = CreateController(loggedIn: true);
            var existing = new Department
            {
                Id = 1,
                DeptName = "営業部",
                IsDeleted = false
            };
            deptRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);

            // Act
            var result = await controller.Delete(1);

            // Assert
            var redirect = result as RedirectToActionResult;
            Assert.IsNotNull(redirect, "RedirectToActionResult が返るはず");
            Assert.AreEqual("Index", redirect.ActionName);

            // DeleteAsync が id=1 で呼ばれたことを検証
            deptRepo.Verify(r => r.DeleteAsync(1), Times.Once,
                "DeleteAsync(1) が1回呼ばれるはず");
        }
    }
}
