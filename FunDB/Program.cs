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
            WriteTestData();

            var dataContext = new TestDataContext();
            using (var reader = dataContext.Customer.GetReader())
            {
                while (reader.ReadLine(out Customer customer))
                {

                }
                    //Console.WriteLine($"Hello {customer.CustomerID} {customer.Name}");
            }
        }

        private static void WriteTestData()
        {
            var dataContext = new TestDataContext();

            dataContext.Customer.Add(new Customer(1231230, "Final Customer"));

            //for (int x = 1; x < 1000000; x++)
                //dataContext.Customer.Add(new Customer(x, $"Name {x}"));
                //dataContext.Customer.Add(new Customer(x, $"Customer {x}", $"Van Tonder {x}", x, 2000 - x));

            dataContext.Customer.Submit();
        }
    }
}
