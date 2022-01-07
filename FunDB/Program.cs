using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FunDB.Database;

namespace FunDB
{
    class Program
    {
        static void Main(string[] args)
        {
            using(var fileStream = new FileStream("Testfile.txt", FileMode.Create))
            {
                int number = 10;
                var bytes = BitConverter.GetBytes(number);
                fileStream.Write(bytes, 0, bytes.Length);
            }

            using(var fileStream = new FileStream("Testfile.txt", FileMode.Open))
            {
                int number = 10;
                var bytes = BitConverter.GetBytes(number);
                fileStream.Write(bytes, 0, bytes.Length);
            }

            StartDB();
        }

        private static void StartDB()
        {
            WriteTestData();

            var dataContext = new TestDataContext();
            using (var reader = dataContext.Customer.GetReader())
            {
                while (reader.ReadLine(out Customer customer))
                    Console.WriteLine($"Hello {customer.CustomerID} {customer.Name}");
            }
        }

        private static void WriteTestData()
        {
            var dataContext = new TestDataContext();

            for (int x = 1; x < 1000; x++)
                dataContext.Customer.Add(new Customer(x, $"Name {x}"));
                //dataContext.Customer.Add(new Customer(x, $"Customer {x}", $"Van Tonder {x}", x, 2000 - x));

            dataContext.Customer.Submit();
        }
    }
}
