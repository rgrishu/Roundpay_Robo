using Roundpay_Robo.AppCode.Configuration;
using Roundpay_Robo.AppCode.DB;

using Roundpay_Robo.AppCode.HelperClass;
using Roundpay_Robo.AppCode.Interfaces;
using Roundpay_Robo.AppCode.Model;
using Roundpay_Robo.AppCode.StaticModel;


using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Roundpay_Robo;
using Roundpay_Robo.AppCode;
using Roundpay_Robo.AppCode.Configuration;

using Roundpay_Robo.AppCode.HelperClass;
using Roundpay_Robo.AppCode.Interfaces;

using Roundpay_Robo.AppCode.Model;

using Roundpay_Robo.AppCode.Model.ProcModel;
using Roundpay_Robo.AppCode.StaticModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Validators;
using Roundpay_Robo.AppCode.Interfaces;
using Roundpay_Robo.AppCode.Model.App;
using Roundpay_Robo.AppCode.DL;
using Roundpay_Robo.AppCode.MiddleLayer;
using Roundpay_Robo.AppCode.WebRequest;
using Roundpay_Robo.AppCode.DL;

namespace Roundpay_Robo.AppCode
{
    public class LoginML : ILoginML
    {
        #region Global Varibale Declaration
        private readonly IConfiguration Configuration;
        private readonly IHttpContextAccessor _accessor;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly IRequestInfo _rinfo;
        private readonly WebsiteInfo _WInfo;
        private readonly IResourceML _resourceML;
        private string IPGeoDetailURL = "http://api.ipstack.com/{IP}?access_key={Access_Key}";
        #endregion

