using System;
using FunDBLib;

namespace FunDB
{
    public class FunPOSDatabase : FDDatabase
    {
        public override string GetPath()
        {
            return @"C:\FunDBTestDB";
        }
    }
}