using Microsoft.AspNetCore.Mvc;
using Roundpay_Robo.AppCode.Configuration;
using Roundpay_Robo.AppCode.DB;
using Roundpay_Robo.AppCode.DL;
using Roundpay_Robo.AppCode.Interfaces;
using Roundpay_Robo.AppCode.Model;
using Roundpay_Robo.AppCode.Model.ProcModel;
using Roundpay_Robo.AppCode.StaticModel;
using Roundpay_Robo.AppCode.WebRequest;
using RoundpayFinTech.AppCode.Model;
using System.Data;

namespace Roundpay_Robo.AppCode.MiddleLayer
{
    public class SendSMSML : ISendSMSML
    {
        private readonly IConfiguration Configuration;
        private readonly IHttpContextAccessor _accessor;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly IRequestInfo _rinfo;
        private readonly WebsiteInfo _WInfo;
        private readonly LoginResponse _lr;
        public SendSMSML(IDAL dal)
        {
            _dal = dal;
        }
        public SendSMSML(IHttpContextAccessor accessor, Microsoft.AspNetCore.Hosting.IHostingEnvironment env, bool IsInSession = true)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _rinfo = new RequestInfo(_accessor, _env);
            if (IsInSession)
            {
                _session = _accessor.HttpContext.Session;
                bool IsProd = _env.IsProduction();
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory());
                builder.AddJsonFile((IsProd ? "appsettings.json" : "appsettings.Development.json"));
                builder.AddEnvironmentVariables();
                Configuration = builder.Build();
                _WInfo = new LoginML(_accessor, _env).GetWebsiteInfo();
                _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
            }

        }
        public void SendUserReg(string LoginID, string Password, string MobileNo, string EmailID, int WID, string Logo)
        {
            try
            {
                DataTable Tp_ReplaceKeywords = new DataTable();
                Tp_ReplaceKeywords.Columns.Add("Keyword", typeof(string));
                Tp_ReplaceKeywords.Columns.Add("ReplaceValue", typeof(string));
                Tp_ReplaceKeywords.Rows.Add(AddKeyAndValue(MessageTemplateKeywords.LoginID, LoginID));
                Tp_ReplaceKeywords.Rows.Add(AddKeyAndValue(MessageTemplateKeywords.Password, Password));
                Tp_ReplaceKeywords.Rows.Add(AddKeyAndValue(MessageTemplateKeywords.PinPassword, string.Empty));
                SendSMS(Tp_ReplaceKeywords, MobileNo, EmailID, WID, true, "User Registration", MessageFormat.Registration, Logo);
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SendUserReg",
                    Error = ex.Message,
                    LoginTypeID = 0,
                    UserId = WID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }

        }
        public void SendSMS(DataTable Tp_ReplaceKeywords, string MobileNo, string EmailID, int WID, bool WithMail, string MailSub, int FormatType, string Logo)
        {
            if (Tp_ReplaceKeywords == null)
            {
                Tp_ReplaceKeywords = new DataTable();
                Tp_ReplaceKeywords.Columns.Add("Keyword", typeof(string));
                Tp_ReplaceKeywords.Columns.Add("ReplaceValue", typeof(string));
                Tp_ReplaceKeywords.Rows.Add(AddKeyAndValue(MessageTemplateKeywords.LoginID, "0"));
            }
            var procSendSMS = new SMSSendREQ
            {
                FormatType = FormatType,
                MobileNo = MobileNo,
                Tp_ReplaceKeywords = Tp_ReplaceKeywords,
                WID = WID
            };
            SMSSendResp smsResponse = (SMSSendResp)SendSMS(procSendSMS);
            if (WithMail)
            {
                if (!string.IsNullOrEmpty(smsResponse.SMS) && !string.IsNullOrEmpty(EmailID))
                {
                    IEmailML emailManager = new EmailML(_dal);
                    emailManager.SendMail(EmailID, null, MailSub, smsResponse.SMS, WID, Logo);
                }
            }
        }
        public object SendSMS(SMSSendREQ _req)
        {
            IProcedure _p = new ProcSendSMS(_dal);
            object _o = _p.Call(_req);
            SMSSendResp _resp = (SMSSendResp)_o;
            _resp.IsSend = false;
            if (_resp.ResultCode > 0)
            {
                string ApiResp = "";
                try
                {
                    if (_resp.APIMethod == "GET")
                    {
                        ApiResp = AppWebRequest.O.CallUsingWebClient_GET(_resp.SmsURL, 0);
                        _resp.IsSend = true;
                    }
                    else if (_resp.APIMethod == "POST")
                    {
                        //To be implemented
                        _resp.IsSend = true;
                    }
                }
                catch (Exception ex)
                {
                    ApiResp = "Exception Occured! " + ex.Message;
                }
                _resp.ApiResp = ApiResp;
                SMSUpdateREQ updateRequest = null;
                updateRequest = new SMSUpdateREQ
                {
                    Response = _resp.ApiResp,
                    ResponseID = "error",
                    Status = SMSResponseTYPE.SEND,
                    SMSID = _resp.SMSID
                };

                UpdateSMSResponse(updateRequest);
            }
            return _resp;
        }
        public void UpdateSMSResponse(SMSUpdateREQ _req)
        {
            IProcedure _p = new ProcSendSMSUpdate(_dal);
            object _o = _p.Call(_req);
        }

        private DataRow AddKeyAndValue(object loginID1, string loginID2)
        {
            throw new NotImplementedException();
        }

        public void SendUserForget(string LoginID, string Password, string Pin, string MobileNo, string EmailID, int WID, string Logo)
        {
            throw new NotImplementedException();
        }

        public string CallSendSMSAPI(SendSMSRequest _req)
        {
            throw new NotImplementedException();
        }
    }
    
}
