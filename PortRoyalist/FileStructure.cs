using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PortRoyalist
{
    public static class FileStructure
    {
        private readonly static string InputDirRelative = "Data\\Input\\";
        private readonly static string PreparedDirRlative = "Data\\Prepared\\";

        public static string InputDir => MapPath(InputDirRelative);
        public static string MapInputDir(string fileName) => Path.Combine(InputDir, fileName);

        public static string PreparedDir => MapPath(PreparedDirRlative);
        public static string MapPreparedDir(string fileName) => Path.Combine(PreparedDir, fileName);


        /// <summary>
        /// Liefert den Verzeichnissnamen für das gegebene Screenshot (xy.png)
        /// </summary>
        public static string SplitDir(string imgFileName, bool remove = false)
        {
            var splitDir = Path.Combine(PreparedDir, imgFileName.Replace(".", "_"));
            if (remove)
            {
                if (Directory.Exists(splitDir)) Directory.Delete(splitDir, true);
            }
            Directory.CreateDirectory(splitDir);

            return splitDir;
        }

        public static string TemplateImg => Path.Combine(MapPath(InputDir), "template.png");


        public static void Init()
        {
            Directory.CreateDirectory(InputDir);
            Directory.CreateDirectory(PreparedDir);
        }



        private static string MapPath(string relativePath)
        {
            //C:\Users\giczi\source\repos\PortRoyalist\PortRoyalist\bin\Debug\netcoreapp3.0
            string startupPath = Environment.CurrentDirectory;

            startupPath = startupPath.Replace(@"\PortRoyalist.Tests\", @"\PortRoyalist\");

            startupPath = startupPath.Substring(0, startupPath.IndexOf("bin\\"));
            //string startupPath = Environment.CurrentDirectory;

            var res = Path.Combine(startupPath, relativePath);
            return res;
        }

    }
}
