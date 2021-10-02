using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DAL.Context;
using Domain;
using MathCore.EF7.Contexts;
using Microsoft.Extensions.Logging;

namespace DAL
{
    public class TestContextInitializer : DBInitializer<TestContext>
    {
        public TestContextInitializer(TestContext db, ILogger<DBInitializer<TestContext>> Logger) : base(db, Logger)
        {
        }

        /// <inheritdoc cref="DBInitializer{TContext}" />
        public new async Task InitializeAsync(CancellationToken Cancel = default)
        {
            await base.InitializeAsync(Cancel);

            _Logger.LogInformation("Проверка миграций БД");
            if (_db.Students.Any())
            {
                _Logger.LogInformation($"В БД есть записи");

                return;
            }
            Student[] students = new[]
            {
                new Student(){Age = 20, Name = "Alex"},
                new Student(){Age = 22, Name = "Nik"},
                new Student(){Age = 28, Name = "Tom"},
                new Student(){Age = 21, Name = "Jhon"},
                new Student(){Age = 19, Name = "Dex"},
                new Student(){Age = 32, Name = "Bado"},
                new Student(){Age = 18, Name = "Alex"},
                new Student(){Age = 25, Name = "Josh"},
                new Student(){Age = 25, Name = "Tom"}
            };
            _Logger.LogInformation($"Добавляю записи БД");
            await _db.Students.AddRangeAsync(students, Cancel);
            _Logger.LogInformation($"Сохраняю изменения БД");
            await _db.SaveChangesAsync(Cancel);
            _Logger.LogInformation($"БД инициализировано");
        }

    }
}
