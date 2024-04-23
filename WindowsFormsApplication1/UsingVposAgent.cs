using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{

    class UsingVposAgent
    {
        [DllImport(@"C:\KOVAN\VPOS_Client.dll", CharSet = CharSet.Ansi)]
        private static extern int Kovan_Auth(byte[] tcode, byte[] tid, byte[] halbu, byte[] tamt, byte[] ori_date, byte[] ori_authno, byte[] tran_serial, byte[] idno, byte[] amt_flag, byte[] tax_amt, byte[] sfee_amt, byte[] free_amt, byte[] filler, byte[] rTranType, byte[] rErrCode, byte[] rCardno, byte[] rHalbu, byte[] rTamt, byte[] rTranDate, byte[] rTranTime, byte[] rAuthNo, byte[] rMerNo, byte[] rTranSerial, byte[] rIssueCard, byte[] rPurchaseCard, byte[] rSignPath, byte[] rMsg1, byte[] rMsg2, byte[] rMsg3, byte[] rMsg4, byte[] rFiller);

        [DllImport(@"C:\KOVAN\VPOS_Client.dll", CharSet = CharSet.Ansi)]
        private static extern int KovanAuthPlusSetInit();

        [DllImport(@"C:\KOVAN\VPOS_Client.dll", CharSet = CharSet.Ansi)]
        private static extern int KovanAuthPlusSet3KOVANPAY(byte[] req, byte[] res);

        [DllImport(@"C:\KOVAN\VPOS_Client.dll", CharSet = CharSet.Ansi)]
        private static extern int KovanAuthPlus(byte[] tcode, byte[] tid, byte[] halbu, byte[] tamt, byte[] ori_date, byte[] ori_authno, byte[] tran_serial, byte[] idno, byte[] amt_flag, byte[] tax_amt, byte[] sfee_amt, byte[] free_amt, byte[] filler, byte[] pgdata_flag, byte[] pgdata_len, byte[] pgdata, byte[] rTranType, byte[] rErrCode, byte[] rCardno, byte[] rHalbu, byte[] rTamt, byte[] rTranDate, byte[] rTranTime, byte[] rAuthNo, byte[] rMerNo, byte[] rTranSerial, byte[] rIssueCard, byte[] rPurchaseCard, byte[] rSignPath, byte[] rMsg1, byte[] rMsg2, byte[] rMsg3, byte[] rMsg4, byte[] rFiller, byte[] rPdata_len, byte[] rPgdata);

        [DllImport(@"C:\KOVAN\VPOS_Client.dll", CharSet = CharSet.Ansi)]
        private static extern int KovanAuthPlusSet(byte[] req, byte[] res);

        [DllImport(@"C:\KOVAN\VPOS_Client.dll", CharSet = CharSet.Ansi)]
        private static extern int KovanAuthPlusGet(byte[] res);

        [DllImport(@"C:\KOVAN\VPOS_Client.dll", CharSet = CharSet.Ansi)]
        private static extern int KovanAuthPlusSet2(byte[] req, byte[] res);

        [DllImport(@"C:\KOVAN\VPOS_Client.dll", CharSet = CharSet.Ansi)]
        private static extern int KovanAuthPlusGet2(byte[] res);

        /// <summary>
        /// PIN 값을 수신하는 함수 : 식별번호를 받거나, 고객의 비밀번호를 암호화 하는 함수
        /// </summary>
        /// <param name="rcvPin"> 요청/응답 전문</param>
        /// <param name="com_port">전자서명기기 연결 PORT</param>
        /// <param name="vanID">전자서명기기 업체 구분 코드</param>
        /// <returns></returns>
        [DllImport(@"\SignPad_dll.dll", CharSet = CharSet.Ansi)]
        private static extern int general_pin(byte[] rcvPin, int com_port, int vanID);

        /// <summary>
        /// 서명 데이터를 수신하는 함수
        /// </summary>
        /// <param name="rcvPin">요청/응답 전문</param>
        /// <param name="com_port">전자서명기기 연결 PORT</param>
        /// <param name="vanID">전자서명기기 업체 구분 코드</param>
        /// <returns></returns>
        [DllImport(@"\SignPad_dll.dll", CharSet = CharSet.Ansi)]
        private static extern int general_sign(byte[] eland, int com_port, int vanID);

        /// <summary>
        /// 승인 데이터 입력
        /// </summary>
        private Dictionary<string, string> inputData = null;

        /// <summary>
        /// 승인 결과 데이터
        /// </summary>
        private Dictionary<string, string> outputData = null;

        /// <summary>
        /// 승인 결과 데이터 가져옵니다.
        /// </summary>
        public Dictionary<string, string> OutputData
        {
            get { return this.outputData; }
        }

        public UsingVposAgent (){
            // 초기화 처리 



        }
        public UsingVposAgent(Dictionary<string, string> inputData)
            :this()
        {
            this.inputData = inputData;
            

        }

        public void CatVirtualFormcs_Shown(Dictionary<string, string> inputData)
        {
            this.outputData = new Dictionary<string, string>();
            Encoding encoding = Encoding.GetEncoding(949);

            long lvalue = 0;
            // 요청 전문 생성 

            // [2][거래구분]
            byte[] tcode = new byte[2];
            if( string.IsNullOrEmpty(inputData["TCODE"].Trim()) ){
                outputData.Add("ERRCODE", "9999");		// 응답코드
                outputData.Add("ResultMessage", "승인 실패 [미확인 거래구분]");
                return;
            }else{
                tcode = encoding.GetBytes(inputData["TCODE"].PadRight(2 , ' '));
            }

            // [10][단말기번호]
            byte[] tid = new byte[2];
            if( string.IsNullOrEmpty(inputData["TID"].Trim()) ){
                outputData.Add("ERRCODE", "9999");		// 응답코드
                outputData.Add("ResultMessage", "승인 실패 [단말기번호 오류]");
                return;
            }else{
                tid = encoding.GetBytes(inputData["TID"].PadRight(10 , ' '));
            }

            // [2]할부개월
            byte[] halbu = new byte[2];
            if (inputData["TCODE"] == "41" || inputData["TCODE"] == "42")
            {	// 현금 영수증
                if ( !string.IsNullOrEmpty(inputData["HALBU"].Trim()) )
                    halbu = encoding.GetBytes(inputData["HALBU"]) ;
                else
                {
                    outputData.Add("ERRCODE", "9999");		// 응답코드
                    outputData.Add("ResultMessage", string.Format("승인 실패 [미지원 현금영수증 승인 코드:{0}]", inputData["HALBU"]));
                    return;
                }
            }else
            {
                if (string.IsNullOrEmpty(inputData["HALBU"].Trim()) || inputData["HALBU"] == "0" || inputData["HALBU"] == "1"){
                    halbu = encoding.GetBytes("00");
                }else{
                    halbu = encoding.GetBytes(inputData["HALBU"].PadLeft(2, '0'));
                }
            }

            // [9]승인금액
            byte[] tamt = new byte[9];
            lvalue = 0;
            if( !string.IsNullOrEmpty(inputData["TAMT"].Trim()) ){
                lvalue = long.Parse(inputData["TAMT"]);
                tamt = encoding.GetBytes(lvalue.ToString().PadLeft(9, '0'));
            }else{
                    outputData.Add("ERRCODE", "9999");		// 응답코드
                    outputData.Add("ResultMessage", string.Format("승인 실패 [승인금액 오류:{0}]", inputData["TAMT"]));
                    return;               
            }

            // [6]원거래일자
            byte[] ori_date = new byte[6];
            if (inputData["TCODE"] == "S0" || 
                inputData["TCODE"] == "41" || 
                inputData["TCODE"] == "P0" || 
                inputData["TCODE"] == "Z0" || 
                inputData["TCODE"] == "Q0" || 
                inputData["TCODE"] == "E0")
                ori_date = encoding.GetBytes("".PadRight(6, ' '));
            else
                ori_date = encoding.GetBytes(inputData["ORI_DATE"].Trim().PadRight(6, ' '));

            // [12]원거래승인번호
            byte[] ori_authno = new byte[12];
            if (inputData["TCODE"] == "S0" || 
                inputData["TCODE"] == "41" || 
                inputData["TCODE"] == "P0" || 
                inputData["TCODE"] == "Z0" || 
                inputData["TCODE"] == "Q0" || 
                inputData["TCODE"] == "E0")
                ori_authno = encoding.GetBytes("".PadRight(12, ' '));
            else
                ori_authno = encoding.GetBytes(inputData["ORI_AUTHNO"].Trim().PadRight(12, ' '));

            // [12]거래일련번호
            byte[] tran_serial = new byte[12];
            if ( string.IsNullOrEmpty(inputData["MSG_TRACE"]) )
                tran_serial = encoding.GetBytes(DateTime.Now.ToString("HHmmss").PadRight(12, ' '));
            else
                tran_serial = encoding.GetBytes(inputData["MSG_TRACE"].PadRight(12, ' '));

            // [33]IDNO
            byte[] idno = new byte[33];
            if (!string.IsNullOrEmpty(inputData["IDNO"]) )
            {
                idno = encoding.GetBytes(inputData["IDNO"].PadRight(33, ' '));
            }
            else
                idno = encoding.GetBytes(string.Empty.PadLeft(33, ' '));

            // [3]미사용
            byte[] amt_flag = new byte[3];
            if (!string.IsNullOrEmpty(inputData["AMT_FLAG"]) )
            {
                amt_flag = encoding.GetBytes(inputData["AMT_FLAG"].PadRight(3, ' '));
            }
            else
            {
                amt_flag = encoding.GetBytes(string.Empty.PadLeft(3, ' '));
            }

            // [9]세금
            byte[] tax_amt = new byte[9];
            lvalue = 0;
            if( !string.IsNullOrEmpty(inputData["TAX_AMT"].Trim()) ){
                lvalue = long.Parse(inputData["TAX_AMT"]);
                tax_amt = encoding.GetBytes(lvalue.ToString().PadLeft(9, '0'));
            }else{
                tax_amt = encoding.GetBytes(string.Empty.PadLeft(9, '0'));
            }

            // [9]봉사료
            byte[] sfee_amt = new byte[9];
            lvalue = 0;
            if( !string.IsNullOrEmpty(inputData["SVC_AMT"].Trim()) ){
                lvalue = long.Parse(inputData["SVC_AMT"]);
                sfee_amt = encoding.GetBytes(lvalue.ToString().PadLeft(9, '0'));
            }else{
                sfee_amt = encoding.GetBytes(string.Empty.PadLeft(9, '0'));
            }

            // [9]비과세
            byte[] free_amt = new byte[9];
            lvalue = 0;
            if( !string.IsNullOrEmpty(inputData["NONTAX_AMT"].Trim()) ){
                lvalue = long.Parse(inputData["NONTAX_AMT"]);
                free_amt = encoding.GetBytes(lvalue.ToString().PadLeft(9, '0'));
            }else{
                free_amt = encoding.GetBytes(string.Empty.PadLeft(9, '0'));
            }

            // [100]여유필드
            byte[] filler = new byte[100];
            if (!string.IsNullOrEmpty(inputData["FILLER"]) )
            {
                filler = encoding.GetBytes(inputData["FILLER"].PadRight(100, ' '));
            }
            else
                filler = encoding.GetBytes(string.Empty.PadLeft(100, ' '));

            byte[] pgdata_flag = encoding.GetBytes(string.Empty.PadLeft(2, ' '));
            byte[] pgdata_len = encoding.GetBytes(string.Empty.PadLeft(3, '0'));
            byte[] pgdata = encoding.GetBytes(string.Empty.PadLeft(500, ' '));

            //응답 변수

            byte[] rTranType = new byte[4];			 // 전문구분[4]:승인-0210, 취소-0430
            byte[] rErrCode = new byte[4];			 // 응답코드[4]
            byte[] rCardno = new byte[18];			 // 카드번호[18]:마스킹된 카드번호
            byte[] rHalbu = new byte[2];			 // 할부개월[2]
            byte[] rTamt = new byte[9];				 // 승인금액[9]
            byte[] rTranDate = new byte[6];			 // 승인일자[6]
            byte[] rTranTime = new byte[6];			 // 승인시간[6]
            byte[] rAuthNo = new byte[12];			 // 승인번호[12]
            byte[] rMerNo = new byte[15];			 // 가맹점번호[15]
            byte[] rTranSerial = new byte[12];		 // 거래일련번호[12]
            byte[] rIssueCard = new byte[30];		 // 발급사명[30]
            byte[] rPurchaseCard = new byte[30];	 // 매입사명[30]
            byte[] rSignPath = new byte[50];		 // 사인경로[50]:해당 경로에서 사인이미지 읽어서 전표출력
            byte[] rMsg1 = new byte[100];			 // 메시지1[100]:거절일때 거절메시지, 정상일때 메시지1
            byte[] rMsg2 = new byte[100];			 // 메시지2[100]:[현금영수증 메시지1, 정상일때 메시지2
            byte[] rMsg3 = new byte[100];			 // 메시지3[100]:[현금영수증 메시지2, 정상일때 메시지3
            byte[] rMsg4 = new byte[100];			 // 메시지4[100]:[현금영수증 메시지3, 정상일때 메시지4
            byte[] rFiller = new byte[102];			 // 여유필드[102]:[발급사코드(2)+매입사코드(2)+스페이스(98)
            byte[] rPdata_len = new byte[3];
            byte[] rPgdata = new byte[500];

            byte[] qrData512 = new byte[512];
            string tmpqrdataStr = string.Empty;
            byte[] qrData256 = new byte[256];

            byte[] output_data = new byte[1024];

            if (inputData["TCODE"] == "Q0" || 
                inputData["TCODE"] == "Q1" 
                )  //통합페이
            {
                if (!string.IsNullOrEmpty( inputData["SET_QR_DATA_512"]))
                {
                    tmpqrdataStr = string.Empty;
                    //tmpqrdataStr += inputData["SET_QR_DATA_512"].ToString().Trim().PadRight(512 - inputData["SET_QR_DATA_512"].ToString().Trim().Length, ' ');
                    tmpqrdataStr += inputData["SET_QR_DATA_512"].ToString().Trim() + string.Empty.PadRight(512 - inputData["SET_QR_DATA_512"].ToString().Trim().Length, ' ');
                    //tmpqrdataStr += tmpqrdataStr.PadRight(512 - inputData["SET_QR_DATA_512"].ToString().Trim().Length, ' ');

                    qrData512 = encoding.GetBytes(tmpqrdataStr);
                }
                else qrData512 = encoding.GetBytes(string.Empty.PadRight(512, ' '));
                
                
                
                if (!string.IsNullOrEmpty( inputData["SET_QR_DATA_256"]))
                {
                    tmpqrdataStr = string.Empty;
                    tmpqrdataStr += inputData["SET_QR_DATA_256"].ToString().Trim() + string.Empty.PadRight(256 - inputData["SET_QR_DATA_256"].ToString().Trim().Length, ' ');
                    //tmpqrdataStr += tmpqrdataStr.PadRight(256 - inputData["SET_QR_DATA_256"].ToString().Trim().Length, ' ');
                    //outputData.Add("ERRCODE", responseErrCode.Trim() + string.Empty.PadRight(4-responseErrCode.Trim().Length, ' '));		// 응답코드

                    qrData256 = encoding.GetBytes(tmpqrdataStr);
                }
                else qrData256 = encoding.GetBytes(string.Empty.PadRight(256, ' '));


            }
            else{
                qrData512 = encoding.GetBytes(string.Empty.PadRight(512, ' '));
                qrData256 = encoding.GetBytes(string.Empty.PadLeft(256, ' '));
            }

            try
            {
                string strkakaodisamt = string.Empty;
                int result = -1;
                result = KovanAuthPlusSetInit();
                
                if (inputData["TCODE"] == "Q0" || inputData["TCODE"] == "Q1") // 카카오페이
                {
                    result = KovanAuthPlusSet(qrData512, output_data);
                    if( result == -1){
                        outputData.Add("ERRCODE", "9999");		// 응답코드
                        outputData.Add("ResultMessage", "승인 실패 [QR데이터 오류]");
                        return;
                    }
                    result = KovanAuthPlusSet2(qrData256, output_data);
                    if( result == -1){
                        outputData.Add("ERRCODE", "9999");		// 응답코드
                        outputData.Add("ResultMessage", "승인 실패 [QR데이터 오류]");
                        return;
                    }

                }

                result = KovanAuthPlus(tcode, tid, halbu, tamt, ori_date, ori_authno, tran_serial, idno, amt_flag, tax_amt, sfee_amt, free_amt, filler, pgdata_flag, pgdata_len, pgdata, rTranType, rErrCode, rCardno, rHalbu, rTamt, rTranDate, rTranTime, rAuthNo, rMerNo, rTranSerial, rIssueCard, rPurchaseCard, rSignPath, rMsg1, rMsg2, rMsg3, rMsg4, rFiller, rPdata_len, rPgdata);
                
                if (result == 0)
                {
                    string responseTranType = encoding.GetString(rTranType);			// 전문구분[4]:승인-0210, 취소-0430
                    string responseErrCode = encoding.GetString(rErrCode);			// 응답코드[4]
                    string responseCardno = encoding.GetString(rCardno);				// 카드번호[18]:마스킹된 카드번호
                    string responseHalbu = encoding.GetString(rHalbu);				// 할부개월[2]
                    string responseTamt = encoding.GetString(rTamt);					// 승인금액[9]
                    string responseTranDate = encoding.GetString(rTranDate);			// 승인일자[6]
                    string responseTranTime = encoding.GetString(rTranTime);			// 승인시간[6]
                    string responseAuthNo = encoding.GetString(rAuthNo);				// 승인번호[12]
                    string responseMerNo = encoding.GetString(rMerNo);				// 가맹점번호[15]
                    string responseTranSerial = encoding.GetString(rTranSerial);		// 거래일련번호[12]
                    string responseIssueCard = encoding.GetString(rIssueCard);		// 발급사명[30]
                    string responsePurchaseCard = encoding.GetString(rPurchaseCard);	// 매입사명[30]
                    string responseSignPath = encoding.GetString(rSignPath);			// 사인경로[50]:해당 경로에서 사인이미지 읽어서 전표출력
                    string responseMsg1 = encoding.GetString(rMsg1);					// 메시지1[100]:거절일때 거절메시지, 정상일때 메시지1
                    string responseMsg2 = encoding.GetString(rMsg2);					// 메시지2[100]:[현금영수증 메시지1, 정상일때 메시지2
                    string responseMsg3 = encoding.GetString(rMsg3);					// 메시지3[100]:[현금영수증 메시지2, 정상일때 메시지3
                    string responseMsg4 = encoding.GetString(rMsg4);					// 메시지4[100]:[현금영수증 메시지3, 정상일때 메시지4
                    string responseFiller = encoding.GetString(rFiller);				// 여유필드[102]:[발급사코드(2)+매입사코드(2)+스페이스(98)
                    string responsePGLen = encoding.GetString(rPdata_len);              // PG 데이터 길이[3]
                    string responsePGData = encoding.GetString(rPgdata);              // PG 데이터 길이[3]
                    string responseIssueCode = string.Empty;								// 발급사코드
                    string responsePurchaseCode = string.Empty;								// 매입사코드

                    if (!string.IsNullOrEmpty(responseFiller) && responseFiller.Length > 0)
                    {
                        if (responseFiller.Length >= 2)
                            responseIssueCode = responseFiller.Substring(0, 2);

                        if (responseFiller.Length >= 4)
                            responsePurchaseCode = responseFiller.Substring(2, 2);
                    }

                    if (responseErrCode == "0000" || responseErrCode == "8999")
                    {	// 정상 승인 및 취소 요청

                            outputData.Add("ERRCODE", responseErrCode.Trim() + string.Empty.PadRight(4-responseErrCode.Trim().Length, ' '));		// 응답코드
                            outputData.Add("TRANTYPE", responseTranType.Trim() + string.Empty.PadRight(4 - responseTranType.Trim().Length, ' '));		// 응답코드
                            outputData.Add("CARDNO", responseCardno.Trim() + string.Empty.PadRight(18 - responseCardno.Trim().Length, ' '));		// 응답코드
                            outputData.Add("HALBU", responseHalbu.Trim() + string.Empty.PadRight(2 - responseHalbu.Trim().Length, ' '));		    // 응답코드
                            outputData.Add("TAMT", responseTamt.Trim() + string.Empty.PadRight(9 - responseTamt.Trim().Length, ' '));		    // 응답코드
                            outputData.Add("TRANDATE", responseTranDate.Trim() + string.Empty.PadRight(6 - responseTranDate.Trim().Length, ' '));		    // 응답코드
                            outputData.Add("TRANTIME", responseTranTime.Trim() + string.Empty.PadRight(6 - responseTranTime.Trim().Length, ' '));		    // 응답코드
                            outputData.Add("AUTHNO", responseAuthNo.Trim() + string.Empty.PadRight(12 - responseAuthNo.Trim().Length, ' '));		    // 응답코드
                            outputData.Add("MERNO", responseMerNo.Trim() + string.Empty.PadRight(15 - responseMerNo.Trim().Length, ' '));		    // 응답코드
                            outputData.Add("TRANSERIAL", responseTranSerial.Trim() + string.Empty.PadRight(12 - responseTranSerial.Trim().Length, ' '));		    // 응답코드
                            outputData.Add("ISSUECARD", responseIssueCard.Trim() + string.Empty.PadRight(30 - responseIssueCard.Trim().Length, ' '));		    // 응답코드
                            outputData.Add("PURCHASECARD", responsePurchaseCard.Trim() + string.Empty.PadRight(30 - responsePurchaseCard.Trim().Length, ' '));		    // 응답코드
                            outputData.Add("SIGNPATH", responseSignPath.Trim() + string.Empty.PadRight(50 - responseSignPath.Trim().Length, ' '));		    // 응답코드
                            outputData.Add("MSG1", responseMsg1.Trim() + string.Empty.PadRight(100 - responseMsg1.Trim().Length, ' '));		    // 응답코드
                            outputData.Add("MSG2", responseMsg2.Trim() + string.Empty.PadRight(100 - responseMsg2.Trim().Length, ' '));		    // 응답코드
                            outputData.Add("MSG3", responseMsg3.Trim() + string.Empty.PadRight(100 - responseMsg3.Trim().Length, ' '));		    // 응답코드
                            outputData.Add("MSG4", responseMsg4.Trim() + string.Empty.PadRight(100 - responseMsg4.Trim().Length, ' '));		    // 응답코드
                            outputData.Add("FILLER", responseFiller.Trim() + string.Empty.PadRight(102 - responseFiller.Trim().Length, ' '));		    // 응답코드
                            outputData.Add("PGLEN", responsePGLen.Trim() + string.Empty.PadRight(3 - responsePGLen.Trim().Length, ' '));		    // 응답코드
                            outputData.Add("PGDATA", responsePGData.Trim() + string.Empty.PadRight(500 - responsePGData.Trim().Length, ' '));		    // 응답코드
                            
                            // 바코드 정보 읽어 오기 
                            if (inputData["TCODE"] == "Q0" || inputData["TCODE"] == "Q1") // 카카오페이
                            {
                                Array.Clear(qrData512, 0, qrData512.Length);
                                Array.Clear(qrData256, 0, qrData256.Length);
                                KovanAuthPlusGet(qrData512);
                                KovanAuthPlusGet2(qrData256);

                                outputData.Add("QR_DATA_512", encoding.GetString(qrData512).Trim() + string.Empty.PadRight(512 - encoding.GetString(qrData512).Trim().Length, ' '));
                                outputData.Add("QR_DATA_256", encoding.GetString(qrData256).Trim() + string.Empty.PadRight(256 - encoding.GetString(qrData256).Trim().Length, ' '));
                            }
                            else
                            {
                                outputData.Add("QR_DATA_512", string.Empty.PadRight(512 , ' '));
                                outputData.Add("QR_DATA_256", string.Empty.PadRight(256 , ' '));
                            }

                        return;
                    }
                    else
                    {
                        outputData.Add("ERRCODE", responseErrCode.Trim() + string.Empty.PadRight(4 - responseErrCode.Trim().Length, ' '));		// 응답코드
                        if (responseErrCode == "8324")
                        {
                            outputData.Add("ResultMessage", "승인 실패 : " + responseMsg1.Trim() + responseMsg2.Trim());
                        }else
                            outputData.Add("ResultMessage", "승인 실패 : " + this.GetErrorMessage(responseErrCode));
                        
                        return;
                    }
                }
                else
                {
                    outputData.Add("ERRCODE", "9999");		// 응답코드
                    outputData.Add("ResultMessage", this.GetConnectErrorMessage(result));
                    
                    return;
                }
            }
            catch (Exception ex)
            {
                outputData.Add("ERRCODE", "9998" );		// 응답코드
                outputData.Add("ResultMessage", string.Format("Exception : {0}", ex.Message));
                
                return;
            }
        }


        /// <summary>
        /// 에러 메시지 가져오기
        /// </summary>
        /// <param name="responseErrCode"></param>
        /// <returns></returns>
        private string GetErrorMessage(string responseErrCode)
        {
            switch (responseErrCode)
            {
                case "4001":
                    return "리더기 포트 미설정";
                case "4002":
                    return "무결성점검 실패";
                case "4003":
                    return "거래요청시 TID가 vpos 설정한 TID와 다름";
                case "4004":
                    return "포트 열기 실패";
                case "4005":
                    return "vpos에 설정한 시리얼번호와 리더기 시리얼번호가 다름";
                case "4006":
                    return "승인서버 IP 형식이 맞지 않음";
                case "4007":
                    return "승인서버 포트 미설정";
                case "5001":
                    return "IC 리딩 실패";
                case "5002":
                    return "거래금액이 없음";
                case "5003":
                    return "IC 카드 인식불가";
                case "5004":
                    return "IC 카드 삽입되어 있음";
                case "5005":
                    return "거래 flow에서 상황에 맞지않는 명령이 호출";
                case "5006":
                    return "IC카드인데 MS리딩이 발생";
                case "5007":
                    return "IC카드 처리중 강제로 카드붂리";
                case "5008":
                    return "기타오류";
                case "5009":
                    return "기타오류(정의되지 않은 오류)";
                case "5010":
                    return "은련카드를 일반거래요청";
                case "5011":
                    return "리더기가 카드구분(IC,MS) 판단하지 못함";
                case "5012":
                    return "pin 미입력";
                case "5013":
                    return "리더기 연결이 끊어짐";
                case "5014":
                    return "리더기 사용불가";
                case "5015":
                    return "리더기 명령 기타응답";
                case "5016":
                    return "은련카드 AID 선택오류";
                case "5017":
                    return "현금영수증 고객정보 미입력";
                case "7001":
                    return "DLL 파일이 없음";
                case "7002":
                    return "DLL 명령어가 없음";
                case "7003":
                    return "pin 암호화 오류";
                case "8324":
                    return "거래거절(승인서버응답)";
                case "8555":
                    return "거래거절(승인서버응답이 공백)";
                case "8899":
                    return "거래거절(승인서버응답이 기타)";
                case "8001":
                    return "거래중 사용자 중단 요청";
                case "8002":
                    return "거래중 망취소(리더기판단)";
                case "8003":
                    return "거래중 망취소(응답 미수신)";
                case "8004":
                    return "거래거절(승인서버 기타응답)";
                case "8005":
                    return "pin거래 MS불가";
                case "9999":
                    return "기타";
                default:
                    return string.Format("알 수 없는 오류[{0}]", responseErrCode);
            }
        }

        /// <summary>
        /// 승인 요청 연결 실패
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private string GetConnectErrorMessage(int result)
        {
            switch (result)
            {
                case -2:
                    return string.Format("승인 실패 : 프로그램 통신 접속 실패 [{0}]", result);
                case -3:
                    return string.Format("승인 실패 : 프로그램 통신 전송 실패 [{0}]", result);
                case -4:
                    return string.Format("승인 실패 : 프로그램 수신 준비 실패 [{0}]", result);
                case -5:
                    return string.Format("승인 실패 : 프로그램 통신 수신 실패 [{0}]", result);
                case -6:
                    return string.Format("승인 실패 : 거래구분오류 [{0}]", result);
                case -7:
                    return string.Format("승인 실패 : 할부개월오류 [{0}]", result);
                case -8:
                    return string.Format("승인 실패 : 취소시 원거래일오류 [{0}]", result);
                case -9:
                    return string.Format("승인 실패 : 취소시 원승인번호 오류 [{0}]", result);
                case -10:
                    return string.Format("승인 실패 : 거래일련번호 오류 [{0}]", result);
                default:
                    return string.Format("승인 실패 : 승인 연결 오류 [{0}]", result);
            }
        }

    }

}
