using System;
using FunDBLib;

namespace FunDB.Database
{
    public class TestDataContext : FDDataContext
    {
        public FDTable<Customer> Customer { get; set; }

        public override string GetDataPath()
        {
            return @"C:\FunDBTestDB";
        }
    }
}