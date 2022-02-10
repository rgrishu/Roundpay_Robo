using Roundpay_Robo.AppCode.DL;
using Roundpay_Robo.AppCode.Interfaces;
using Roundpay_Robo.AppCode.Model;
using Roundpay_Robo.AppCode.Model.ProcModel;
using Roundpay_Robo.AppCode.StaticModel;
using System.Text;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;

namespace Roundpay_Robo.AppCode.MiddleLayer
{
    public class AlertML : BaseML, IAlertML
    {
        private readonly IResourceML _resourceML;
        private readonly ISendSMSML _sendSMSML;
        public AlertML(IHttpContextAccessor accessor, Microsoft.AspNetCore.Hosting.IHostingEnvironment env, bool IsInSession = true) : base(accessor, env, IsInSession)
        {
            _resourceML = new ResourceML(_accessor, _env);
            _sendSMSML = new SendSMSML(_dal);
        }


        
        public IResponseStatus SendSMS(int APIID, AlertReplacementModel model)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.SUCCESS
            };
            if (string.IsNullOrEmpty(model.MobileNos))
            {
                _res.Msg = "Invaild MobileMo";
                return _res;
            }
            var MobileList = !string.IsNullOrEmpty(model.MobileNos) ? model.MobileNos.Split(',').ToList() : new List<string>();
            if (MobileList == null || MobileList.Count < 1)
            {
                _res.Msg = "Invaild MobileMo";
                return _res;
            }
            if (string.IsNullOrEmpty(model.Msg))
            {
                _res.Msg = "Fill message";
                return _res;
            }
            if (APIID < 1)
            {
                _res.Msg = "Invaild API";
                return _res;
            }

