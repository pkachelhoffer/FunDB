using System;
using System.Collections;

namespace FunDBLib
{
    public struct PrimaryKeyIndexInt : IComparable
    {
        public int PrimaryKey { get; set; }

        public int CompareTo(object obj)
        {
            return PrimaryKey.CompareTo(((PrimaryKeyIndexInt)obj).PrimaryKey);
        }
    }

    public struct PrimaryKeyIndexLong : IComparable
    {
        public long PrimaryKey { get; set; }

        public int CompareTo(object obj)
        {
            return PrimaryKey.CompareTo(((PrimaryKeyIndexLong)obj).PrimaryKey);
        }
    }

    public struct PrimaryKeyIndexString : IComparable
    {
        public string PrimaryKey { get; set; }

        public int CompareTo(object obj)
        {
            return PrimaryKey.CompareTo(((PrimaryKeyIndexLong)obj).PrimaryKey);
        }
    }

    public struct PrimaryKeyIndexByte : IComparable
    {
        public byte PrimaryKey { get; set; }

        public int CompareTo(object obj)
        {
            return PrimaryKey.CompareTo(((PrimaryKeyIndexLong)obj).PrimaryKey);
        }
    }
}