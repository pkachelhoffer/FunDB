using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FunDBLib.MetaData;

namespace FunDBLib
{
    public abstract class FDDataContext
    {
        private static object InitialiseLock = new object();

        private IEnumerable<FDTable> Tables { get; set; }

        private static bool Initialised { get; set; }
        private static string DataPath { get; set; }

        public FDDataContext()
        {
            if (!Initialised)
                lock (InitialiseLock)
                    if (!Initialised)
                        Initialise();

            InstantiateTables();
        }

        private void Initialise()
        {
            DataPath = GetDataPath();

            if (!Directory.Exists(DataPath))
                Directory.CreateDirectory(DataPath);

            Initialised = true;
        }

        public abstract string GetDataPath();

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
    }
}