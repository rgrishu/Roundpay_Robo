using Roundpay_Robo.AppCode;
using Roundpay_Robo.AppCode.Configuration;
using Roundpay_Robo.AppCode.DB;
using Roundpay_Robo.AppCode.HelperClass;
using Roundpay_Robo.AppCode.Interfaces;
using Roundpay_Robo.AppCode.Model;

using Roundpay_Robo.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Roundpay_Robo.AppCode.Configuration;
using Roundpay_Robo.AppCode.DL;
using Roundpay_Robo.AppCode.HelperClass;
using Roundpay_Robo.AppCode.Interfaces;

using Roundpay_Robo.AppCode.Model;
using Roundpay_Robo.AppCode.Model.App;

using Roundpay_Robo.AppCode.Model.ProcModel;
using Roundpay_Robo.AppCode.StaticModel;

using Roundpay_Robo.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Validators;
using Roundpay_Robo.AppCode.MiddleLayer;
using Roundpay_Robo.AppCode.DL;

namespace Roundpay_Robo.AppCode
{

    public class UserML : IUserML
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
        private readonly IResourceML _resourceML;
        private readonly LoginResponse _lrEmp;

        #region DashboardRelated
        //public Dashboard DashBoard()
        //{
        //    IProcedure f = new ProcDashBoardDetail(_dal);
        //    return (Dashboard)f.Call();
        //}
        #endregion
        public UserML(IHttpContextAccessor accessor, Microsoft.AspNetCore.Hosting.IHostingEnvironment env, bool IsInSession = true)
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
                _lrEmp = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponseEmp);
            }
            _resourceML = new ResourceML(_accessor, _env);

        }
        public UserML(LoginResponse lr)
        {
            _lr = lr;
        }
        public CompanyProfileDetail GetCompanyProfileApp(int WID)
        {
            IProcedure proc = new ProcCompanyProfile(_dal);
            return (CompanyProfileDetail)proc.Call(WID);
        }
        public ResponseStatus CallSignup(UserCreate _req)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if (_WInfo.WID > 0)
            {
                if (_req.RoleID < 2)
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Role";
                    return _resp;
                }

                if (Validate.O.IsNumeric(_req.Name ?? "") || (_req.Name ?? "").Length > 100)
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Name";
                    return _resp;
                }
                if (Validate.O.IsNumeric(_req.OutletName ?? "") || (_req.OutletName ?? "").Length > 100)
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Outlet Name";
                    return _resp;
                }
                if (!Validate.O.IsMobile(_req.MobileNo ?? ""))
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Mobile Number";
                    return _resp;
                }
                if (string.IsNullOrEmpty(_req.EmailID) || !_req.EmailID.Contains("@") || !_req.EmailID.Contains("."))
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " EmailID";
                    return _resp;
                }
                if (!Validate.O.IsPinCode(_req.Pincode ?? ""))
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Pincode";
                    return _resp;
                }
                _req.RequestModeID = RequestMode.PANEL;
                _req.WID = _WInfo.WID;
                _req.Browser = _rinfo.GetBrowserFullInfo();
                _req.IP = _rinfo.GetRemoteIP();
                _req.Password = ApplicationSetting.IsPasswordNumeric ? HashEncryption.O.CreatePasswordNumeric(8) : HashEncryption.O.CreatePassword(8);
                _req.Pin = HashEncryption.O.CreatePasswordNumeric(4);
                IProcedure _p = new ProcSignup(_dal);
                var res = (AlertReplacementModel)_p.Call(_req);
                _resp.Msg = res.Msg;
                if (res.Statuscode == ErrorCodes.One)
                {
                    _resp.Statuscode = res.Statuscode;

                   // if (_req.IsWebsite && res.WID > 0)
                   // {
                   //     _resourceML.CreateWebsiteDirectory(res.WID, FolderType.Website);
                   // }
                   // IAlertML alertMl = new AlertML(_accessor, _env);
                   // Parallel.Invoke(() => alertMl.RegistrationSMS(res),
                   //() => alertMl.RegistrationEmail(res),
                   //() => alertMl.SocialAlert(res));
                }
            }
            return _resp;
        }

      


    }
}

