using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace WindowsFormsApplication1
{
    class INIfile
    {
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string name, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        public string INIpath;

        public INIfile(string path)
        {
            this.INIpath = path;

        }

        public string ReadINI(string name, string key)
        {
            StringBuilder sb = new StringBuilder(255);
            int ini = GetPrivateProfileString(name, key, "", sb, 255, this.INIpath);
            return sb.ToString();
        }

        public void WriteINI(string name, string key, string value)
        {
            WritePrivateProfileString(name, key, value, this.INIpath);
        }
    }
}
