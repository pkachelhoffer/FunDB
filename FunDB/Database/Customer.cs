using System;
using FunDBLib;
using FunDBLib.Attributes;

namespace FunDB.Database
{
    public class Customer
    {
        public int CustomerID { get; set; }

        public string Name { get; set; }

        public Customer()
        {

        }

        public Customer(int customerID, string name)
        {
            CustomerID = customerID;
            Name = name;
        }
    }
}