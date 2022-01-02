using System;

namespace FunDBLib.MetaData
{
    internal class MetaFieldTable : FDTable<MetaField>
    {
        private Type TableType { get; set; }

        public MetaFieldTable(Type tableType)
        {
            TableType = tableType;
        }

        protected override string GetTableName()
        {
            return $"{TableType.Name}_Meta";
        }
    }
}