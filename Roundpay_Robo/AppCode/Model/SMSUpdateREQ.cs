using Microsoft.AspNetCore.Mvc;

namespace Roundpay_Robo.AppCode.Model
{
    public class SMSUpdateREQ
    {
        public int SMSID { get; set; }
        public int Status { get; set; }
        public string Response { get; set; }
        public string ResponseID { get; set; }
    }
}