        public LoginML(IHttpContextAccessor accessor, Microsoft.AspNetCore.Hosting.IHostingEnvironment env, bool IsInSession = true)
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
                _WInfo = GetWebsiteInfo();
            }
            //_resourceML = new ResourceML(_accessor, _env);
        }
        public WebsiteInfo GetWebsiteInfo()
        {
            string domain = _rinfo.GetDomain(Configuration);
            var _wi = _session.GetObjectFromJson<WebsiteInfo>(SessionKeys.WInfo);
            bool IsCall = true;
            if (_wi != null)
            {
                if (_wi.WebsiteName == domain && _wi.WID > 0)
                {
                    IsCall = false;
                }
            }
            if (IsCall)
            {
                ProcGetWebsiteInfo procGetWebsiteInfo = new ProcGetWebsiteInfo(_dal);
                _wi = (WebsiteInfo)procGetWebsiteInfo.Call(new CommonReq { CommonStr = domain });
                _session.SetObjectAsJson(SessionKeys.WInfo, _wi);
            }
            var cInfo = _rinfo.GetCurrentReqInfo();
            _wi.AbsoluteHost = cInfo.Scheme + "://" + cInfo.Host + (cInfo.Port > 0 ? ":" + cInfo.Port : "");
            return _wi;
        }

        public LoginML(IHttpContextAccessor accessor, Microsoft.AspNetCore.Hosting.IHostingEnvironment env, string domain)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _rinfo = new RequestInfo(_accessor, _env);
            bool IsProd = _env.IsProduction();
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((IsProd ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _WInfo = GetWebsiteInfo(domain);
            //_resourceML = new ResourceML(_accessor, _env);
        }
        public ResponseStatus DoLogin(LoginDetail loginDetail)
        {
            loginDetail.RequestIP = _rinfo.GetRemoteIP();
            loginDetail.Browser = _rinfo.GetBrowserFullInfo();
         //   loginDetail.UserAgent = _rinfo.GetUserAgentMD5();
            loginDetail.WID = _WInfo.WID;
            var responseStatus = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidLogin,
            };
            if (loginDetail.WID < 0)
            {
                responseStatus.Msg = ErrorCodes.NotRecogRouteLogin;
                return responseStatus;
            }
            var _p = new ProcLogin(_dal);
            var _lr = (LoginResponse)_p.Call(loginDetail);
            _lr.LoginTypeID = loginDetail.LoginTypeID;
            _lr.LoginType = LoginType.GetLoginType(loginDetail.LoginTypeID);
            responseStatus.Msg = _lr.Msg;
            int wId = _lr.WID <= 0 ? loginDetail.WID : _lr.WID;
            if (_lr.ResultCode < 1)
            {
                if (_lr.UserID > 0)
                {
                    var resCheckInvalidAttempt = CheckInvalidAttempt(_lr.LoginTypeID, _lr.UserID, true, false, true, loginDetail.CommonStr);
                    if (resCheckInvalidAttempt.Statuscode == ErrorCodes.Minus1)
                    {
                        if ((resCheckInvalidAttempt.CommonStr ?? "").Trim() != "")
                        {
                            //Task.Factory.StartNew(() => SendLoginAlert(string.Empty, _lr.LoginTypeID, _lr.UserID, loginDetail.RequestIP, loginDetail.Browser, wId, _lr.RoleID, true));
                           // SendLoginAlert(string.Empty, _lr.LoginTypeID, _lr.UserID, loginDetail.RequestIP, loginDetail.Browser, wId, _lr.RoleID, true);
                        }
                        responseStatus.Msg = resCheckInvalidAttempt.Msg;
                        responseStatus.CommonBool = _lr.RoleID == Role.Admin ? true : false;
                    }
                }
                return responseStatus;
            }
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin)
            {
                if (!ValidateProject())
                {
                    responseStatus.Statuscode = ErrorCodes.Minus1;
                    responseStatus.Msg = ErrorCodes.PROJEXPIRE;
                    return responseStatus;
                }
            }
            //if (_lr.LoginTypeID == LoginType.CustomerCare)
            //{
            //    ICustomercareML customercareML = new CustomercareML(_accessor, _env);
            //    _lr.operationsAssigned = customercareML.GetOperationAssigneds(_lr.RoleID);
            //}
            if (_lr.LoginTypeID == LoginType.Employee)
                _session.SetObjectAsJson(SessionKeys.LoginResponseEmp, _lr);
            else
                _session.SetObjectAsJson(SessionKeys.LoginResponse, _lr);
            //if (!string.IsNullOrEmpty(_lr.OTP))
            //{
            //    IUserML uml = new UserML(_accessor, _env);
            //    var alertData = uml.GetUserDeatilForAlert(_lr.UserID);
            //    alertData.FormatID = MessageFormat.OTP;
            //    if (alertData.Statuscode == ErrorCodes.One)
            //    {
            //        IAlertML alertMl = new AlertML(_accessor, _env);
            //        Parallel.Invoke(() => alertData.OTP = _lr.OTP,
            //        () => alertMl.OTPSMS(alertData),
            //        () => alertMl.OTPEmail(alertData),
            //        () => alertMl.SocialAlert(alertData));
            //    }
            //    responseStatus.Statuscode = LoginResponseCode.OTP;
            //    responseStatus.Msg = "Enter OTP!";
            //    if (ApplicationSetting.IsRPOnly && _lr.RoleID == Role.Admin)
            //    {
            //      //  SendLoginAlert(string.Empty, _lr.LoginTypeID, _lr.UserID, loginDetail.RequestIP, loginDetail.Browser, wId, _lr.RoleID);
            //        //Task.Factory.StartNew(() => SendLoginAlert(string.Empty, _lr.LoginTypeID, _lr.UserID, loginDetail.RequestIP, loginDetail.Browser, wId, _lr.RoleID));
            //    }
            //    return responseStatus;
            //}
            if (_lr.IsGoogle2FAEnable && !_lr.IsDeviceAuthenticated)
            {
                responseStatus.Statuscode = LoginResponseCode.Google2FAEnabled;
                responseStatus.Msg = "Please Enter Google Authenticator PIN";
                if (ApplicationSetting.IsRPOnly && _lr.RoleID == Role.Admin)
                {
                   // SendLoginAlert(string.Empty, _lr.LoginTypeID, _lr.UserID, loginDetail.RequestIP, loginDetail.Browser, wId, _lr.RoleID);
                    //Task.Factory.StartNew(() => SendLoginAlert(string.Empty, _lr.LoginTypeID, _lr.UserID, loginDetail.RequestIP, loginDetail.Browser, wId, _lr.RoleID));
                }
                return responseStatus;
            }
            _session.SetString(SessionKeys.AppSessionID, _lr.SessionID);
            CookieHelper cookie = new CookieHelper(_accessor);
            byte[] SessionID = Encoding.ASCII.GetBytes(_lr.SessionID);
            cookie.Set(SessionKeys.AppSessionID, Base64UrlTextEncoder.Encode(SessionID), _lr.CookieExpire);
            responseStatus.Statuscode = LoginResponseCode.SUCCESS;
            responseStatus.Msg = "Login successfull!";
            responseStatus.CommonStr = GetDashboardPath(_lr);
            //if (ApplicationSetting.IsRPOnly && _lr.RoleID == Role.Admin)
            //{
            //    Task.Factory.StartNew(() => SendLoginAlert(string.Empty, _lr.LoginTypeID, _lr.UserID, loginDetail.RequestIP, loginDetail.Browser, wId, _lr.RoleID));
            //}
            return responseStatus;
        }

        public bool IsInValidSession()
        {
            string AppSessionID = _session.GetString(SessionKeys.AppSessionID);
            return (string.IsNullOrEmpty(AppSessionID) || AppSessionID.Length != Numbers.THIRTY_TWO);
        }
        public ResponseStatus CheckInvalidAttempt(int LT, int UserID, bool IsInvalidLoginAttempt, bool IsInvalidOTPAttempt, bool InCheck, string IMEI)
        {
            string IP = _rinfo.GetRemoteIP();
            var req = new CommonReq
            {
                LoginTypeID = LT,
                LoginID = UserID,
                CommonBool = IsInvalidLoginAttempt,
                CommonBool1 = IsInvalidOTPAttempt,
                CommonBool2 = InCheck,
                CommonStr = IMEI ?? "",
                CommonStr2 = IP ?? ""
            };
            IProcedure _proc = new ProcInvalidAttempt(_dal);
            return (ResponseStatus)_proc.Call(req);
        }
        private bool ValidateProject()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("http://helpdesk.mrupay.in/Checklogin.asmx/AuthenticateProjectLogin?ProjectId={0}&DomainName={1}", ApplicationSetting.ProjectID, _rinfo.GetDomain(Configuration));

            var res = "";
            Authentication authentication = new Authentication();
            try
            {
                res = AppWebRequest.O.CallUsingHttpWebRequest_GET(sb.ToString());
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "ValidateProject",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                return true;
                //Based on assumption that API is not responding so service will continue.
            }
            try
            {
                if ((res ?? "").Trim() != "")
                {
                    authentication = XMLHelper.O.DesrializeToObject(authentication, res, "Authentication");
                    if (authentication.STATUS == ErrorCodes.One)
                    {
                        IProcedure ProcProjectValidate = new ProcProjectValidate(_dal);
                        var resUpdate = (ResponseStatus)ProcProjectValidate.Call(authentication);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "ValidateProject",
                    Error = "(ResponseMatch)Ex:" + ex.Message + " | Res:" + res,
                    LoginTypeID = 1,
                    UserId = 1
                });
            }

            return false;
        }
        private string GetDashboardPath(LoginResponse _lr)
        {
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if (_lr.RoleID == Role.Admin)
                    return "Admin";
                else if (_lr.RoleID == Role.Retailor_Seller)
                {
                            return "Seller";
                }


                else if (_lr.RoleID == Role.APIUser)
                    return "APIUser";
                else if (_lr.RoleID == Role.Customer)
                    return "Customer";
                else if (_lr.RoleID == Role.MasterWL)
                    return "MasterWL";
                else if (_lr.RoleID == Role.FOS)
                    return "FOS";
                else if (_lr.RoleID > 0)
                    return "User";
            }
            else if (_lr.LoginTypeID == LoginType.CustomerCare)
            {
                return "CustomerCare";
            }
            else if (_lr.LoginTypeID == LoginType.Employee)
            {
                return "Employee";
            }
            return string.Empty;
        }
        public WebsiteInfo GetWebsiteInfo(string domain)
        {
            var _wi = new WebsiteInfo
            {
                WID = 999999999,//Fixed for WHITELABEL unknown from app
                WebsiteName = domain
            };
            if (!domain.Equals("WHITELABEL"))
            {
                ProcGetWebsiteInfo procGetWebsiteInfo = new ProcGetWebsiteInfo(_dal);
                _wi = (WebsiteInfo)procGetWebsiteInfo.Call(new CommonReq { CommonStr = domain });
            }
            return _wi;
        }
        public WebsiteInfo GetWebsiteInfo(int WID)
        {
            ProcGetWebsiteInfo procGetWebsiteInfo = new ProcGetWebsiteInfo(_dal);
            return (WebsiteInfo)procGetWebsiteInfo.Call(new CommonReq { CommonInt = WID });
        }
        public async Task<IResponseStatus> DoLogout(LogoutReq logoutReq)
        {
            logoutReq.IP = _rinfo.GetRemoteIP();
            logoutReq.Browser = _rinfo.GetBrowserFullInfo();
            var resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            IProcedureAsync procLogout = new ProcLogout(_dal);
            resp = (ResponseStatus)await procLogout.Call(logoutReq);
            return resp;
        }
    }
}