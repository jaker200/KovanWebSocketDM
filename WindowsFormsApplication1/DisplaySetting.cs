using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


using System.IO;
using System.Runtime.InteropServices;


namespace WindowsFormsApplication1
{
    public partial class DisplaySetting : Form
    {
        public static string INI_DAEMON_KEY_IP = "IP";
        public static string INI_DAEMON_KEY_PORT = "PORT";
        public static string INI_DAEMON_NAME = "DAEMONINFO";
        public static string INI_DAEMON_FILE_NAME = "WebDaemonInfo.ini";

        public string path = string.Empty;


        public DisplaySetting()
        {
            InitializeComponent();

            path = System.IO.Directory.GetCurrentDirectory() + "\\" + INI_DAEMON_FILE_NAME;

            
        }

  
        private void label_IP_Click(object sender, EventArgs e)
        {

        }

        private void DisplaySetting_Load(object sender, EventArgs e)
        {
            // 환경설정 파일 로드
            string ip = string.Empty;
            string port = string.Empty;

            INIfile inifile = new INIfile(path);

            ip = inifile.ReadINI(INI_DAEMON_NAME, INI_DAEMON_KEY_IP);
            if (ip.Length > 0)
            {

                textBox_IP.Text = ip;
            }

            port = inifile.ReadINI(INI_DAEMON_NAME, INI_DAEMON_KEY_PORT);
            if (port.Length > 0)
            {

                textBox_PORT.Text = port;
            }

        }

        private void button_save_Click(object sender, EventArgs e)
        {
            int itextIpLen = textBox_IP.Text.ToString().Trim().Length;
            int  itextPortLen = textBox_IP.Text.ToString().Trim().Length;
            if (itextIpLen <= 0 || itextPortLen <= 0)
            {
                MessageBox.Show("IP / PORT 정보를 입력 해 주세요. ") ;
            }
            else
            {
                // 환경설정 파일 저장
                INIfile inifile = new INIfile(path);

                inifile.WriteINI(INI_DAEMON_NAME, INI_DAEMON_KEY_IP, textBox_IP.Text);
                inifile.WriteINI(INI_DAEMON_NAME, INI_DAEMON_KEY_PORT, textBox_PORT.Text);
                this.DialogResult = DialogResult.OK;
                this.Close();

            }
        }

        private void button_exit_Click(object sender, EventArgs e)
        {
            MessageBox.Show("저장 하지 않고 종료합니다.");
            this.DialogResult = DialogResult.Cancel;
            this.Close();

        }
    }
}
