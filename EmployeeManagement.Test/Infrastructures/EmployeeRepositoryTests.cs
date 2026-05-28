using EmployeeManagement.Web.Data;
using EmployeeManagement.Web.Models;
using EmployeeManagement.Web.Repository;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Test.Infrastructures
{
    [TestClass]
    public class EmployeeRepositoryTests
    {
        private static AppDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [TestMethod]
        public async Task GetAllAsync_論理削除されていない社員のみ取得できる()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            context.Employees.Add(new Employee { EmployeeNo = "EMP001", LastName = "山田", FirstName = "太郎", IsDeleted = false });
            context.Employees.Add(new Employee { EmployeeNo = "EMP002", LastName = "佐藤", FirstName = "花子", IsDeleted = false });
            context.Employees.Add(new Employee { EmployeeNo = "EMP999", LastName = "削除", FirstName = "済", IsDeleted = true });
            await context.SaveChangesAsync();

            var repository = new EmployeeRepository(context);

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            Assert.AreEqual(2, result.Count(), "論理削除されていない社員は2件のはず");
            Assert.IsFalse(result.Any(e => e.EmployeeNo == "EMP999"), "削除済み社員は含まれないはず");
        }

        [TestMethod]
        public async Task GetAllWithDepartmentAsync_社員と部門情報が一緒に取得できる()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var dept = new Department { DeptName = "営業部", IsDeleted = false };
            context.Departments.Add(dept);
            await context.SaveChangesAsync();

            context.Employees.Add(new Employee
            {
                EmployeeNo = "EMP001",
                LastName = "山田",
                FirstName = "太郎",
                DeptId = dept.Id,
                IsDeleted = false
            });
            await context.SaveChangesAsync();

            var repository = new EmployeeRepository(context);

            // Act
            var result = await repository.GetAllWithDepartmentAsync();

            // Assert
            Assert.AreEqual(1, result.Count());
            var emp = result.First();
            Assert.IsNotNull(emp.Department, "Department が Include されているはず");
            Assert.AreEqual("営業部", emp.Department.DeptName, "部門名が取得できているはず");
        }

        [TestMethod]
        public async Task GetWithDepartmentAsync_指定社員と部門情報が取得できる()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var dept = new Department { DeptName = "開発部", IsDeleted = false };
            context.Departments.Add(dept);
            await context.SaveChangesAsync();

            var emp = new Employee
            {
                EmployeeNo = "EMP001",
                LastName = "鈴木",
                FirstName = "一郎",
                DeptId = dept.Id,
                IsDeleted = false
            };
            context.Employees.Add(emp);
            await context.SaveChangesAsync();

            var repository = new EmployeeRepository(context);

            // Act
            var result = await repository.GetWithDepartmentAsync(emp.Id);

            // Assert
            Assert.IsNotNull(result, "該当社員が取得できるはず");
            Assert.AreEqual("鈴木", result.LastName);
            Assert.AreEqual("一郎", result.FirstName);
            Assert.IsNotNull(result.Department, "Department が Include されているはず");
            Assert.AreEqual("開発部", result.Department.DeptName);
        }

        [TestMethod]
        public async Task AddAsync_社員が登録されidとタイムスタンプが自動設定される()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var dept = new Department { DeptName = "営業部", IsDeleted = false };
            context.Departments.Add(dept);
            await context.SaveChangesAsync();

            var repository = new EmployeeRepository(context);
            var newEmp = new Employee
            {
                EmployeeNo = "EMP001",
                LastName = "山田",
                FirstName = "太郎",
                Email = "yamada@example.com",
                Phone = "090-1234-5678",
                BirthDate = new DateOnly(1990, 4, 1),
                DeptId = dept.Id,
                Status = 1,
                IsDeleted = false
            };
            var beforeAdd = DateTime.Now;

            // Act
            await repository.AddAsync(newEmp);

            // Assert
            Assert.AreNotEqual(0, newEmp.Id, "Idが自動採番されているはず");
            Assert.IsNotNull(newEmp.CreatedAt, "CreatedAtが自動設定されているはず");
            Assert.IsNotNull(newEmp.UpdatedAt, "UpdatedAtが自動設定されているはず");
            Assert.IsTrue(newEmp.CreatedAt >= beforeAdd);

            // DBから取り直して保存内容を検証
            var saved = await context.Employees.FindAsync(newEmp.Id);
            Assert.IsNotNull(saved);
            Assert.AreEqual("山田", saved.LastName);
            Assert.AreEqual("太郎", saved.FirstName);
            Assert.AreEqual("yamada@example.com", saved.Email);
            Assert.AreEqual(dept.Id, saved.DeptId);
        }

        [TestMethod]
        public async Task UpdateAsync_社員情報が更新されUpdatedAtも更新される()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var emp = new Employee
            {
                EmployeeNo = "EMP001",
                LastName = "山田",
                FirstName = "太郎",
                Email = "yamada@example.com",
                IsDeleted = false,
                CreatedAt = DateTime.Now.AddDays(-1),
                UpdatedAt = DateTime.Now.AddDays(-1)
            };
            context.Employees.Add(emp);
            await context.SaveChangesAsync();
            var originalUpdatedAt = emp.UpdatedAt;

            var repository = new EmployeeRepository(context);

            // Act
            emp.LastName = "佐藤";
            await repository.UpdateAsync(emp);

            // Assert
            var updated = await context.Employees.FindAsync(emp.Id);
            Assert.IsNotNull(updated);
            Assert.AreEqual("佐藤", updated.LastName, "LastNameが更新されているはず");
            Assert.AreEqual("太郎", updated.FirstName, "FirstNameは変わっていないはず");
            Assert.IsTrue(updated.UpdatedAt > originalUpdatedAt, "UpdatedAtが新しい時刻に更新されているはず");
        }

        [TestMethod]
        public async Task DeleteAsync_論理削除されGetAllAsyncに含まれなくなる()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var emp = new Employee
            {
                EmployeeNo = "EMP001",
                LastName = "山田",
                FirstName = "太郎",
                IsDeleted = false
            };
            context.Employees.Add(emp);
            await context.SaveChangesAsync();

            var repository = new EmployeeRepository(context);

            // Act
            await repository.DeleteAsync(emp.Id);

            // Assert
            // GetAllAsync には含まれない
            var allEmps = await repository.GetAllAsync();
            Assert.IsFalse(allEmps.Any(e => e.Id == emp.Id), "削除した社員は一覧に含まれないはず");

            // ただしDBには残っている（物理削除ではなく論理削除）
            var deletedEmp = await context.Employees.FindAsync(emp.Id);
            Assert.IsNotNull(deletedEmp, "レコード自体はDBに残っているはず（論理削除）");
            Assert.IsTrue(deletedEmp.IsDeleted, "IsDeleted フラグが true になっているはず");
        }
    }
}
