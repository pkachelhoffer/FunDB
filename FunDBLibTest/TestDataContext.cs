using FunDBLib;

namespace FunDBLibTest
{
    public class TestDataContext : FDDataContext
    {
        public const string ConstDataPath = @".\TestDB";
        public FDTable<TestTable> TestTable { get; set; }

        public TestDataContext()
        {
            
        }

        public override string GetDataPath()
        {
            return ConstDataPath;
        }

        protected override void OnModelCreated()
        {
            TestTable.AddIndex("PK", (s) => new { s.TestTableID });
        }
    }
}