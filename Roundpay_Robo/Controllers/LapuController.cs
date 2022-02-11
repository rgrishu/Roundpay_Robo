using Microsoft.AspNetCore.Mvc;
using Roundpay_Robo.AppCode;
using Roundpay_Robo.AppCode.Configuration;
using Roundpay_Robo.AppCode.Interfaces;
using Roundpay_Robo.AppCode.Model;
using Roundpay_Robo.AppCode.StaticModel;
using Roundpay_Robo.Models;
using Roundpay_Robo.Services;

namespace Roundpay_Robo.Controllers
{
    public class LapuController : Controller
    {
        private readonly IDapper _dapper;
        private readonly IHttpContextAccessor _accessor;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly LoginResponse _lr;
        private readonly ILoginML loginML;
        public LapuController(IHttpContextAccessor accessor, Microsoft.AspNetCore.Hosting.IHostingEnvironment env, IDapper dapper)
        {
            _accessor = accessor;
            _env = env;
            _dapper = dapper;
            _session = _accessor.HttpContext.Session;
            _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
            loginML = new LoginML(_accessor, _env);
        }
        [HttpGet]
        public IActionResult Index()
        {
            if (loginML.IsInValidSession())
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }
        [HttpPost]
        [Route("LapuAdd")]
        public IActionResult AddLApu()
        {
            ILapuML _lml2 = new LapuML(_accessor, _env, _dapper);
            var res1 = _lml2.GetServices(_lr).Result;
            ILapuML _lml = new LapuML(_accessor, _env, _dapper);
            var res = _lml.GetVendorLapu(_lr).Result;
            SeviceVendorVM model = new SeviceVendorVM();
            model.listLAPU = res;
            model.listLapuServices = res1;

            return PartialView("PartialView/_AddLapu", model);
        }
        [HttpPost]
        [Route("LapuList")]
        public IActionResult GetLapuList()
        {
            ILapuML _lml = new LapuML(_accessor, _env, _dapper);
            var res = _lml.GetLapuList(_lr).Result;
            return PartialView("PartialView/_LapuList", res);
        }

        [HttpPost]
        [Route("LapuLogin")]
        public IActionResult LapuLogin(LapuLoginRequest lapuloginreq, int lapuid)
        {
            if (loginML.IsInValidSession())
            {
                return RedirectToAction("Index", "Login");
            }
            ILapuML _lml = new LapuML(_accessor, _env, _dapper);
            var res = _lml.LapuLogin(lapuloginreq, _lr, lapuid).Result;
            if (res.StatusCode == ErrorCodes.Two)
            {
                return PartialView("PartialView/_ValidateOTP");
            }
            else
            {
                return Json(res);
            }
        }
        [HttpPost]
        [Route("LapuBalance")]
        public IActionResult LapuBalance(LapuLoginRequest lapuloginreq, int lapuid)
        {
            if (loginML.IsInValidSession())
            {
                return RedirectToAction("Index", "Login");
            }
            ILapuML _lml = new LapuML(_accessor, _env, _dapper);
            var res = _lml.LapuBalance(lapuloginreq, _lr, lapuid).Result;
            return Json(res);
        }
        [HttpPost]
        [Route("ValidateOTP")]
        public IActionResult ValidateOTP(ValidateLapuLoginOTP lapuloginotp, int lapuid)
        {
            if (loginML.IsInValidSession())
            {
                return RedirectToAction("Index", "Login");
            }
            ILapuML _lml = new LapuML(_accessor, _env, _dapper);
            var res = _lml.ValidateOtp(lapuloginotp, _lr, lapuid).Result;
            return Json(res);
        }
        [HttpPost]
        [Route("SaveLapuBtn")]
        public async Task<IActionResult> SaveLapu(Lapu LapuUserDetail)
        {
            ILapuML _lml = new LapuML(_accessor, _env, _dapper);
            return Json(_lml.SaveLapu(LapuUserDetail, _lr));
        }
        [HttpPost]
        [Route("DeleteLapuDetail/{LapuID}")]
        public async Task<IActionResult> DeleteLapu(int LapuID)
        {
            ILapuML _lml = new LapuML(_accessor, _env, _dapper);
            return Json(_lml.DeleteLapu(LapuID, _lr));
        }
        [HttpGet]
        [Route("GetEditLapuDetail/{LapuID}")]
        public IActionResult GetEditLapuList(int LapuID)
        {
            ILapuML _lml = new LapuML(_accessor, _env, _dapper);
            var res = _lml.GetEditLapulist(LapuID, _lr).Result;
            return PartialView("PartialView/_Addlapu", res);
        }
        [HttpPost]
        [Route("UpdateLapuStatus/{LapuID}")]
        public IActionResult UpdatelapuStatus(int LapuID)
        {
            ILapuML _lml = new LapuML(_accessor, _env, _dapper);
            return Json(_lml.UpdateLapuStatus(LapuID, _lr));
        }
        //[HttpGet]
        //[Route("GetVendorLapu")]
        //public IActionResult GetVendorLapu()
        //{
        //    ILapuML _lml = new LapuML(_accessor, _env, _dapper);
        //    var res = _lml.GetVendorLapu(_lr).Result;
        //    return PartialView("PartialView/_AddLapu", res);
        //}


        [HttpPost]
        [Route("LapuTransactioData")]
        public IActionResult LapuTransactioData()
        {
            if (loginML.IsInValidSession())
            {
                return RedirectToAction("Index", "Login");
            }
            ILapuML _lml = new LapuML(_accessor, _env, _dapper);
           // var res = _lml.LapuBalance(lapuloginreq, _lr, lapuid).Result;




            return Ok();
            
           
        }
    }
}
