using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FunDBLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FunDBLibTest
{
    [TestClass]
    public class TextIndexPerformance
    {
        [TestMethod]
        public void TestIndexPerformance()
        {
            TestRoutines.ClearDB();
            TestRoutines.CreateDB();

            List<TestTableIndex> list = new List<TestTableIndex>();

            for (int x = 0; x < 1000000; x++)
            {
                list.Add(new TestTableIndex()
                {
                    TestTableIndexID = x,
                    LineName = $"This is line {x}"
                });
            }

            var dc = new TestDataContext();
            foreach (var line in list)
                dc.TestTableIndex.Add(line);

            var start = DateTime.Now;

            dc.TestTableIndex.Submit();

            var duration = DateTime.Now - start;

            Debug.WriteLine("Write Duration: " + duration.TotalSeconds);

            using (var reader = dc.TestTableIndex.GetReader())
                foreach (var item in list.Take(1000))
                {
                    var readItem = reader.Seek(new PrimaryKeyIndexInt() { PrimaryKey = item.TestTableIndexID });
                    Assert.AreEqual(item.TestTableIndexID, readItem.TestTableIndexID);
                    Assert.AreEqual(item.LineName, readItem.LineName);
                }
        }
    }
}