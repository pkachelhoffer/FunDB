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

            UpdateData();

            DeleteData();
        }

        private void CreateDB()
        {
            TestDataContext context = new TestDataContext();

            Assert.IsTrue(Directory.Exists(TestDataContext.ConstDataPath), "DB directory not created");
            Assert.IsTrue(File.Exists(Path.Combine(TestDataContext.ConstDataPath, "fdb_TestTable.dat")), "DB file not created");
            Assert.IsTrue(File.Exists(Path.Combine(TestDataContext.ConstDataPath, "TestTable_PK.idx")), "PK index file not created");
        }

        private void DeleteData()
        {
            TestDataContext context = new TestDataContext();

            var deleteRow = new TestTable(4, "Delete Row", 392932);

            context.TestTable.Add(deleteRow);
            context.TestTable.Submit();

            bool found = false;
            using (var reader = context.TestTable.GetReader())
                while (reader.ReadLine(out TestTable row))
                    if (row.TestTableID == deleteRow.TestTableID && row.RowDescription == deleteRow.RowDescription)
                        found = true;

            Assert.AreEqual(true, found, "Delete row not found");

            using (var reader = context.TestTable.GetReader())
                while (reader.ReadLine(out TestTable row))
                    if (row.TestTableID == deleteRow.TestTableID)
                        context.TestTable.Delete(row);

            context.TestTable.Submit();

            found = false;
            using (var reader = context.TestTable.GetReader())
                while (reader.ReadLine(out TestTable row))
                    if (row.TestTableID == deleteRow.TestTableID && row.RowDescription == deleteRow.RowDescription)
                        found = true;

            Assert.AreEqual(false, found, "Delete row found when it should be deleted");
        }   

        private void UpdateData()
        {
            TestDataContext context = new TestDataContext();

            string newDescription = "This row is updated. Deal with it";

            using (var reader = context.TestTable.GetReader())
                while (reader.ReadLine(out TestTable row))
                {
                    if (row.TestTableID == 2)
                    {
                        row.RowDescription = newDescription;
                        context.TestTable.Update(row);
                    }
                }
            
            context.TestTable.Submit();

            bool found = false;

            using (var reader = context.TestTable.GetReader())
                while (reader.ReadLine(out TestTable row))
                {
                    if (row.TestTableID == 2)
                    {
                        Assert.AreEqual(newDescription, row.RowDescription);
                        found = true;
                    }
                }

            Assert.AreEqual(true, found, "Updated row not found");
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