            string sendRes = string.Empty;
            ISMSAPIML ML = new APIML(_accessor, _env);
            var detail = ML.GetSMSAPIDetailByID(APIID);
            var _l = MobileList.LastOrDefault();
            if (string.IsNullOrEmpty(_l))
            {
                MobileList.RemoveAt(MobileList.Count - 1);
            }
            if (detail != null)
            {
                model.Msg = GetFormatedMessage(model.Msg, model);
                var smsSetting = new SMSSetting
                {
                    APIID = detail.ID,
                    SMSID = detail.ID,
                    APIMethod = detail.APIMethod,
                    IsLapu = false,
                    URL = detail.URL,
                    MobileNos = string.Join(",", MobileList),
                    WID = _lr != null ? _lr.WID : 0,
                    SMS = model.Msg
                };
                var SMSURL = new StringBuilder(smsSetting.URL);
                if (!detail.IsMultipleAllowed)
                {
                    foreach (var item in MobileList)
                    {

                        SMSURL.Clear();
                        SMSURL.Append(smsSetting.URL);
                        SMSURL.Replace("{SENDERID}", smsSetting.SenderID ?? "");
                        SMSURL.Replace("{TO}", item);
                        SMSURL.Replace("{MESSAGE}", smsSetting.SMS);

                        var p = new SendSMSRequest()
                        {
                            APIMethod = smsSetting.APIMethod,
                            SmsURL = SMSURL.ToString(),
                            IsLapu = false
                        };
                        if (detail.APIType == 2)
                        {
                            sendRes = "Only save message";
                        }
                        else
                        {
                            sendRes = _sendSMSML.CallSendSMSAPI(p);
                        }

                        var _Response = new Roundpay_Robo.AppCode.MiddleLayer.SMSResponse
                        {
                            ReqURL = SMSURL.ToString(),
                            Response = Convert.ToString(sendRes),
                            ResponseID = "",
                            Status = SMSResponseTYPE.SEND,
                            SMSID = smsSetting.SMSID,
                            MobileNo = item,
                            TransactionID = "",
                            SMS = smsSetting.SMS,
                            WID = smsSetting.WID
                        };
                        SaveSMSResponse(_Response);
                    }
                }
                else
                {
                    SMSURL.Replace("{SENDERID}", smsSetting.SenderID ?? "");
                    SMSURL.Replace("{TO}", smsSetting.MobileNos);
                    SMSURL.Replace("{MESSAGE}", smsSetting.SMS);
                    var p = new SendSMSRequest()
                    {
                        APIMethod = smsSetting.APIMethod,
                        SmsURL = SMSURL.ToString(),
                        IsLapu = false
                    };
                    sendRes = _sendSMSML.CallSendSMSAPI(p);
                    var _Response = new Roundpay_Robo.AppCode.MiddleLayer.SMSResponse
                    {
                        ReqURL = SMSURL.ToString(),
                        Response = Convert.ToString(sendRes),
                        ResponseID = "",
                        Status = SMSResponseTYPE.SEND,
                        SMSID = smsSetting.SMSID,
                        MobileNo = smsSetting.MobileNos,
                        TransactionID = "",
                        SMS = smsSetting.SMS,
                        WID = smsSetting.WID
                    };
                    SaveSMSResponse(_Response);
                }
            }
            _res.Statuscode = ErrorCodes.One;
            return _res;
        }

        private void SaveSMSResponse(Roundpay_Robo.AppCode.MiddleLayer.SMSResponse response)
        {
            throw new NotImplementedException();
        }





       
        public IResponseStatus FundDebitSMS(AlertReplacementModel param)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string sendRes = string.Empty;
            string SMS = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = MessageFormat.FundDebit
            };
            IProcedure _proc = new ProcGetSMSSettingByFormat(_dal);
            var smsSetting = (SMSSetting)_proc.Call(Sqlparam);
            if (smsSetting.IsEnableSMS)
            {
                bool IsNoTemplate = true;
                StringBuilder sbUrl = new StringBuilder(smsSetting.URL);
                if (string.IsNullOrEmpty(smsSetting.Template))
                {
                    sendRes = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (smsSetting.SMSID == 0)
                    {
                        sendRes = "No API Found";
                    }

                    SMS = GetFormatedMessage(smsSetting.Template, param);
                    if (smsSetting.SMSID > 0 && !string.IsNullOrEmpty(smsSetting.URL))
                    {
                        sbUrl.Replace("{SENDERID}", smsSetting.SenderID);
                        sbUrl.Replace("{TO}", param.UserMobileNo);
                        sbUrl.Replace("{MESSAGE}", SMS);
                        var p = new SendSMSRequest()
                        {
                            APIMethod = smsSetting.APIMethod,
                            SmsURL = sbUrl.ToString()
                        };
                        sendRes = _sendSMSML.CallSendSMSAPI(p);
                    }
                }
                var _Response = new Roundpay_Robo.AppCode.MiddleLayer.SMSResponse
                {
                    ReqURL = sbUrl.ToString(),
                    Response = Convert.ToString(sendRes),
                    ResponseID = "",
                    Status = SMSResponseTYPE.SEND,
                    SMSID = smsSetting.SMSID,
                    MobileNo = param.UserMobileNo,
                    TransactionID = param.TransactionID,
                    SMS = SMS,
                    WID = param.WID
                };
                SaveSMSResponse(_Response);
            }

            return _res;
        }

        
        public IResponseStatus OTPSMS(AlertReplacementModel param)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string sendRes = string.Empty;
            string SMS = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = MessageFormat.OTP
            };
            IProcedure _proc = new ProcGetSMSSettingByFormat(_dal);
            var smsSetting = (SMSSetting)_proc.Call(Sqlparam);

            if (smsSetting.IsEnableSMS)
            {
                StringBuilder sbUrl = new StringBuilder(smsSetting.URL);
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(smsSetting.Template))
                {
                    sendRes = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (smsSetting.SMSID == 0)
                    {
                        sendRes = "No API Found";
                    }
                    SMS = GetFormatedMessage(smsSetting.Template, param);
                    if (smsSetting.SMSID > 0 && !string.IsNullOrEmpty(smsSetting.URL))
                    {
                        sbUrl.Replace("{SENDERID}", smsSetting.SenderID);
                        sbUrl.Replace("{TO}", param.UserMobileNo);
                        sbUrl.Replace("{MESSAGE}", SMS);
                        var p = new SendSMSRequest
                        {
                            APIMethod = smsSetting.APIMethod,
                            SmsURL = sbUrl.ToString()
                        };
                        sendRes = _sendSMSML.CallSendSMSAPI(p);
                    }
                }
                var _Response = new Roundpay_Robo.AppCode.MiddleLayer.SMSResponse
                {
                    ReqURL = sbUrl.ToString(),
                    Response = Convert.ToString(sendRes),
                    ResponseID = "",
                    Status = SMSResponseTYPE.SEND,
                    SMSID = smsSetting.SMSID,
                    MobileNo = param.UserMobileNo,
                    TransactionID = "",
                    SMS = SMS,
                    WID = param.WID
                };
                SaveSMSResponse(_Response);
            }
            return _res;
        }

        
        public IResponseStatus ForgetPasswordSMS(AlertReplacementModel param)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string sendRes = string.Empty;
            string SMS = string.Empty;
            var Sqlparam = new CommonReq
            {
                LoginID = param.LoginID,
                CommonInt = MessageFormat.ForgetPass,
                CommonInt2 = param.WID
            };
            IProcedure _proc = new ProcGetSMSSettingByFormat(_dal);
            var smsSetting = (SMSSetting)_proc.Call(Sqlparam);
            StringBuilder sbUrl = new StringBuilder(smsSetting.URL);
            if (smsSetting.IsEnableSMS)
            {
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(smsSetting.Template))
                {
                    sendRes = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (smsSetting.SMSID == 0)
                    {
                        sendRes = "No API Found";
                    }
                    SMS = GetFormatedMessage(smsSetting.Template, param);
                    if (smsSetting.SMSID > 0 && !string.IsNullOrEmpty(smsSetting.URL))
                    {

                        sbUrl.Replace("{SENDERID}", smsSetting.SenderID);
                        sbUrl.Replace("{TO}", param.LoginMobileNo);
                        sbUrl.Replace("{MESSAGE}", SMS);
                        var p = new SendSMSRequest()
                        {
                            APIMethod = smsSetting.APIMethod,
                            SmsURL = sbUrl.ToString()
                        };
                        sendRes = _sendSMSML.CallSendSMSAPI(p);
                    }
                }
                var _Response = new Roundpay_Robo.AppCode.MiddleLayer.SMSResponse
                {
                    ReqURL = sbUrl.ToString(),
                    Response = Convert.ToString(sendRes),
                    ResponseID = "",
                    Status = SMSResponseTYPE.SEND,
                    SMSID = smsSetting.SMSID,
                    MobileNo = param.LoginMobileNo,
                    TransactionID = "",
                    SMS = SMS,
                    WID = param.WID
                };
                SaveSMSResponse(_Response);
            }
            return _res;
        }
        
        public IResponseStatus RegistrationSMS(AlertReplacementModel param)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string sendRes = string.Empty;
            string SMS = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = MessageFormat.Registration
            };
            IProcedure _proc = new ProcGetSMSSettingByFormat(_dal);
            var smsSetting = (SMSSetting)_proc.Call(Sqlparam);
            if (smsSetting.IsEnableSMS)
            {
                bool IsNoTemplate = true;
                StringBuilder sbUrl = new StringBuilder(smsSetting.URL);
                if (string.IsNullOrEmpty(smsSetting.Template))
                {
                    sendRes = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (smsSetting.SMSID == 0)
                    {
                        sendRes = "No API Found";
                    }
                    SMS = GetFormatedMessage(smsSetting.Template, param);
                    if (smsSetting.SMSID > 0 && !string.IsNullOrEmpty(smsSetting.URL))
                    {
                        sbUrl.Replace("{SENDERID}", smsSetting.SenderID);
                        sbUrl.Replace("{TO}", param.UserMobileNo);
                        sbUrl.Replace("{MESSAGE}", SMS);
                        var p = new SendSMSRequest
                        {
                            APIMethod = smsSetting.APIMethod,
                            SmsURL = sbUrl.ToString()
                        };
                        sendRes = _sendSMSML.CallSendSMSAPI(p);
                    }
                }
                var _Response = new Roundpay_Robo.AppCode.MiddleLayer.SMSResponse
                {
                    ReqURL = sbUrl.ToString(),
                    Response = Convert.ToString(sendRes),
                    ResponseID = "",
                    Status = SMSResponseTYPE.SEND,
                    SMSID = smsSetting.SMSID,
                    MobileNo = param.UserMobileNo,
                    TransactionID = "",
                    SMS = SMS,
                    WID = param.WID
                };
                SaveSMSResponse(_Response);
            }
            return _res;
        }
       
        private string APINotFoundResponse(int SocialType = 0)
        {
            string result = "API Not Found.";
            switch (SocialType)
            {
                case 1:
                    result = "No Whatsapp API Found";
                    break;
                case 2:
                    result = "No Hangout API Found";
                    break;
                case 3:
                    result = "No Telegram API Found";
                    break;
            }
            return result;
        }
        private string GetFormatedMessage(string Template, AlertReplacementModel Replacements)
        {
            StringBuilder sb = new StringBuilder(Template);

            sb.Replace("{FromUserName}", Replacements.LoginUserName);
            sb.Replace("{FromUserMobile}", Replacements.LoginMobileNo);
            sb.Replace("{FromUserID}", Replacements.LoginPrefix + Replacements.LoginID.ToString());
            sb.Replace("{ToUserMobile}", Replacements.UserMobileNo);
            sb.Replace("{ToUserID}", Replacements.UserPrefix + Replacements.UserID.ToString());
            sb.Replace("{ToUserName}", Replacements.UserName);
            sb.Replace("{UserName}", Replacements.UserName);
            sb.Replace("{Mobile}", Replacements.UserMobileNo);
            sb.Replace("{UserMobile}", Replacements.UserMobileNo);
            sb.Replace("{Amount}", Convert.ToString(Replacements.Amount));
            sb.Replace("{BalanceAmount}", Convert.ToString(Replacements.BalanceAmount));
            sb.Replace("{UserBalanceAmount}", Convert.ToString(Replacements.UserCurrentBalance));
            sb.Replace("{LoginBalanceAmount}", Convert.ToString(Replacements.LoginCurrentBalance));
            sb.Replace("{Operator}", Replacements.Operator);
            sb.Replace("{OperatorName}", Replacements.Operator);
            sb.Replace("{Company}", Replacements.Company);
            sb.Replace("{CompanyName}", Replacements.Company);
            sb.Replace("{CompanyDomain}", Replacements.CompanyDomain);
            sb.Replace("{CompanyAddress}", Replacements.CompanyAddress);
            sb.Replace("{BrandName}", Replacements.BrandName);
            sb.Replace("{OutletName}", Replacements.OutletName);
            sb.Replace("{SupportNumber}", Replacements.SupportNumber);
            sb.Replace("{SupportEmail}", Replacements.SupportEmail);
            sb.Replace("{AccountNumber}", Replacements.AccountNo);
            sb.Replace("{AccountsContactNo}", Replacements.AccountsContactNo);
            sb.Replace("{AccountEmail}", Replacements.AccountEmail);
            sb.Replace("{OTP}", Replacements.OTP);
            sb.Replace("{LoginID}", !String.IsNullOrEmpty(Replacements.CommonStr) ? Replacements.CommonStr : Replacements.LoginPrefix + Replacements.LoginID.ToString());
            sb.Replace("{Password}", Replacements.Password);
            sb.Replace("{PinPassword}", Replacements.PinPassword);
            sb.Replace("{AccountNo}", Replacements.AccountNo);
            sb.Replace("{LiveID}", Replacements.LiveID);
            sb.Replace("{TID}", Convert.ToString(Replacements.TID));
            sb.Replace("{TransactionID}", Replacements.TransactionID);
            sb.Replace("{BankRequestStatus}", Replacements.RequestStatus);
            sb.Replace(MessageTemplateKeywords.Message, Replacements.Message);
            sb.Replace(MessageTemplateKeywords.UserEmail, Replacements.EmailID);
            sb.Replace(MessageTemplateKeywords.SenderName, Replacements.SenderName);
            sb.Replace(MessageTemplateKeywords.TransMode, Replacements.TransMode);
            sb.Replace(MessageTemplateKeywords.UTRorRRN, Replacements.UTRorRRN);
            sb.Replace(MessageTemplateKeywords.IFSC, Replacements.IFSC);
            sb.Replace("{DATETIME}", Replacements.DATETIME);
            sb.Replace("{Duration}", Replacements.Duration);
            sb.Replace("{CouponCode}", Replacements.CouponCode);
            sb.Replace("{CouponQty}", Convert.ToString(Replacements.CouponQty));
            sb.Replace("{CouponValidty}", Convert.ToString(Replacements.CouponValdity));

            //sb.Replace(MessageTemplateKeywords.AccountNumber, Replacements.AccountNumber);
            return Convert.ToString(sb);
        }

        private string NumberNotFoundResponse(int SocialType = 0)
        {
            string result = "No number found.";
            switch (SocialType)
            {
                case 1:
                    result = "No whatsapp number found";
                    break;
                case 2:
                    result = "No hangout number found";
                    break;
                case 3:
                    result = "No telegram number found";
                    break;
            }
            return result;
        }

        public IResponseStatus ResendSMS(string SendTo, string msg, AlertReplacementModel param = null)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string sendRes = string.Empty;
            string SMS = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = _lr.LoginTypeID,
                CommonInt = _lr.WID
            };
            IProcedure _proc = new procGetApiIDforResend(_dal);
            var smsSetting = (SMSAPIDetail)_proc.Call(Sqlparam);
            StringBuilder sbUrl = new StringBuilder(smsSetting.URL);
            if (smsSetting.ID == 0)
            {
                _res.Msg = "No API Found";
                return _res;
            }
            // SMS = GetFormatedMessage(smsSetting.Template, param);
            if (smsSetting.ID > 0 && !string.IsNullOrEmpty(smsSetting.URL))
            {
                sbUrl.Replace("{SENDERID}", "");
                sbUrl.Replace("{TO}", SendTo);
                sbUrl.Replace("{MESSAGE}", msg);
                var p = new SendSMSRequest
                {
                    APIMethod = smsSetting.APIMethod,
                    SmsURL = sbUrl.ToString()
                };
                sendRes = _sendSMSML.CallSendSMSAPI(p);
            }

            var _Response = new Roundpay_Robo.AppCode.MiddleLayer.SMSResponse
            {
                ReqURL = sbUrl.ToString(),
                Response = Convert.ToString(sendRes),
                ResponseID = "",
                Status = SMSResponseTYPE.RESEND,
                SMSID = smsSetting.ID,
                MobileNo = SendTo,
                TransactionID = "",
                SMS = msg,
                WID = _lr.WID
            };
            SaveSMSResponse(_Response);

            return _res;
        }
        public IResponseStatus RegistrationEmail(AlertReplacementModel param)
        {
            bool IsSent = false;
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string EmailBody = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = MessageFormat.Registration
            };
            IProcedure _proc = new ProcGetEmailSettingByFormat(_dal);
            var mailSetting = (EmailSettingswithFormat)_proc.Call(Sqlparam);
            if (mailSetting.IsEnableEmail)
            {
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(mailSetting.EmailTemplate))
                {
                    EmailBody = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (string.IsNullOrEmpty(mailSetting.FromEmail))
                    {
                        EmailBody = "No Email Found";
                    }
                    EmailBody = GetFormatedMessage(mailSetting.EmailTemplate, param);
                    if (param.WID > 0)
                    {
                        if (!string.IsNullOrEmpty(mailSetting.FromEmail))
                        {
                            IEmailML emailManager = new EmailML(_dal);
                            string logo = _resourceML.GetLogoURL(param.WID).ToString();
                            string Footer = "<p><h4 style='color:#000000;font-family:verdana,sans-serif;margin-bottom:1.5px'><em>{CompanyName}</em></h4><span>{CompanyAddress}</span></p>";
                            Footer = Footer.Replace("{CompanyName}", param.Company).Replace("{CompanyAddress}", param.CompanyAddress);
                            IsSent = emailManager.SendEMail(mailSetting, param.UserEmailID, null, mailSetting.Subject, EmailBody, param.WID, logo, true, Footer);
                        }
                    }
                }
                SendEmail sendEmail = new SendEmail
                {
                    From = mailSetting.FromEmail,
                    Body = EmailBody,
                    Recipients = param.UserEmailID + "," + (param.bccList != null ? (param.bccList.Count > 0 ? String.Join(",", param.bccList) : "") : ""),
                    Subject = mailSetting.Subject,
                    IsSent = IsSent,
                    WID = param.WID
                };
                EmailDL emailDL = new EmailDL(_dal);
                emailDL.SaveMail(sendEmail);
            }
            return _res;
        }
        public IResponseStatus SendEmail(AlertReplacementModel model)
        {
            throw new NotImplementedException();
        }

        public Task BulkWebNotification(AlertReplacementModel param)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus SendSocialAlert(AlertReplacementModel model, List<string> APIIDs)
        {
            throw new NotImplementedException();
        }       

        public IResponseStatus FundTransferSMS(AlertReplacementModel param)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus FundTransferEmail(AlertReplacementModel param)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus FundTransferNotification(AlertReplacementModel param)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus FundReceiveSMS(AlertReplacementModel param)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus FundReceiveEmail(AlertReplacementModel param)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus FundReceiveNotification(AlertReplacementModel param)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus FundCreditSMS(AlertReplacementModel param)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus FundCreditEmail(AlertReplacementModel param)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus FundCreditNotification(AlertReplacementModel param)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus FundDebitEmail(AlertReplacementModel param)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus FundDebitNotification(AlertReplacementModel param)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus OTPEmail(AlertReplacementModel param)
        {
            throw new NotImplementedException();
        }

        public bool ForgetPasswordEmail(AlertReplacementModel param)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus OperatorUPSMS(AlertReplacementModel param, bool IsUp)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus OperatorUpEmail(AlertReplacementModel param, bool IsUp)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus OperatorUpNotification(AlertReplacementModel param, bool IsUp, bool IsBulk)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus FundOrderSMS(AlertReplacementModel param)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus FundOrderEmail(AlertReplacementModel param)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus FundOrderNotification(AlertReplacementModel param)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus KYCApprovalSMS(AlertReplacementModel param, bool IsApproved)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus KYCApprovalEmail(AlertReplacementModel param, bool IsApproved)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus KYCApprovalNotification(AlertReplacementModel param, bool IsApproved)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus RecharegeSuccessSMS(AlertReplacementModel param, bool IsSuccess)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus RecharegeSuccessEmail(AlertReplacementModel param, bool IsSuccess)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus RecharegeSuccessNotification(AlertReplacementModel param, bool IsSuccess)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus RechargeRefundSMS(AlertReplacementModel param, bool IsRejected)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus RechargeRefundEmail(AlertReplacementModel param, bool IsRejected)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus RechargeRefundNotification(AlertReplacementModel param, bool IsRejected)
        {
            throw new NotImplementedException();
        }

        public Task LowBalanceSMS(AlertReplacementModel param, SMSSetting smsSetting)
        {
            throw new NotImplementedException();
        }

        public Task LowBalanceEmail(AlertReplacementModel param, EmailSettingswithFormat mailSetting)
        {
            throw new NotImplementedException();
        }

        public Task LowBalanceNotification(AlertReplacementModel param)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus RecharegeAcceptSMS(AlertReplacementModel param)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus RecharegeAcceptEmail(AlertReplacementModel param)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus RecharegeAcceptNotification(AlertReplacementModel param)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus CallNotPickedSMS(AlertReplacementModel param)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus CallNotPickedEmail(AlertReplacementModel param)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus CallNotPickedNotification(AlertReplacementModel param)
        {
            throw new NotImplementedException();
        }

        public Task UserPartialApprovalSMS(AlertReplacementModel param)
        {
            throw new NotImplementedException();
        }

        public Task UserPartialApprovalEmail(AlertReplacementModel param)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus UserSubscription(AlertReplacementModel param)
        {
            throw new NotImplementedException();
        }

        public Task MarginRevisedSMS(AlertReplacementModel param)
        {
            throw new NotImplementedException();
        }

        public Task MarginRevisedEmail(AlertReplacementModel param)
        {
            throw new NotImplementedException();
        }

        public Task MarginRevisedNotification(AlertReplacementModel param, bool IsBulk)
        {
            throw new NotImplementedException();
        }

        public Task WebNotification(AlertReplacementModel param)
        {
            throw new NotImplementedException();
        }      

        public IResponseStatus BirthdayWishSMS(AlertReplacementModel param, SMSSetting smsSetting)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus BirthdayWishEmail(AlertReplacementModel param, EmailSettingswithFormat mailSetting)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus BirthdayWishNotification(AlertReplacementModel param)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus BBPSSuccessSMS(AlertReplacementModel param)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus BBPSComplainRegistrationAlert(AlertReplacementModel param)
        {
            throw new NotImplementedException();
        }

        public Task SocialAlert(AlertReplacementModel param)
        {
            throw new NotImplementedException();
        }
    }   
}