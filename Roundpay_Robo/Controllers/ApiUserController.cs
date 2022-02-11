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
    public class ApiUserController : Controller
    {
        private readonly IDapper _dapper;
        private readonly IHttpContextAccessor _accessor;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly LoginResponse _lr;
        private readonly ILoginML loginML;
        public ApiUserController(IHttpContextAccessor accessor, Microsoft.AspNetCore.Hosting.IHostingEnvironment env, IDapper dapper)
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
            if (_lr.RoleID == Role.APIUser && LoginType.ApplicationUser == _lr.LoginTypeID)
                return View();
            return Ok();

        }
        [Route("Report")]
        public IActionResult Report()
        {
            if (loginML.IsInValidSession())
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }
        [HttpPost]
        [Route("GetLapuReport")]
        public IActionResult GetLapuRport(LapuReport Filter)
        {
            ILapuML _lml = new LapuML(_accessor, _env, _dapper);
            var res = _lml.GetLapuReport(Filter, _lr).Result;
            return PartialView("PartialView/_LapuReport", res);
        }

        [HttpPost]
        [Route("GetReqRes/{TID}/{LapuID}")]
        public IActionResult GetReqRes(int TID, int LapuID)
        {
            ILapuML _lml = new LapuML(_accessor, _env, _dapper);
            var res = _lml.GetReqRes(TID, LapuID, _lr).Result;
            return PartialView("PartialView/_RequestResponse", res);
        }
        [HttpPost]
        [Route("UpdateTransactionStatus")]
        public async Task<IActionResult> LapuRechargeUpdate(LapuTransaction ltr)
        {
            if (loginML.IsInValidSession())
            {
                return RedirectToAction("Index", "Login");
            }
            ILapuML _lml = new LapuML(_accessor, _env, _dapper);
            ltr.UserID = _lr.UserID;
            var res = _lml.UpdateTransaction(ltr).Result;
            return Json(res);
        }
    }
}
