using System.Collections.Generic;
using FunDBLib.Attributes;

namespace FunDBLibTest
{
    public class TestTable
    {
        [FDPrimaryKey]
        public int TestTableID { get; set; }

        public string RowDescription { get; set; }

        public decimal Balance { get; set; }

        public TestTable()
        {
            
        }

        public TestTable(int testTableID, string rowDescription, decimal balance)
        {
            TestTableID = testTableID;
            RowDescription = rowDescription;
            Balance = balance;
        }

        public static TestTable[] GetTestData()
        {
            return new TestTable[]
            {
                new TestTable(1, "This is the first record", 34.2234m),
                new TestTable(2, "Middle record. Not here nor there", 19.99m),
                new TestTable(3, "Last record. Final", 1999232.3322m)
            };
        }
    }
}