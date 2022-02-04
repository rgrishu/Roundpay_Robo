using Roundpay_Robo.AppCode.Model;
using Microsoft.AspNetCore.Mvc;
using Roundpay_Robo.AppCode.Configuration;
using Roundpay_Robo.AppCode.Interfaces;
using Roundpay_Robo.AppCode.StaticModel;
using Roundpay_Robo.AppCode.Configuration;
using Validators;
using Roundpay_Robo.AppCode.HelperClass;
using Roundpay_Robo.AppCode;
using Roundpay_Robo.AppCode.Model.ProcModel;
using Roundpay_Robo.Models;
using Roundpay_Robo.AppCode.MiddleLayer;
using Roundpay_Robo.AppCode.MiddleLayer;

namespace Roundpay_Robo.Controllers
{
    public class LoginController : Controller
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _env;
        private readonly ILoginML _loginML;
        public LoginController(IHttpContextAccessor accessor, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _loginML = new LoginML(_accessor, _env);
        }
        public IActionResult Index()
        {
            var WebInfo = _loginML.GetWebsiteInfo();
            UserML userML = new UserML(_accessor, _env);
            var Cprofile = userML.GetCompanyProfileApp(WebInfo.WID);
            var loginPageModel = new LoginPageModel
            {
                WID = WebInfo.WID,
                Host = WebInfo.AbsoluteHost,
                ThemeID = WebInfo.ThemeId,
                AppName = Cprofile.AppName,
                CustomerCareMobileNos = Cprofile.CustomerCareMobileNos,
                CustomerPhoneNos = Cprofile.CustomerPhoneNos
            };
            return View(loginPageModel);
        }
        [HttpPost]
        [Route("/Login")]
        public IActionResult LoginCheck([FromBody] LoginDetail loginDetail)
        {
            loginDetail.LoginTypeID = 1;
            IResponseStatus responseStatus = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidLogin
            };
            if (loginDetail == null)
            {
                return Json(responseStatus);
            }
            if (!loginDetail.LoginTypeID.In(LoginType.ApplicationUser, LoginType.CustomerCare, LoginType.Employee))
            {
                responseStatus.Msg = "Choose user login type!";
                return Json(responseStatus);
            }
            if (ApplicationSetting.WithCustomLoginID && loginDetail.LoginTypeID == LoginType.ApplicationUser)
            {
                loginDetail.Prefix = string.Empty;
                loginDetail.CommonStr2 = loginDetail.LoginMobile;
                loginDetail.LoginMobile = string.Empty;
            }
            else if (!Validate.O.IsMobile(loginDetail.LoginMobile))
            {
                loginDetail.Prefix = Validate.O.Prefix(loginDetail.LoginMobile);
                if (Validate.O.IsNumeric(loginDetail.Prefix))
                    return Json(responseStatus);
                string loginID = Validate.O.LoginID(loginDetail.LoginMobile);
                if (!Validate.O.IsNumeric(loginID))
                {
                    return Json(responseStatus);
                }
                loginDetail.LoginID = Convert.ToInt32(loginID);
                loginDetail.LoginMobile = "";
            }
            //IRequestInfo _rinfo = new RequestInfo(_accessor, _env);
            //if (_rinfo.GetCurrentReqInfo().Scheme.Equals("https") && (loginDetail.Longitude == 0 || loginDetail.Latitude == 0))
            //{
            //    responseStatus.Statuscode = -2;
            //    responseStatus.Msg = "Please allow location first";
            //    return Json(responseStatus);
            //}
            loginDetail.RequestMode = RequestMode.PANEL;
            loginDetail.Password = HashEncryption.O.Encrypt(loginDetail.Password);
            responseStatus = _loginML.DoLogin(loginDetail);
            return Json(new { responseStatus.Statuscode, responseStatus.Msg, Path = responseStatus.CommonStr, IsBrowserBlock = responseStatus.CommonBool });
        }




        [HttpGet]
        [Route("/Signup")]
        public IActionResult SignUp(int rid)
        {
            if (!ApplicationSetting.IsSingupPageOff)
            {
                rid = rid < 1 ? 1 : rid;
                ISettingML _settingML = new SettingML(_accessor, _env);
                var WebInfo = _loginML.GetWebsiteInfo();
                UserML userML = new UserML(_accessor, _env);
                var Cprofile = userML.GetCompanyProfileApp(WebInfo.WID);
                var loginPageModel = new LoginPageModel
                {
                    WID = WebInfo.WID,
                    Host = WebInfo.AbsoluteHost,
                    ThemeID = WebInfo.ThemeId,
                    AppName = Cprofile.AppName,
                    CustomerCareMobileNos = Cprofile.CustomerCareMobileNos,
                    CustomerPhoneNos = Cprofile.CustomerPhoneNos,
                    referralRoleMaster = new ReferralRoleMaster
                    {
                        ReferralID = rid,
                        Roles = _settingML.GetRoleForReferral(rid)
                    }
                };
                if (loginPageModel.ThemeID == 4)
                {
                    IBannerML bannerML = new ResourceML(_accessor, _env);
                    // loginPageModel.BGServiceImgURLs = bannerML.SiteGetServices(loginPageModel.WID, loginPageModel.ThemeID);
                }
                return View("SignUp", loginPageModel);
            }
            return Ok();
        }


        [HttpPost]
        [Route("Signup")]
        public IActionResult Signup([FromBody] UserCreate UserCreate)
        {
            if (!ApplicationSetting.IsSingupPageOff)
            {
                IUserML _UserML = new UserML(_accessor, _env);
                ResponseStatus _resp = _UserML.CallSignup(UserCreate);
                return Json(_resp);
            }
            return Ok();
        }
    }
}
