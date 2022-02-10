using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Roundpay_Robo.AppCode.Configuration;
using Roundpay_Robo.AppCode.DB;
using Roundpay_Robo.AppCode.DL;
using Roundpay_Robo.AppCode.Interfaces;
using Roundpay_Robo.AppCode.Model;
using Roundpay_Robo.AppCode.StaticModel;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class BaseML
    {
        #region Gloabl Variables

        protected readonly IHttpContextAccessor _accessor;
        protected readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _env;
        protected readonly IDAL _dal;
        protected readonly IConnectionConfiguration _c;
        protected readonly ISession _session;
        protected readonly LoginResponse _lr;
        #endregion
        public BaseML(IHttpContextAccessor accessor, Microsoft.AspNetCore.Hosting.IHostingEnvironment env, bool IsInSession = true)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            if (IsInSession)
            {
                _session = _accessor.HttpContext.Session;
                _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
            }
            else
            {
                _lr = new LoginResponse();
            }
        }
        private void SaveSMSResponse(Roundpay_Robo.AppCode.MiddleLayer.SMSResponse Response)
        {
            IProcedure _p = new ProcSaveSMSResponse(_dal);
            object _o = _p.Call(Response);
        }
    }
}