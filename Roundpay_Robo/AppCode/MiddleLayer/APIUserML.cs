using Roundpay_Robo.AppCode.Configuration;
using Roundpay_Robo.AppCode.DB;
using Roundpay_Robo.AppCode.HelperClass;
using Roundpay_Robo.AppCode.Interfaces;
using Roundpay_Robo.AppCode.Model;
using Roundpay_Robo.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Roundpay_Robo.AppCode.DL;
using Roundpay_Robo.AppCode.Interfaces;

using Roundpay_Robo.AppCode.Model;
using Roundpay_Robo.AppCode.Model.ProcModel;
using Roundpay_Robo.AppCode.Model.Recharge;
using System;
using System.Linq;
using System.Threading.Tasks;
using Validators;

namespace Roundpay_Robo.AppCode.MiddleLayer
{
    public sealed partial class APIUserML : IAPIUserMiddleLayer
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _env;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly IRequestInfo _info;
        public APIUserML(IHttpContextAccessor accessor, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _info = new RequestInfo(_accessor, _env);
        }

        public async Task SaveAPILog(APIReqResp aPIReqResp)
        {
            IProcedureAsync _proc = new ProcLogAPIUserReqResp(_dal);
            await _proc.Call(aPIReqResp).ConfigureAwait(false);
        }




    }
}
