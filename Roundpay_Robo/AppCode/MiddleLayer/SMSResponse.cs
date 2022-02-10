namespace Roundpay_Robo.AppCode.MiddleLayer
{
    internal class SMSResponse
    {
        public string ReqURL { get; set; }
        public string Response { get; set; }
        public string ResponseID { get; set; }
        public int Status { get; set; }
        public object SMSID { get; set; }
        public string MobileNo { get; set; }
        public string TransactionID { get; set; }
        public string SMS { get; set; }
        public int WID { get; set; }
        public int SocialAlertType { get; set; }
    }
}