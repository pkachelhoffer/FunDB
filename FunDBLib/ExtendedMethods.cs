using System;

namespace FunDBLib
{
    internal static class ExtendedMethods
    {
        public static int CompareObjects(this object source, object target)
        {
            var targetProperties = target.GetType().GetProperties();
            var sourceProperties = target.GetType().GetProperties();

            int result = 0;

            for (int x = 0; x < sourceProperties.Length; x++)
            {
                var sourceValue = (IComparable)sourceProperties[x].GetValue(source);
                var targetValue = (IComparable)targetProperties[x].GetValue(target);

                result = sourceValue.CompareTo(targetValue);
                if (result != 0)
                    return result;
            }

            return result;
        }
    }
}