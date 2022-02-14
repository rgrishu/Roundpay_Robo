using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Roundpay_Robo.AppCode;
using Roundpay_Robo.AppCode.Interfaces;
using Roundpay_Robo.AppCode.MiddleLayer;
using Roundpay_Robo.AppCode.Model.Recharge;
using Roundpay_Robo.AppCode.StaticModel;
using Roundpay_Robo.Models;
using Roundpay_Robo.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Roundpay_Robo.Controllers
{
    public class ApiRechargeController : Controller
    {


        private readonly IHttpContextAccessor _accessor;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _env;
        private readonly IDapper _dapper;
        private readonly IAPIUserMiddleLayer _apiML;
        public ApiRechargeController(IHttpContextAccessor accessor, Microsoft.AspNetCore.Hosting.IHostingEnvironment env, IDapper dapper)
        {
            _accessor = accessor;
            _env = env;
            _dapper = dapper;
            _apiML = new APIUserML(_accessor, _env);
        }

        [HttpGet]
        [Route("api/LapuRecharge")]
        public async Task<IActionResult> LapuRecharge(LapuRechargeRequest req)
        {
            ILapuML ml = new LapuML(_accessor, _env, _dapper);
            LapuRechargeResponse res = ml.LappuApiRecharge(req).Result;
             string resp = JsonConvert.SerializeObject(res);
                await SaveReqResp(req.UserID, resp).ConfigureAwait(false);
            return Json(res);
        }

        [HttpGet]
        [Route("api/GetLapuTranData")]
        public async Task<IActionResult> LapuTransactions(string Account="")
        {
            ILapuML ml = new LapuML(_accessor, _env, _dapper);
            var res = ml.LapuTransactions().Result;
            string resp = JsonConvert.SerializeObject(res);
           // await SaveReqResp(req.UserID, resp).ConfigureAwait(false);
            return Json(res);
        }
        private async Task SaveReqResp(int UserID, string resp)
        {
            string req = "";
            var request = HttpContext.Request;
            if (request.Method == "POST")
            {
                string rbody = "";
                if (request.Body.CanSeek)
                    request.Body.Position = 0;
                using (var reader = new StreamReader(request.Body))
                {
                    rbody = await reader.ReadToEndAsync();
                }
                req = GetAbsoluteURI() + "?" + rbody;
            }
            else
            {
                req = GetAbsoluteURI() + request.QueryString.ToString();
            }
            var aPIReqResp = new APIReqResp
            {
                UserID = UserID,
                Request = req,
                Response = resp,
                Method = request.Method
            };
            await _apiML.SaveAPILog(aPIReqResp).ConfigureAwait(false);
        }
        private string GetAbsoluteURI()
        {
            var request = HttpContext.Request;
            return request.Scheme + "://" + request.Host + request.Path;
        }
    }
}
