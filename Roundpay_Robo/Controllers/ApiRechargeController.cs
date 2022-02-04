using Microsoft.AspNetCore.Mvc;
using Roundpay_Robo.AppCode;
using Roundpay_Robo.AppCode.Interfaces;
using Roundpay_Robo.Models;
using Roundpay_Robo.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Roundpay_Robo.Controllers
{

  




    public class ApiRechargeController : ControllerBase
    {


        private readonly IHttpContextAccessor _accessor;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _env;
        private readonly IDapper _dapper;
        public ApiRechargeController(IHttpContextAccessor accessor, Microsoft.AspNetCore.Hosting.IHostingEnvironment env, IDapper dapper)
        {
            _accessor = accessor;
            _env = env;
            _dapper = dapper;
        }

        [HttpGet]
        [Route("api/LapuRecharge")]
        public async Task<IActionResult> LapuRecharge(LapuRechargeRequest req)
        {
            ILapuML ml = new LapuML(_accessor, _env, _dapper);
            var res = ml.LappuApiRecharge(req).Result;
            return Ok(res);
        }
    }
}
