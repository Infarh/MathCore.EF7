using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MathCore.EF7.Interfaces.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MathCore.EF7.Contexts
{
    /// <inheritdoc/>
    public class DBInitializer<TDb> : IDbInitializer where TDb:DbContext
    {
        protected readonly TDb _db;
        protected readonly ILogger<DBInitializer<TDb>> _Logger;

        /// <inheritdoc/>
        public bool Recreate { get; set; }
        /// <summary> Инициализирует работу с базой данных </summary>
        /// <param name="db">контекст базы данных</param>
        /// <param name="Logger">логгер</param>
        public DBInitializer(TDb db, ILogger<DBInitializer<TDb>> Logger)
        {
            _db = db;
            _Logger = Logger;
        }

        /// <inheritdoc/>
        public bool Delete()
        {
            if (!_db.Database.EnsureDeleted()) return false;
            _Logger.LogInformation("БД удалена");
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(CancellationToken Cancel = default)
        {
            if (!await _db.Database.EnsureDeletedAsync(Cancel).ConfigureAwait(false)) return false;
            _Logger.LogInformation("БД удалена");
            return true;
        }

        /// <inheritdoc/>
        public void Initialize()
        {
            var db = _db.Database;

            if (Recreate) Delete();

            var pending_migrations = db.GetPendingMigrations().ToArray();
            var applied_migrations = db.GetAppliedMigrations().ToArray();
            if (applied_migrations.Length == 0 && pending_migrations.Length == 0)
            {
                if (db.EnsureCreated())
                    _Logger.LogInformation("БД успешно создана");
            }
            else if (pending_migrations.Length > 0)
            {
                _Logger.LogInformation(
                    "БД существует. Миграций применено {0}. Требуется применить миграций {1}",
                    applied_migrations.Length, pending_migrations.Length);
                if (applied_migrations.Length > 0)
                    _Logger.LogInformation("Применённые миграции: {0}", string.Join(",", applied_migrations));

                db.Migrate();
                _Logger.LogInformation("Применённые миграции: {0}", string.Join(",", pending_migrations));
            }

            _Logger.LogInformation("Базовая инициализация экземпляра БД выполнена");

        }

        /// <inheritdoc/>
        public async Task InitializeAsync(CancellationToken Cancel = default)
        {
            var db = _db.Database;

            if (Recreate) await DeleteAsync(Cancel).ConfigureAwait(false);

            var pending_migrations = (await db.GetPendingMigrationsAsync(Cancel)).ToArray();
            var applied_migrations = (await db.GetAppliedMigrationsAsync(Cancel)).ToArray();
            if (applied_migrations.Length == 0 && pending_migrations.Length == 0)
            {
                if (await db.EnsureCreatedAsync(Cancel))
                    _Logger.LogInformation("БД успешно создана");
            }
            else if (pending_migrations.Length > 0)
            {
                _Logger.LogInformation(
                    "БД существует. Миграций применено {0}. Требуется применить миграций {1}",
                    applied_migrations.Length, pending_migrations.Length);
                if (applied_migrations.Length > 0)
                    _Logger.LogInformation("Применённые миграции: {0}", string.Join(",", applied_migrations));

                await db.MigrateAsync(Cancel);

                _Logger.LogInformation("Применённые миграции: {0}", string.Join(",", pending_migrations));
            }

            _Logger.LogInformation("Базовая инициализация экземпляра БД выполнена");
        }
    }
}
