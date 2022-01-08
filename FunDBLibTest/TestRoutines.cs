using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FunDBLibTest
{
    [TestClass]
    public class TestRoutines
    {
        [TestMethod]
        public void TestDB()
        {
            ClearDB();

            CreateDB();

            InsertData();
        }

        private void CreateDB()
        {
            TestDataContext context = new TestDataContext();

            Assert.IsTrue(Directory.Exists(TestDataContext.ConstDataPath), "DB directory not created");
            Assert.IsTrue(File.Exists(Path.Combine(TestDataContext.ConstDataPath, "fdb_TestTable.dat")), "DB file not created");
            Assert.IsTrue(File.Exists(Path.Combine(TestDataContext.ConstDataPath, "TestTable_PK.idx")), "PK index file not created");
        }

        private void InsertData()
        {
            TestDataContext context = new TestDataContext();

            var testData = TestTable.GetTestData();

            foreach (var row in testData)
                context.TestTable.Add(row);

            context.TestTable.Submit();

            int x = 0;
            using (var reader = context.TestTable.GetReader())
            {
                while (reader.ReadLine(out TestTable row))
                {
                    Assert.AreEqual(testData[x].TestTableID, row.TestTableID);
                    Assert.AreEqual(testData[x].RowDescription, row.RowDescription);
                    Assert.AreEqual(testData[x].Balance, row.Balance);

                    x++;
                }
            }
        }

        private void ClearDB()
        {
            if (Directory.Exists(TestDataContext.ConstDataPath))
                foreach (var file in Directory.GetFiles(TestDataContext.ConstDataPath))
                    File.Delete(file);
        }
    }
}