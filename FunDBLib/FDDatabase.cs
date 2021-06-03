using System;
using System.IO;

namespace FunDBLib
{
    public abstract class FDDatabase
    {
        private string Path { get; set; }

        private bool Initialised { get; set; }

        public FDDatabase()
        {
            Initialised = false;
        }

        /// <summary>
        /// Return path to folder where FunDB will be created
        /// </summary>
        public abstract string GetPath();

        public void Initialise()
        {
            if (Initialised)
                throw new Exception("Already initialised");

            Path = GetPath();
            
            if (string.IsNullOrEmpty(Path))
                throw new Exception("Path not set");

            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);

            Initialised = true;
        }
    }
}