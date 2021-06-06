using System;

namespace FunDBLib
{
    public abstract class FDDataContext
    {
        internal string DataPath { get; set; }

        public FDDataContext()
        {
            
        }

        internal void Initialise(string path)
        {
            DataPath = path;
            InstantiateTables();
        }

        private void InstantiateTables()
        {
            foreach(var property in GetType().GetProperties())
            {
                if (property.PropertyType.IsAssignableTo(typeof(FDTable)))
                {
                    var instance = Activator.CreateInstance(property.PropertyType) as FDTable;
                    instance.SetDataPath(DataPath);
                    property.SetValue(this, instance);
                }
            }
        }
    }
}