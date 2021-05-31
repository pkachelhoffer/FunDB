using System;
using System.Collections.Generic;
using System.IO;

namespace FunDB
{
    class Program
    {
        static void Main(string[] args)
        {
            Write();
            var records = Read();
        }

        private static void Write()
        {
            List<TestRecord> records = new List<TestRecord>();
            for (int x = 0; x < 100000; x++)
            {
                records.Add(new TestRecord($"Name{x}", $"Surname{x}", x % 100, x));
            }
            //var records = new TestRecord[]
            //{
            //    new TestRecord("Frank", "Smith", 29, 34.34m),
            //    new TestRecord("Steve", "Hackney", 56, 10534.95m),
            //    new TestRecord("Sam", "Stout", 36, 9945.34m)
            //};

            using (var sr = new FileStream("SomeFile.dat", FileMode.Create))
            {
                foreach (var record in records)
                {
                    var bytesValues = record.SerializeLine();
                    byte[] length = BitConverter.GetBytes(bytesValues.Length);
                    var bytesLine = new byte[length.Length + bytesValues.Length];

                    length.CopyTo(bytesLine, 0);
                    bytesValues.CopyTo(bytesLine, length.Length);

                    sr.Write(bytesLine, 0, bytesLine.Length);
                }
            }
        }

        private static List<TestRecord> Read()
        {
            List<TestRecord> records = new List<TestRecord>();

            using (var sr = new StreamReader("SomeFile.dat"))
            {
                while (sr.BaseStream.Position < sr.BaseStream.Length)
                {
                    byte[] bytesLength = new byte[4];
                    sr.BaseStream.Read(bytesLength, 0, bytesLength.Length);

                    int length = BitConverter.ToInt32(bytesLength);
                    byte[] bytesValues = new byte[length];
                    sr.BaseStream.Read(bytesValues, 0, length);

                    var record = new TestRecord();
                    record.Deserialise(bytesValues);

                    records.Add(record);
                }
            }

            return records;
        }
    }
}
