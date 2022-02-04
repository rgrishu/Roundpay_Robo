using Roundpay_Robo.Services;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Roundpay_Robo.AppCode;
using Roundpay_Robo.AppCode.Configuration;
using Roundpay_Robo.AppCode.Interfaces;
using Roundpay_Robo.AppCode.Model;
using Roundpay_Robo.AppCode.StaticModel;
using System.Data;

namespace Roundpay_Robo.Controllers
{
    public class AdminController : Controller
    {
        private readonly IDapper _dapper;
        private readonly IHttpContextAccessor _accessor;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly LoginResponse _lr;
        private readonly ILoginML loginML;

        public AdminController(IHttpContextAccessor accessor, Microsoft.AspNetCore.Hosting.IHostingEnvironment env, IDapper dapper)
        {
            _accessor = accessor;
            _env = env;
            _dapper = dapper;
            _session = _accessor.HttpContext.Session;
            _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
         
            loginML = new LoginML(_accessor, _env);
        }
        public IActionResult Index()
        {
            if (loginML.IsInValidSession())
            {
                return RedirectToAction("Index", "Login");
            }
            if (_lr.RoleID == Role.Admin && LoginType.ApplicationUser == _lr.LoginTypeID)
                return View();
            return Ok();
        }
        [HttpPost]
        [Route("Login/Logout")]
        [Route("Logout")]
        public async Task<IActionResult> Logout(int ULT, int UserID, int SType)
        {
            if (loginML.IsInValidSession())
            {
                return RedirectToAction("Index", "Login");
            }
            IResponseStatus responseStatus = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.RoleID > 0)
            {
                LogoutReq logoutReq = new LogoutReq
                {
                    LT = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    ULT = ULT == 0 ? _lr.LoginTypeID : ULT,
                    UserID = UserID == 0 ? _lr.UserID : UserID,
                    SessID = _lr.SessID,
                    SessionType = SType == 0 ? SessionType.Single : SType,
                    RequestMode = RequestMode.PANEL
                };
                ILoginML loginML = new LoginML(_accessor, _env);
                responseStatus = await loginML.DoLogout(logoutReq);
                if (ClearCurrentSession())
                {
                    return Json(responseStatus);
                }
                else
                {
                    return Json(responseStatus);
                }
            }
            return Json(responseStatus);
        }
        private bool ClearCurrentSession()
        {
            try
            {
                HttpContext.Session.Clear();
                CookieHelper cookie = new CookieHelper(_accessor);
                cookie.Remove(SessionKeys.AppSessionID);
                return true;
            }
            catch (Exception)
            {
            }
            return false;
        }
        [HttpGet]
        [Route("Vendor")]
        public IActionResult VendorMaster()
        {
            if (loginML.IsInValidSession())
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }
        [HttpPost]
        [Route("VendorAdd")]
        public IActionResult AddVendor(VendorMaster vendormaster)
        {
            return PartialView("PartialView/_VendorMaster", vendormaster);
        }
        [HttpPost]
        [Route("SaveVendor")]
        public async  Task<IActionResult> SaveVendorDetail(VendorMaster vendormaster)
        {
            ILapuML _lml = new LapuML(_accessor,_env,_dapper);
            return Json(_lml.SaveVendor(vendormaster, _lr).Result);
        }

        [HttpPost]
        [Route("VendorList")]
        public IActionResult GetVendorList()
        {
                ILapuML _lml = new LapuML(_accessor, _env, _dapper);
                var res = _lml.GetVendorList(_lr).Result;
                return PartialView("PartialView/_VendorMasterList", res);
        }

    }
}
