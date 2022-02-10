using Microsoft.AspNetCore.Mvc;

namespace Roundpay_Robo.AppCode.Model.ProcModel
{
    public class SMSSendResp
    {
        public int ResultCode { get; set; }
        public string Msg { get; set; }
        public int SMSID { get; set; }
        public int APIID { get; set; }
        public string SMS { get; set; }
        public string SmsURL { get; set; }
        public string APIMethod { get; set; }
        public string TransactionID { get; set; }
        public string MobileNo { get; set; }
        public bool IsLapu { get; set; }
        public bool IsSend { get; set; }
        public string ApiResp { get; set; }
    }
}
