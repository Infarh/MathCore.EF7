using System.Threading;
using System.Threading.Tasks;

namespace MathCore.EF7.Interfaces.Context
{
    /// <summary> Описывает инициализацию и удаление базы данных </summary>
    public interface IDbInitializer
    {
        /// <summary> Необходимость пересоздать базу данных после удаления</summary>
        bool Recreate { get; set; }
        /// <summary> Удаление базы данных </summary>
        /// <returns>успех операции удаления</returns>
        bool Delete();
        /// <summary> Удаление базы данных </summary>
        /// <returns>успех операции удаления</returns>
        Task<bool> DeleteAsync(CancellationToken Cancel = default);
        /// <summary> Инициализировать базу данных </summary>
        void Initialize();
        /// <summary> Инициализировать базу данных </summary>
        Task InitializeAsync(CancellationToken Cancel = default);
    }
}
