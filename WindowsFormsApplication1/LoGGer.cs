using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public class LoGGer
    {
        public string strCheckFolder;
        public string strFileName;
        public string strLocal;

        public LoGGer()
        {
            strCheckFolder = string.Empty;
            strFileName = string.Empty;

            strLocal = Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\"));

            strCheckFolder = strLocal + "\\Logs";
            if (!System.IO.Directory.Exists(strCheckFolder))
            {
                System.IO.Directory.CreateDirectory(strCheckFolder);

            }
        }

        public void Log_Write(string strMsg)
        {
            try
            {
                /*
                string strCheckFolder = string.Empty;
                string strFileName = string.Empty;

                string strLocal = Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\"));

                strCheckFolder = strLocal + "Logs";
                if (!System.IO.Directory.Exists(strCheckFolder))
                {
                    System.IO.Directory.CreateDirectory(strCheckFolder);

                }
                */

                strFileName = strCheckFolder + "\\" + "WebSocket" + DateTime.Now.ToString("yyMMdd") + ".txt";

                System.IO.StreamWriter FileWriter = new System.IO.StreamWriter(strFileName, true);
                FileWriter.Write("[" + DateTime.Now.ToString("hh:mm:ss") + "] " + strMsg + "\r\n");
                FileWriter.Flush();
                FileWriter.Close();

            }
            catch
            {

            }

        }
    }
}
