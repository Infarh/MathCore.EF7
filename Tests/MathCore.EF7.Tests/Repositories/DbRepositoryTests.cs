using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MathCore.EF7.Repositories;
using MathCore.EF7.Repositories.Base;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MathCore.EF7.Tests.Repositories
{
    [TestClass]
    public class DbRepositoryTests
    {
        private class TestDbRepository : DbRepository<TestDbEntity>
        {
            public TestDbRepository(TestDbContext Context) : base(Context) { }
        }

        private class TestDbEntity
            : IEntity
        {
            public int Id { get; set; }
            public string Value { get; set; }

            public override string ToString() => $"[id:{Id}] {Value}";
        }

        private class TestDbContext : DbContext
        {
            public DbSet<TestDbEntity> Values { get; set; }

            public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
        }

        private TestDbContext _Context;

        [TestInitialize]
        public void TestInitialize()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase("TestDb").Options;
            _Context = new TestDbContext(options);
        }

        [TestMethod]
        public async Task AddEntityTest()
        {
            var repository = new TestDbRepository(_Context);

            var source_item1 = new TestDbEntity { Value = "Item1" };
            var source_item2 = new TestDbEntity { Value = "Item2" };

            var added_item1 = await repository.Add(source_item1);
            var added_item2 = await repository.Add(source_item2);

            var added_item1_id = added_item1.Id;
            var added_item2_id = added_item2.Id;

            var db_item1 = await repository.Get(added_item1_id);
            var db_item2 = await repository.Get(added_item2_id);

            var all_items = await repository.Items.ToArrayAsync();

            Assert.That.Value(db_item1)
               .Where(item => item.Id).CheckEquals(added_item1_id)
               .Where(item => item.Value).CheckEquals(source_item1.Value);
            Assert.That.Value(db_item2)
               .Where(item => item.Id).CheckEquals(added_item2_id)
               .Where(item => item.Value).CheckEquals(source_item2.Value);

            Assert.That.Collection(all_items)
               .Contains(item => item.Value == source_item1.Value)
               .Contains(item => item.Value == source_item2.Value);

        }
    }
}
