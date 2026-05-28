using EmployeeManagement.Web.Data;
using EmployeeManagement.Web.Models;
using EmployeeManagement.Web.Repository;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Test.Infrastructures
{
    [TestClass]
    public class DepartmentRepositoryTests
    {
        private static AppDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [TestMethod]
        public async Task GetAllAsync_論理削除されていない部門のみ取得できる()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            context.Departments.Add(new Department { DeptName = "営業部", IsDeleted = false });
            context.Departments.Add(new Department { DeptName = "開発部", IsDeleted = false });
            context.Departments.Add(new Department { DeptName = "廃止部", IsDeleted = true });
            await context.SaveChangesAsync();

            var repository = new DepartmentRepository(context);

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            Assert.AreEqual(2, result.Count(), "論理削除されていない部門は2件のはず");
            Assert.IsFalse(result.Any(d => d.DeptName == "廃止部"), "廃止部は含まれないはず");
        }

        [TestMethod]
        public async Task GetByIdAsync_存在する部門IDで該当部門が取得できる()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var dept = new Department { DeptName = "営業部", IsDeleted = false };
            context.Departments.Add(dept);
            await context.SaveChangesAsync();
            var targetId = dept.Id;  // 自動採番されたID

            var repository = new DepartmentRepository(context);

            // Act
            var result = await repository.GetByIdAsync(targetId);

            // Assert
            Assert.IsNotNull(result, "該当部門が取得できるはず");
            Assert.AreEqual("営業部", result.DeptName);
            Assert.AreEqual(targetId, result.Id);
        }

        [TestMethod]
        public async Task GetByIdAsync_存在しない部門IDでnullが返る()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            context.Departments.Add(new Department { DeptName = "営業部", IsDeleted = false });
            await context.SaveChangesAsync();

            var repository = new DepartmentRepository(context);

            // Act
            var result = await repository.GetByIdAsync(99999);

            // Assert
            Assert.IsNull(result, "存在しないIDの場合はnullが返るはず");
        }

        [TestMethod]
        public async Task AddAsync_部門が登録されidとタイムスタンプが自動設定される()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var repository = new DepartmentRepository(context);
            var newDept = new Department
            {
                DeptName = "営業部",
                Phone = "03-1234-5678",
                IsDeleted = false
            };
            var beforeAdd = DateTime.Now;

            // Act
            await repository.AddAsync(newDept);

            // Assert
            Assert.AreNotEqual(0, newDept.Id, "Idが自動採番されているはず");
            Assert.IsNotNull(newDept.CreatedAt, "CreatedAtが自動設定されているはず");
            Assert.IsNotNull(newDept.UpdatedAt, "UpdatedAtが自動設定されているはず");
            Assert.IsTrue(newDept.CreatedAt >= beforeAdd, "CreatedAtは現在時刻付近のはず");

            // DBから取り直しても残っているか確認
            var saved = await context.Departments.FindAsync(newDept.Id);
            Assert.IsNotNull(saved);
            Assert.AreEqual("営業部", saved.DeptName);
            Assert.AreEqual("03-1234-5678", saved.Phone);
        }

        [TestMethod]
        public async Task UpdateAsync_部門名が更新されUpdatedAtも更新される()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var dept = new Department
            {
                DeptName = "営業部",
                IsDeleted = false,
                CreatedAt = DateTime.Now.AddDays(-1),
                UpdatedAt = DateTime.Now.AddDays(-1)
            };
            context.Departments.Add(dept);
            await context.SaveChangesAsync();
            var originalUpdatedAt = dept.UpdatedAt;

            var repository = new DepartmentRepository(context);

            // Act
            dept.DeptName = "開発部";
            await repository.UpdateAsync(dept);

            // Assert
            var updated = await context.Departments.FindAsync(dept.Id);
            Assert.IsNotNull(updated);
            Assert.AreEqual("開発部", updated.DeptName, "DeptNameが更新されているはず");
            Assert.IsTrue(updated.UpdatedAt > originalUpdatedAt, "UpdatedAtが新しい時刻に更新されているはず");
        }

        [TestMethod]
        public async Task DeleteAsync_論理削除されGetAllAsyncに含まれなくなる()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var dept = new Department { DeptName = "営業部", IsDeleted = false };
            context.Departments.Add(dept);
            await context.SaveChangesAsync();

            var repository = new DepartmentRepository(context);

            // Act
            await repository.DeleteAsync(dept.Id);

            // Assert
            // GetAllAsync には含まれない
            var allDepts = await repository.GetAllAsync();
            Assert.IsFalse(allDepts.Any(d => d.Id == dept.Id), "削除した部門は一覧に含まれないはず");

            // ただしDBには残っている（物理削除ではなく論理削除）
            var deletedDept = await context.Departments.FindAsync(dept.Id);
            Assert.IsNotNull(deletedDept, "レコード自体はDBに残っているはず（論理削除）");
            Assert.IsTrue(deletedDept.IsDeleted, "IsDeleted フラグが true になっているはず");
        }

        [TestMethod]
        public async Task DeleteAsync_存在しない部門IDでも例外を投げず何もしない()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            context.Departments.Add(new Department { DeptName = "営業部", IsDeleted = false });
            await context.SaveChangesAsync();
            var beforeCount = await context.Departments.CountAsync();

            var repository = new DepartmentRepository(context);

            // Act & Assert
            // 例外が投げられないことを確認（投げられたらテスト失敗）
            await repository.DeleteAsync(99999);

            // データに変化がないことを確認
            var afterCount = await context.Departments.CountAsync();
            Assert.AreEqual(beforeCount, afterCount, "存在しないIDの削除はデータに影響しないはず");
        }
    }
}
