using System;
using FunDBLib;

namespace FunDBLib.Index
{
    public class ComparableObject<T> : IComparable
    {
        public T ContainedObject { get; private set; }

        public ComparableObject(T containedObject)
        {
            ContainedObject = containedObject;
        }

        public int CompareTo(object obj)
        {
            return ContainedObject.CompareObjects(obj);
        }
    }
}