#nullable enable

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MathCore.EF7.Interfaces.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MathCore.EF7.Contexts
{
    /// <summary> Базовая реализация фабрики инициализации БД </summary>
    /// <typeparam name="TDb">тип контекста базы данных</typeparam>
    public class DBFactoryInitializer<TDb> : IDbInitializer, IDisposable where TDb :DbContext
    {
        private readonly TDb _db;
        protected readonly ILogger<DBInitializer<TDb>> _Logger;

        /// <inheritdoc/>
        public bool Recreate { get; set; }
        /// <summary> инициализация фабрики </summary>
        /// <param name="db">контекст базы данных</param>
        /// <param name="Logger">логгер</param>
        public DBFactoryInitializer(
            IDbContextFactory<TDb> db, 
            ILogger<DBInitializer<TDb>> Logger)
        {
            if (db is null) throw new ArgumentNullException(nameof(db));
            _db = db.CreateDbContext();
            _Logger = Logger ?? throw new ArgumentNullException(nameof(Logger));
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
            var timer = Stopwatch.StartNew();
            var db = _db.Database;

            if (Recreate)
                if (!Delete())
                    _Logger.LogInformation("БД отсутствует");

            var pending_migrations = db.GetPendingMigrations().ToArray();
            var applied_migrations = db.GetAppliedMigrations().ToArray();
            if (applied_migrations.Length == 0 && pending_migrations.Length == 0)
            {
                if (db.EnsureCreated())
                    _Logger.LogInformation("БД успешно создана. {0} с", timer.Elapsed.TotalSeconds);
            }
            else if (pending_migrations.Length > 0)
            {
                _Logger.LogInformation(
                    "БД существует. Миграций применено {0}. Требуется применить миграций {1}. {2} c",
                    applied_migrations.Length, pending_migrations.Length, timer.Elapsed.TotalSeconds);
                if (applied_migrations.Length > 0)
                    _Logger.LogInformation("Применённые миграции: {0}", string.Join(",", applied_migrations));

                db.Migrate();
                _Logger.LogInformation("Применённые миграции: {0}", string.Join(",", pending_migrations));
            }

            _Logger.LogInformation("Базовая инициализация экземпляра БД выполнена за {0} с", timer.Elapsed.TotalSeconds);

        }

        /// <inheritdoc/>
        public async Task InitializeAsync(CancellationToken Cancel = default)
        {
            var timer = Stopwatch.StartNew();
            var db = _db.Database;

            if (Recreate)
                if (!await DeleteAsync(Cancel).ConfigureAwait(false))
                    _Logger.LogInformation("БД отсутствует");

            var pending_migrations = (await db.GetPendingMigrationsAsync(Cancel)).ToArray();
            var applied_migrations = (await db.GetAppliedMigrationsAsync(Cancel)).ToArray();
            if (applied_migrations.Length == 0 && pending_migrations.Length == 0)
            {
                if (await db.EnsureCreatedAsync(Cancel))
                    _Logger.LogInformation("БД успешно создана. {0} с", timer.Elapsed.TotalSeconds);
            }
            else if (pending_migrations.Length > 0)
            {
                _Logger.LogInformation(
                    "БД существует. Миграций применено {0}. Требуется применить миграций {1}. {2} c",
                    applied_migrations.Length, pending_migrations.Length, timer.Elapsed.TotalSeconds);
                if (applied_migrations.Length > 0)
                    _Logger.LogInformation("Применённые миграции: {0}", string.Join(",", applied_migrations));

                await db.MigrateAsync(Cancel);

                _Logger.LogInformation("Применённые миграции: {0}. {1} с", 
                    string.Join(",", pending_migrations), timer.Elapsed.TotalSeconds);
            }

            _Logger.LogInformation("Базовая инициализация экземпляра БД выполнена за {0} с", timer.Elapsed.TotalSeconds);
        }
        /// <summary> Освобождение контекста </summary>
        /// <param name="disposing">необходимость освобождения контекста</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
