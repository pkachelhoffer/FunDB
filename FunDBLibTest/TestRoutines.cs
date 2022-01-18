using System.Collections.Generic;
using System.IO;
using FunDBLib;
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

            TestIndex();
        }

        [TestMethod]
        public void PerformanceTest()
        {
            ClearDB();
            CreateDB();

            var dc = new TestDataContext();

            for (int x = 0; x < 1000000; x++)
            {
                dc.TestTable.Add(new TestTable(x, $"This is row {x}", x * 12.25m));
            }

            dc.TestTable.Submit();

            using (var reader = dc.TestTable.GetReader())
            {
                var row = reader.Seek(new PrimaryKeyIndexInt() { PrimaryKey = 123456 });
            }
        }

        public static void CreateDB()
        {
            TestDataContext context = new TestDataContext();

            Assert.IsTrue(Directory.Exists(TestDataContext.ConstDataPath), "DB directory not created");
            Assert.IsTrue(File.Exists(Path.Combine(TestDataContext.ConstDataPath, "fdb_TestTable.dat")), "DB file not created");
        }

        private void TestIndex()
        {
            TestDataContext context = new TestDataContext();

            List<TestTableIndex> indexEntries = new List<TestTableIndex>();
            indexEntries.Add(new TestTableIndex() { TestTableIndexID = 5, LineName = "sadfas" });
            indexEntries.Add(new TestTableIndex() { TestTableIndexID = 4, LineName = "sadfas" });
            indexEntries.Add(new TestTableIndex() { TestTableIndexID = 6, LineName = "sadfas" });
            indexEntries.Add(new TestTableIndex() { TestTableIndexID = 8, LineName = "sadfas" });
            indexEntries.Add(new TestTableIndex() { TestTableIndexID = 1, LineName = "sadfas" });
            indexEntries.Add(new TestTableIndex() { TestTableIndexID = 3, LineName = "sadfas" });
            indexEntries.Add(new TestTableIndex() { TestTableIndexID = 2, LineName = "sadfas" });
            indexEntries.Add(new TestTableIndex() { TestTableIndexID = 9, LineName = "sadfas" });
            indexEntries.Add(new TestTableIndex() { TestTableIndexID = 7, LineName = "sadfas" });
            indexEntries.Add(new TestTableIndex() { TestTableIndexID = 10, LineName = "sadfas" });
            indexEntries.Add(new TestTableIndex() { TestTableIndexID = 12, LineName = "sadfas" });
            indexEntries.Add(new TestTableIndex() { TestTableIndexID = 14, LineName = "sadfas" });
            indexEntries.Add(new TestTableIndex() { TestTableIndexID = 16, LineName = "sadfas" });
            indexEntries.Add(new TestTableIndex() { TestTableIndexID = 18, LineName = "sadfas" });
            indexEntries.Add(new TestTableIndex() { TestTableIndexID = 20, LineName = "sadfas" });

            foreach (var entry in indexEntries)
                context.TestTableIndex.Add(entry);

            context.TestTableIndex.Submit();

            List<TestTableIndex> indexEntries2 = new List<TestTableIndex>();
            indexEntries2.Add(new TestTableIndex() { TestTableIndexID = 11, LineName = "sadfas" });
            indexEntries2.Add(new TestTableIndex() { TestTableIndexID = 13, LineName = "sadfas" });
            indexEntries2.Add(new TestTableIndex() { TestTableIndexID = 15, LineName = "sadfas" });
            indexEntries2.Add(new TestTableIndex() { TestTableIndexID = 17, LineName = "sadfas" });
            indexEntries2.Add(new TestTableIndex() { TestTableIndexID = 19, LineName = "sadfas" });

            foreach (var entry in indexEntries2)
                context.TestTableIndex.Add(entry);

            context.TestTableIndex.Submit();

            indexEntries.AddRange(indexEntries2);

            using (var reader = context.TestTableIndex.GetReader())
            {
                foreach (var entry in indexEntries)
                {
                    var row = reader.Seek(new PrimaryKeyIndexInt() { PrimaryKey = entry.TestTableIndexID });
                    Assert.IsNotNull(row, $"Could not find record for ID {entry.TestTableIndexID}.");
                    Assert.AreEqual(entry.TestTableIndexID, row.TestTableIndexID);
                }
            }

            foreach (var entry in indexEntries)
                context.TestTableIndex.Add(entry);

            //Delete index
            using (var reader = context.TestTableIndex.GetReader())
            {
                var row = reader.Seek(new PrimaryKeyIndexInt() { PrimaryKey = 5 });
                context.TestTableIndex.Delete(row);
                row = reader.Seek(new PrimaryKeyIndexInt() { PrimaryKey = 10 });
                context.TestTableIndex.Delete(row);
                row = reader.Seek(new PrimaryKeyIndexInt() { PrimaryKey = 1 });
                context.TestTableIndex.Delete(row);
            }

            context.TestTableIndex.Submit();

            using (var reader = context.TestTableIndex.GetReader())
            {
                foreach (var entry in indexEntries)
                {
                    var row = reader.Seek(new PrimaryKeyIndexInt() { PrimaryKey = entry.TestTableIndexID });
                    if (entry.TestTableIndexID == 5 || entry.TestTableIndexID == 10 || entry.TestTableIndexID == 1)
                        Assert.IsNull(row, $"Row {entry.TestTableIndexID} was not deleted from index");
                    else
                    {
                        Assert.IsNotNull(row, $"Could not find record for ID {entry.TestTableIndexID}.");
                        Assert.AreEqual(entry.TestTableIndexID, row.TestTableIndexID);
                    }
                }
            }
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

        public static void ClearDB()
        {
            if (Directory.Exists(TestDataContext.ConstDataPath))
                foreach (var file in Directory.GetFiles(TestDataContext.ConstDataPath))
                    if (Path.GetFileName(file).StartsWith("fdb"))
                        File.Delete(file);
        }
    }
}