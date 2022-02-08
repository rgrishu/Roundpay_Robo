using Roundpay_Robo.AppCode.StaticModel;
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Roundpay_Robo.AppCode.Model.ProcModel;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class TransactionStatus
    {
        public int Status { get; set; }
        public string VendorID { get; set; }
        public string OperatorID { get; set; }
        public int APIID { get; set; }
        public int APIType { get; set; }
        public int UserID { get; set; }
        public int TID { get; set; }
        public string APIOpCode { get; set; }
        public string APIName { get; set; }
        public bool APICommType { get; set; }
        public decimal APIComAmt { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public string ExtraParam { get; set; }
        public string APIGroupCode { get; set; }
        public string APIErrorCode { get; set; }
        public string APIMsg { get; set; }
        public string RefferenceID { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMsg { get; set; }
        public string Balance { get; set; }
        public bool IsResend { get; set; }
        public bool IsReLoginLapu { get; set; }
        public int SwitchingID { get; set; }
    }
    public class RechargeAPIHit
    {
      //  public JRRechargeReq JRRechReq { get; set; }
        public APIDetail aPIDetail { get; set; }
        public string Response { get; set; }
        public bool IsException { get; set; }
        public int LoginID { get; set; }
        public int ServiceID { get; set; }
        //public CyberPlatRequestModel CPTRNXRequest { get; set; }
        public string SessionNo { get; set; }
        public string MGPSARequestID { get; set; }
    }
}