using System;
using FunDBLib;

namespace FunDB.Database
{
    public class Customer
    {
        public int CustomerID { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }

        public int Age { get; set; }

        public decimal BankBalance { get; set; }

        public Customer()
        {

        }

        public Customer(int customerID, string name, string surname, int age, decimal bankBalance)
        {
            CustomerID = customerID;
            Name = name;
            Surname = surname;
            Age = age;
            BankBalance = bankBalance;
        }


    }
}