using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

using System.Net;
using System.Net.Sockets;
using System.Diagnostics;



namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        private Boolean isExit = false;
        private string DM_IP =null;
        private int DM_PORT = 0;

        public TcpListener server = null;
        public TcpClient client = null;
        public NetworkStream stream = null;

        public static string INI_DAEMON_KEY_IP = "IP";
        public static string INI_DAEMON_KEY_PORT = "PORT";
        public static string INI_DAEMON_NAME = "DAEMONINFO";

        private string loggerBuffer;

        public LoGGer log;
       
        public Form1()
        {
            InitializeComponent();
            log = new LoGGer();
        }

        private void test_svc(){
            if (server == null)
            {
                server = new TcpListener(IPAddress.Parse(DM_IP), DM_PORT);
            }
            else
            {
                this.Close();
                if (Application.MessageLoop == true)
                {
                    Application.Exit();
                }
                else
                {
                    Environment.Exit(1);
                }
            }

            server.Start();
            this.backgroundWorker1.RunWorkerAsync();  

        }

        private void Click_Exit(object sender, EventArgs e)
        {
            if (!isExit)
            {
                Debug.WriteLine("======= Click_Exit Click =============\n");
                log.Log_Write("======= Click_Exit Click =============");
                
                isExit = true;
                if (server != null)
                {
                    Debug.WriteLine("======= Click_Exit Server Stop =============\n");
                    log.Log_Write("======= Click_Exit Server Stop =============");
                    if( client != null)
                        client.Close();
                    
                    server.Stop();
                }
            }
            Application.Exit();
            
        }

        private void backgroundworker_doWork(object sender, DoWorkEventArgs e)
        {

            try
            {
                client = server.AcceptTcpClient();
                Debug.WriteLine("======= Client WebSocket Connect!! =============\n");
                log.Log_Write("======= Client WebSocket Connect!! =============");

            }
            catch (NoNullAllowedException)
            {
                Debug.WriteLine("======= AcceptTcpClient NoNullAllowedException =============\n");
                log.Log_Write("======= AcceptTcpClient NoNullAllowedException =============");
                return;
            }
            catch (SocketException)
            {
                Debug.WriteLine("======= AcceptTcpClient SocketException =============\n");
                log.Log_Write("======= AcceptTcpClient SocketException =============");
                return;
            }
            catch (Exception)
            {
                Debug.WriteLine("======= AcceptTcpClient Exception =============\n");
                log.Log_Write("======= AcceptTcpClient Exception =============");
                return;
            }
            try
            {
                while (!isExit)
                {

                    stream = client.GetStream();

                    while (!stream.DataAvailable) ;
                    Debug.WriteLine("Client Send Data\n");

                    while (client.Available < 3) ;

                    byte[] bytes = new byte[client.Available];

                    stream.Read(bytes, 0, client.Available);

                    string s = Encoding.UTF8.GetString(bytes);

                    if (Regex.IsMatch(s, "^GET", RegexOptions.IgnoreCase))
                    {
                        Debug.WriteLine("=====Handshaking from client=====\n", s);
                        log.Log_Write("=====Handshaking from client=====");
                        // 1. Obtain the value of the "Sec-WebSocket-Key" request header without any leading or trailing whitespace
                        // 2. Concatenate it with "258EAFA5-E914-47DA-95CA-C5AB0DC85B11" (a special GUID specified by RFC 6455)
                        // 3. Compute SHA-1 and Base64 hash of the new value
                        // 4. Write the hash back as the value of "Sec-WebSocket-Accept" response header in an HTTP response
                        string swk = Regex.Match(s, "Sec-WebSocket-Key: (.*)").Groups[1].Value.Trim();
                        string swka = swk + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
                        byte[] swkaSha1 = System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(swka));
                        string swkaSha1Base64 = Convert.ToBase64String(swkaSha1);

                        // HTTP/1.1 defines the sequence CR LF as the end-of-line marker
                        byte[] response = Encoding.UTF8.GetBytes(
                            "HTTP/1.1 101 Switching Protocols\r\n" +
                            "Connection: Upgrade\r\n" +
                            "Upgrade: websocket\r\n" +
                            "Sec-WebSocket-Accept: " + swkaSha1Base64 + "\r\n\r\n");

                        stream.Write(response, 0, response.Length);
                        Debug.WriteLine("=====Handshaking SUCCESS Write To client=====\n", s);
                    }
                    else
                    {
                        /*
                        * 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
                          +-+-+-+-+-------+-+-------------+-------------------------------+
                          |F|R|R|R| opcode|M| Payload len |    Extended payload length    |
                          |I|S|S|S|  (4)  |A|     (7)     |             (16/64)           |
                          |N|V|V|V|       |S|             |   (if payload len==126/127)   |
                          | |1|2|3|       |K|             |                               |
                          +-+-+-+-+-------+-+-------------+ - - - - - - - - - - - - - - - +
                          |     Extended payload length continued, if payload len == 127  |
                          + - - - - - - - - - - - - - - - +-------------------------------+
                          |                               |Masking-key, if MASK set to 1  |
                          +-------------------------------+-------------------------------+
                          | Masking-key (continued)       |          Payload Data         |
                          +-------------------------------- - - - - - - - - - - - - - - - +
                          :                     Payload Data continued ...                :
                          + - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - +
                          |                     Payload Data continued ...                |
                        */

                        bool fin = ((bytes[0] & 0x80) != 0); // Indicates that this is the final fragment in a message.  The first fragment MAY also be the final fragment.

                        bool mask = ((bytes[1] & 0x80) != 0); // must be true, "All messages from the client to the server have this bit set"

                        int opcode = (bytes[0] & 0x0F); // expecting 1 - text message
                        int msglen = bytes[1] - 128, // & 0111 1111
                            offset = 2;

                        Debug.WriteLine("Payload len is " + msglen);
                        loggerBuffer = "Payload len is " + msglen;
                        log.Log_Write(loggerBuffer);
                        if (msglen == 126)
                        {
                            // was ToUInt16(bytes, offset) but the result is incorrect
                            msglen = BitConverter.ToUInt16(new byte[] { bytes[3], bytes[2] }, 0);
                            offset = 4;

                            loggerBuffer = "RECEIVE LEN [" + msglen + "]";
                            log.Log_Write(loggerBuffer);

                        }
                        else if (msglen == 127)
                        {
                            Console.WriteLine("TODO: msglen == 127, needs qword to store msglen");
                            // i don't really know the byte order, please edit this
                            // msglen = BitConverter.ToUInt64(new byte[] { bytes[5], bytes[4], bytes[3], bytes[2], bytes[9], bytes[8], bytes[7], bytes[6] }, 0);
                            // offset = 10;
                        }

                        if (opcode == 8 || msglen == 0) // 종료 신호 
                        {
                            Debug.WriteLine("Client is Exit");
                            log.Log_Write("Client Send Exit Signal");
                            client.Close();
                            break;

                        }
                        else if (mask)
                        {
                            byte[] decoded = new byte[msglen];
                            byte[] masks = new byte[4] { bytes[offset], bytes[offset + 1], bytes[offset + 2], bytes[offset + 3] };
                            offset += 4;

                            for (int i = 0; i < msglen; ++i)
                                decoded[i] = (byte)(bytes[offset + i] ^ masks[i % 4]);

                            string text = Encoding.UTF8.GetString(decoded);
                            Debug.WriteLine(text, "[RECEIVE]=");
                            loggerBuffer = "[RECEIVE]=" + text;
                            log.Log_Write(loggerBuffer);

                            MsgStructUsePOS.ReqPacktFromPOS reqmsgPOS = new MsgStructUsePOS.ReqPacktFromPOS();
                            Dictionary<string, string> reqMsgUsingDaemon = new Dictionary<string, string>();
                            Dictionary<string, string> respMsgUsingDaemon = new Dictionary<string, string>();

                            string resp_buf = String.Empty;
                            int totallen = 0;

                            reqMsgUsingDaemon.Clear();

                            try
                            {
                                reqmsgPOS.header.LENGTH = text.Substring(0, 4);
                                reqmsgPOS.header.MSG_VERSION = text.Substring(4, 4);
                                reqmsgPOS.header.TCODE = text.Substring(8, 2);
                                reqmsgPOS.header.MSG_TRACE = text.Substring(10, 12);
                                reqmsgPOS.header.DATA_TYPE = text.Substring(22, 10);

                                if (reqmsgPOS.header.DATA_TYPE == "FIXEDLEN  ")
                                {
                                    reqmsgPOS.body.TID = text.Substring(32, 10);
                                    reqmsgPOS.body.HALBU = text.Substring(42, 2);
                                    reqmsgPOS.body.TAMT = text.Substring(44, 9);
                                    reqmsgPOS.body.ORI_DATE = text.Substring(53, 6);
                                    reqmsgPOS.body.ORI_AUTHNO = text.Substring(59, 12);
                                    reqmsgPOS.body.IDNO = text.Substring(71, 33);
                                    reqmsgPOS.body.TAX_AMT = text.Substring(104, 9);
                                    reqmsgPOS.body.SVC_AMT = text.Substring(113, 9);
                                    reqmsgPOS.body.NONTAX_AMT = text.Substring(122, 9);
                                    reqmsgPOS.body.FILLER = text.Substring(131, 100);
                                    //reqmsgPOS.body.PG_FLAG = text.Substring(231, 2);
                                    //reqmsgPOS.body.PG_LEN = text.Substring(233, 3);
                                    //reqmsgPOS.body.PG_DATA = text.Substring(236, 500);
                                    if (reqmsgPOS.header.TCODE.ToString() == "Q0" || reqmsgPOS.header.TCODE.ToString() == "Q1")
                                    {
                                        reqmsgPOS.body.SET_QR_DATA_512 = text.Substring(231, 512);
                                        reqmsgPOS.body.SET_QR_DATA_256 = text.Substring(743, 256);
                                    }
                                    else
                                    {
                                        reqmsgPOS.body.SET_QR_DATA_512 = string.Empty.PadRight(512, ' ');
                                        reqmsgPOS.body.SET_QR_DATA_256 = string.Empty.PadRight(256, ' ');
                                    }
                                }
                                else
                                {
                                    // json 으로 변환
                                    //text = text.Replace(@"\", "");
                                    //var sendText = JsonConvert.DesserializeObject<VposAgentDataJson.ReqPacketBody>(text);

                                }

                                reqMsgUsingDaemon.Add("TCODE", reqmsgPOS.header.TCODE.ToString());              // [0][거래구분] : "S0" : 신용승인, "S1" : 신용취소, "41" : 현금영수증승인, "42" : 현금영수증취소, "E0" : 은련승인, "E1" : 은련취소, 
                                //                 "P0" : 앱카드승인, "P1" : 앱카드취소, "Z0" : 제로페이승인 "Z1" : 제로페이취소 "Q0" : 통합페이승인 "Q1" : 통합페이취소 
                                reqMsgUsingDaemon.Add("TID", reqmsgPOS.body.TID.ToString());                    // [1][단말기번호] 
                                reqMsgUsingDaemon.Add("HALBU", reqmsgPOS.body.HALBU.ToString());                // [2][할부] : 일시불 1
                                reqMsgUsingDaemon.Add("TAMT", reqmsgPOS.body.TAMT.ToString());                  // [3][거래금액]
                                reqMsgUsingDaemon.Add("TAX_AMT", reqmsgPOS.body.TAX_AMT.ToString());            // [4][세금]
                                reqMsgUsingDaemon.Add("SVC_AMT", reqmsgPOS.body.SVC_AMT.ToString());            // [5][봉사료]
                                reqMsgUsingDaemon.Add("NONTAX_AMT", reqmsgPOS.body.NONTAX_AMT.ToString());      // [6][비과세] 0 미적용, 그 외 적용 세액
                                reqMsgUsingDaemon.Add("ORI_DATE", reqmsgPOS.body.ORI_DATE.ToString());          // [7][원거래일자] 승인취소시에만 사용
                                reqMsgUsingDaemon.Add("ORI_AUTHNO", reqmsgPOS.body.ORI_AUTHNO.ToString());      // [8][원승인번호] 승인취소시에만 사용
                                reqMsgUsingDaemon.Add("MSG_TRACE", reqmsgPOS.header.MSG_TRACE.ToString());      // [9][거래일련번호]
                                reqMsgUsingDaemon.Add("IDNO", reqmsgPOS.body.IDNO.ToString());                  // [10][IDNO]
                                reqMsgUsingDaemon.Add("FILLER", reqmsgPOS.body.FILLER.ToString());              // [11][Filler]
                                //reqMsgUsingDaemon.Add("PG_FLAG", reqmsgPOS.body.PG_FLAG.ToString());          // [12][PG_FLAG]
                                //reqMsgUsingDaemon.Add("PG_LEN", reqmsgPOS.body.PG_LEN.ToString());            // [13][PG_LEN]
                                //reqMsgUsingDaemon.Add("PG_DATA", reqmsgPOS.body.PG_DATA.ToString());          // [14][PG_DATA]

                                reqMsgUsingDaemon.Add("SET_QR_DATA_512", reqmsgPOS.body.SET_QR_DATA_512.ToString());// [11][QR_DATA_512]
                                reqMsgUsingDaemon.Add("SET_QR_DATA_256", reqmsgPOS.body.SET_QR_DATA_256.ToString());// [12][QR_DATA_256]

                                UsingVposAgent agent = new UsingVposAgent();

                                agent.CatVirtualFormcs_Shown(reqMsgUsingDaemon);
                                respMsgUsingDaemon.Clear();
                                respMsgUsingDaemon = agent.OutputData;


                                totallen = 0;

                                if (respMsgUsingDaemon["ERRCODE"].ToString().Equals("0000") || respMsgUsingDaemon["ERRCODE"].ToString().Equals("8999"))
                                {
                                    totallen = respMsgUsingDaemon["ERRCODE"].Length + respMsgUsingDaemon["TRANTYPE"].Length + respMsgUsingDaemon["CARDNO"].Length + respMsgUsingDaemon["HALBU"].Length + respMsgUsingDaemon["TAMT"].Length + respMsgUsingDaemon["TRANDATE"].Length + respMsgUsingDaemon["TRANTIME"].Length + respMsgUsingDaemon["AUTHNO"].Length + respMsgUsingDaemon["MERNO"].Length + respMsgUsingDaemon["TRANSERIAL"].Length + respMsgUsingDaemon["ISSUECARD"].Length + respMsgUsingDaemon["PURCHASECARD"].Length +
                                        respMsgUsingDaemon["SIGNPATH"].Length + respMsgUsingDaemon["MSG1"].Length + respMsgUsingDaemon["MSG2"].Length + respMsgUsingDaemon["MSG3"].Length + respMsgUsingDaemon["MSG4"].Length + respMsgUsingDaemon["FILLER"].Length + respMsgUsingDaemon["QR_DATA_512"].Length + respMsgUsingDaemon["QR_DATA_256"].Length + 32;

                                    reqmsgPOS.header.LENGTH = totallen.ToString().Trim().PadLeft(4, '0');

                                    //resp_buf = reqmsgPOS.header.LENGTH.ToString() + reqmsgPOS.header.MSG_VERSION.ToString() + reqmsgPOS.header.TCODE.ToString() + reqmsgPOS.header.MSG_TRACE.ToString() + reqmsgPOS.header.DATA_TYPE.ToString();

                                    resp_buf = reqmsgPOS.header.LENGTH.ToString() +
                                                reqmsgPOS.header.MSG_VERSION.ToString() +
                                                reqmsgPOS.header.TCODE.ToString() +
                                                reqmsgPOS.header.MSG_TRACE.ToString() +
                                                reqmsgPOS.header.DATA_TYPE.ToString() +
                                                respMsgUsingDaemon["ERRCODE"].ToString() +
                                                respMsgUsingDaemon["TRANTYPE"].ToString() +
                                                respMsgUsingDaemon["CARDNO"].ToString() +
                                                respMsgUsingDaemon["HALBU"].ToString() +
                                                respMsgUsingDaemon["TAMT"].ToString() +
                                                respMsgUsingDaemon["TRANDATE"].ToString() +
                                                respMsgUsingDaemon["TRANTIME"].ToString() +
                                                respMsgUsingDaemon["AUTHNO"].ToString() +
                                                respMsgUsingDaemon["MERNO"].ToString() +
                                                respMsgUsingDaemon["TRANSERIAL"].ToString() +
                                                respMsgUsingDaemon["ISSUECARD"].ToString() +
                                                respMsgUsingDaemon["PURCHASECARD"].ToString() +
                                                respMsgUsingDaemon["SIGNPATH"].ToString() +
                                                respMsgUsingDaemon["MSG1"].ToString() +
                                                respMsgUsingDaemon["MSG2"].ToString() +
                                                respMsgUsingDaemon["MSG3"].ToString() +
                                                respMsgUsingDaemon["MSG4"].ToString() +
                                                respMsgUsingDaemon["FILLER"].ToString() +
                                                respMsgUsingDaemon["QR_DATA_512"].ToString() +
                                                respMsgUsingDaemon["QR_DATA_256"].ToString();

                                    //respMsgUsingDaemon["PGLEN"].ToString() + 
                                    //respMsgUsingDaemon["PGDATA"].ToString();

                                }
                                else
                                {
                                    totallen = 32 + respMsgUsingDaemon["ERRCODE"].Length + respMsgUsingDaemon["ResultMessage"].Length;
                                    //totallen = respMsgUsingDaemon["ERRCODE"].Length + respMsgUsingDaemon["ResultMessage"].Length + 32;

                                    reqmsgPOS.header.LENGTH = totallen.ToString().Trim().PadLeft(4, '0');

                                    //resp_buf = reqmsgPOS.header.LENGTH.ToString() + reqmsgPOS.header.MSG_VERSION.ToString() + reqmsgPOS.header.TCODE.ToString() + reqmsgPOS.header.MSG_TRACE.ToString() + reqmsgPOS.header.TID.ToString() + reqmsgPOS.header.DATA_TYPE.ToString() + respMsgUsingDaemon["0"].ToString() + respMsgUsingDaemon["ResultMessage"].ToString();
                                    resp_buf = reqmsgPOS.header.LENGTH.ToString() +
                                                reqmsgPOS.header.MSG_VERSION.ToString() +
                                                reqmsgPOS.header.TCODE.ToString() +
                                                reqmsgPOS.header.MSG_TRACE.ToString() +
                                                reqmsgPOS.header.DATA_TYPE.ToString() +
                                                respMsgUsingDaemon["ERRCODE"].ToString() +
                                                respMsgUsingDaemon["ResultMessage"].ToString();

                                }
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                string resultMessage = "ArgumentOutOfRangeException";
                                string resultErrcode = "9999";
                                totallen = 32 + resultErrcode.Length + resultMessage.Length;
                                //totallen = respMsgUsingDaemon["ERRCODE"].Length + respMsgUsingDaemon["ResultMessage"].Length + 32;

                                reqmsgPOS.header.LENGTH = totallen.ToString().Trim().PadLeft(4, '0');

                                //resp_buf = reqmsgPOS.header.LENGTH.ToString() + reqmsgPOS.header.MSG_VERSION.ToString() + reqmsgPOS.header.TCODE.ToString() + reqmsgPOS.header.MSG_TRACE.ToString() + reqmsgPOS.header.TID.ToString() + reqmsgPOS.header.DATA_TYPE.ToString() + respMsgUsingDaemon["0"].ToString() + respMsgUsingDaemon["ResultMessage"].ToString();
                                resp_buf = reqmsgPOS.header.LENGTH.ToString() +
                                            reqmsgPOS.header.MSG_VERSION.ToString() +
                                            reqmsgPOS.header.TCODE.ToString() +
                                            reqmsgPOS.header.MSG_TRACE.ToString() +
                                            reqmsgPOS.header.DATA_TYPE.ToString() +
                                            resultErrcode.ToString() +
                                            resultMessage.ToString();
                            }
                            catch (NullReferenceException)
                            {

                            }

                            Send_Message(stream, resp_buf);
                            Debug.WriteLine(resp_buf, "[SEND]=");
                            loggerBuffer = "[SEND]=" + resp_buf;
                            log.Log_Write(loggerBuffer);
                        }
                        else
                        {
                            Debug.WriteLine("mask bit not set");
                            log.Log_Write("mask bit not set");
                        }

                    }
                    Thread.Sleep(1);
                }
            }
            catch(Exception)
            {


            }



            Debug.WriteLine("End of While");
            log.Log_Write("End of While");

        }
        private void doExit()
        {
            if (Application.MessageLoop == true)
            {
                Application.Exit();
            }
            else
            {
                Environment.Exit(1);
            }

        }
        private Boolean checkINIfileData()
        {
            string path = System.IO.Directory.GetCurrentDirectory() + "\\WebDaemonInfo.ini";
            INIfile inifile = new INIfile(path);

            string strIP = inifile.ReadINI(INI_DAEMON_NAME, INI_DAEMON_KEY_IP);
            string striniPort = inifile.ReadINI(INI_DAEMON_NAME, INI_DAEMON_KEY_PORT);

            // check
            if (!string.IsNullOrEmpty(strIP) && !string.IsNullOrEmpty(striniPort))
            {
                this.DM_PORT = Convert.ToInt32(striniPort);
                this.DM_IP = strIP;

                return true;

            }

            return false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            bool isSavedINIFile = checkINIfileData();
            if (!isSavedINIFile)
            {
                //inifile.WriteINI(INI_DAEMON_NAME, INI_DAEMON_KEY_IP, "127.0.0.1");
                //inifile.WriteINI(INI_DAEMON_NAME, INI_DAEMON_KEY_PORT, "8888");
                DisplaySetting newform = new DisplaySetting();
                if( newform.ShowDialog() == DialogResult.Cancel)
                {

                    doExit();
                }
                else
                {
                    checkINIfileData();

                }

            }
          

            if (this.DM_PORT <= 0)
            {
                MessageBox.Show("WebDaemonInfo INI File Load Fail !!");
                log.Log_Write("INI File Daemon Port is 0 ");
                doExit();
            }
            else
            {
                if (server == null)
                {
                    server = new TcpListener(IPAddress.Parse(DM_IP), DM_PORT);
                    server.Start();
                    this.backgroundWorker1.RunWorkerAsync();
                }
                else
                {
                    this.Close();
                    doExit();
                }
            }

            loggerBuffer = "Load Connection IP [" + this.DM_IP + " ] PORT [" + this.DM_PORT + "]";
            log.Log_Write(loggerBuffer);
            
            log.Log_Write("DAEMON is READY!!");
            
        }

        void Send_Message(NetworkStream innerstream, string Message)
        {
            byte[] MessageByte = StringToByte(Message);

            // 길이 파악
            int offset = 2;
            int msglen = MessageByte.Length;
            if (msglen < 126)
            {
                offset = 2;
            }
            else if (msglen < 65536)
            {
                offset = 4;
            }
            else
            {
                offset = 10;
            }
            int bytelength = offset + msglen;

            byte[] sendbytes = new byte[bytelength];

            // 길이 설정
            sendbytes[0] = (byte)129;
            if (offset == 2)
            {
                sendbytes[1] = (byte)(msglen);
            }
            else if (offset == 4)
            {
                sendbytes[1] = (byte)(126);
                sendbytes[2] = (byte)(msglen >> 8);
                sendbytes[3] = (byte)(msglen >> 0);
            }
            else
            {
                sendbytes[1] = (byte)(127);
                sendbytes[2] = (byte)(msglen >> 24);
                sendbytes[3] = (byte)(msglen >> 16);
                sendbytes[4] = (byte)(msglen >> 8);
                sendbytes[5] = (byte)(msglen >> 0);
                sendbytes[6] = (byte)(msglen >> 56);
                sendbytes[7] = (byte)(msglen >> 48);
                sendbytes[8] = (byte)(msglen >> 40);
                sendbytes[9] = (byte)(msglen >> 32);
            }

            Array.Copy(MessageByte, 0, sendbytes, offset, msglen);

            innerstream.Write(sendbytes , 0 , sendbytes.Length);

            /// 전송 하지 못하였을 경우 
            
        }


        static string ByteArrayToString(byte[] ba)
        {
            return BitConverter.ToString(ba).Replace("-", "");
        }

        // 바이트 배열을 String으로 변환 
        private static string ByteToString(byte[] strByte) 
        { 
            string str = Encoding.Default.GetString(strByte); 
            return str; 
        } 
        
        // String을 바이트 배열로 변환 
        private static byte[] StringToByte(string str) 
        { 
            byte[] StrByte = Encoding.UTF8.GetBytes(str); 
            return StrByte; 
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            Debug.WriteLine("RunWorkerCompleted Server Restart");
            log.Log_Write("RunWorkerCompleted Server Restart");
            if (server.Pending())
            {
                Debug.WriteLine("Server Pendgin!! ");
                server.Stop();
                server.Start();
                Debug.WriteLine("Server Restart Done");
                log.Log_Write("Server Restart ");
            }

            this.backgroundWorker1.RunWorkerAsync();
        }

        private void changeIPPORTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DisplaySetting newform = new DisplaySetting();

            newform.ShowDialog();
        }
    
    }
}
