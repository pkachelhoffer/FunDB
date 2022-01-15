using System;
using System.Collections;

namespace FunDBLib
{
    public class PrimaryKeyIndexInt : IComparable
    {
        public int PrimaryKey { get; set; }

        public int CompareTo(object obj)
        {
            return PrimaryKey.CompareTo(((PrimaryKeyIndexInt)obj).PrimaryKey);
        }
    }

    public class PrimaryKeyIndexLong : IComparable
    {
        public long PrimaryKey { get; set; }

        public int CompareTo(object obj)
        {
            return PrimaryKey.CompareTo(((PrimaryKeyIndexLong)obj).PrimaryKey);
        }
    }

    public class PrimaryKeyIndexString : IComparable
    {
        public string PrimaryKey { get; set; }

        public int CompareTo(object obj)
        {
            return PrimaryKey.CompareTo(((PrimaryKeyIndexLong)obj).PrimaryKey);
        }
    }

    public class PrimaryKeyIndexByte : IComparable
    {
        public byte PrimaryKey { get; set; }

        public int CompareTo(object obj)
        {
            return PrimaryKey.CompareTo(((PrimaryKeyIndexLong)obj).PrimaryKey);
        }
    }
}