using FunDBLib;

namespace FunDBLibTest
{
    public class TestDataContext : FDDataContext
    {
        public const string ConstDataPath = @".\TestDB";

        public FDTable<TestTable> TestTable { get; set; }

        public FDTable<TestTableIndex> TestTableIndex { get; set; }

        public TestDataContext()
        {
            
        }

        public override string GetDataPath()
        {
            return ConstDataPath;
        }

        protected override void OnModelCreated()
        {
            
        }
    }
}