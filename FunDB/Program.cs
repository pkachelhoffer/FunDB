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
            StartDB();
        }

        private static void StartDB()
        {
            var dataContext = new TestDataContext();

            WriteTestData();

            using (var reader = dataContext.Customer.GetReader())
            {
                while (reader.ReadLine(out Customer customer))
                {
                    Console.WriteLine($"Hello {customer.Name}, balance is {customer.BankBalance}");
                }
            }
        }

        private static void WriteTestData()
        {
            var dataContext = new TestDataContext();

            for (int x = 1; x < 1000000; x++)
                dataContext.Customer.Add(new Customer(x, $"Customer {x}", $"Van Tonder {x}", x, 2000 - x));

            dataContext.Customer.Submit();
        }
    }
}
