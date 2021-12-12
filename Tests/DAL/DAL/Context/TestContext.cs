using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL.Context
{
    public class TestContext : DbContext
    {
        //public TestContext(DbContextOptions<TestContext> options, IConfiguration configuration) : base(options)
        //{
        //    ConnectionString = configuration.GetConnectionString("DefaultConnection");
        //}

        //public TestContext(IConfiguration configuration) : base(GetOptions(configuration.GetConnectionString("DefaultConnection")))
        //{ }
        //public TestContext(string ConnectionString) : base(GetOptions(ConnectionString))
        //{
        //}

        public TestContext(DbContextOptions<TestContext> options) : base(options) { }
        /// <summary> Инициализация опций контекста по строке подключения </summary>
        /// <param name="ConnectionString">строка подключения</param>
        /// <returns></returns>
        private static DbContextOptions<TestContext> GetOptions(string ConnectionString)
        {
            var builder = new DbContextOptionsBuilder<TestContext>();
            builder.UseSqlServer(ConnectionString);
            builder.UseLazyLoadingProxies();
            return builder.Options;

        }

        //public string ConnectionString { get; private set; }

        public DbSet<Student> Students { get; set; }
    }
}
