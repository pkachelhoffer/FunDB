﻿using System;

namespace FunDBLib
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class FDTableAttribute : Attribute
    {
        public string TableName { get; set; }

        public FDTableAttribute(string tableName)
        {
            TableName = tableName;
        }
    }
}
