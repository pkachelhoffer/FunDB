using System;
using System.Collections.Generic;
using System.Reflection;
using FunDBLib.MetaData;

namespace FunDBLib
{
    public abstract class FDDataContext
    {
        internal string DataPath { get; set; }

        private IEnumerable<FDTable> Tables { get; set; }

        internal void Initialise(string path)
        {
            DataPath = path;
            InstantiateTables();
        }

        private void InstantiateTables()
        {
            var tables = new List<FDTable>();

            foreach(var property in GetType().GetProperties())
            {
                if (property.PropertyType.IsAssignableTo(typeof(FDTable)))
                {
                    var instance = Activator.CreateInstance(property.PropertyType) as FDTable;
                    instance.SetDataPath(DataPath);
                    property.SetValue(this, instance);

                    tables.Add(instance);
                }
            }

            Tables = tables;
        }

        public void Migrate()
        {
            foreach (var table in Tables)
            {
                MetaFieldTable metaTable = new MetaFieldTable(table.GetRowType());
                metaTable.SetDataPath(DataPath);
                foreach (var field in table.TableMetaData.Fields)
                    metaTable.Add(field);

                metaTable.Submit();
            }
        }
    }
}