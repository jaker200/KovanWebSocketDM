using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    class MsgStructUsePOS
    {

        public class ReqPacktFromPOS
        {
            public PacketHeader header = new PacketHeader();
            public ReqPacketBody body = new ReqPacketBody();
            
        }

        public class RespPacktToPOS
        {
            public PacketHeader header = new PacketHeader();
            public ReqPacketBody body = new ReqPacketBody();
            
        }
        public class PacketHeader   //헤더
        {
            public string LENGTH { get; set; }
            public string MSG_VERSION { get; set; }
            public string TCODE { get; set; }
            public string MSG_TRACE { get; set; }
            public string DATA_TYPE { get; set; }
            

        }
        public class ReqPacketBody   // 요청전문 헤더
        {
            public string TID { get; set; }
            public string HALBU { get; set; }
            public string TAMT { get; set; }
            public string ORI_DATE { get; set; }
            public string ORI_AUTHNO { get; set; }
            public string IDNO { get; set; }
            public string AMT_FLAG { get; set; }
            public string TAX_AMT { get; set; }
            public string SVC_AMT { get; set; }
            public string NONTAX_AMT { get; set; }
            public string FILLER { get; set; }
            //public string PG_FLAG { get; set; }
            //public string PG_LEN { get; set; }
            //public string PG_DATA { get; set; }
            public string SET_QR_DATA_512 { get; set; }
            public string SET_QR_DATA_256 { get; set; }
            
        }

        public class RespPacketBody   // 응답전문
        {
            public string TID { get; set; }
            public string HALBU { get; set; }
            public string TAMT { get; set; }
            public string ORI_DATE { get; set; }
            public string ORI_AUTHNO { get; set; }
            public string IDNO { get; set; }
            public string TAX_AMT { get; set; }
            public string SVC_AMT { get; set; }
            public string NONTAX_AMT { get; set; }
            public string FILLER { get; set; }
            public string PG_FLAG { get; set; }
            public string PG_LEN { get; set; }
            public string PG_DATA { get; set; }
            public string SET_QR_DATA1 { get; set; }
            public string SET_QR_DATA2 { get; set; }
            
        }
    }
}
